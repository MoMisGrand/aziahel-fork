/*
 * Imlight
 * Copyright (C) 2025 Revive101
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.Game.Ultimus;
using Imlight.CoreLib.Shared.Resources;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// A dungeon's floors, the way a player plays them (Niduus, 2026-11-16: "Ultimus is not respecting floors. In order to TP
/// he has to complete the battles for that floor for the zone TP to unlock. This also has a puzzle in the dungeon ... he's
/// not doing any of the battles at all is the issue"). Read from the area's own files, shared by Ultimus and the Quest Team:
/// each room's battle circle (stepping on it starts the room's fight), each gate or door that opens later and what opens
/// it (a creature beaten, a spot stepped on, a lever pulled once a puzzle is right), and each puzzle plate with the state
/// it flips. <see cref="Next"/> gives the next floor duty near the wizard: the room's battle circle first, then the
/// creatures, then the spots and the puzzle, never a skip. Big Ben: every room's circle brings out the floor's elite whose
/// defeat opens the door by the teleporter up, and the plate room (Ice, Skull, Moon and Bird down) unlocks the lever.
/// </summary>
internal static class BotFloors {

    /// <summary>A room's battle circle: the trigger that starts its fight and the spot to step on.</summary>
    internal sealed record Circle(string Trigger, string Volume, float X, float Y, float Z = float.NaN);

    /// <summary>A puzzle plate: stepping on <see cref="Volume"/> turns <see cref="Object"/> from one state to another.</summary>
    internal sealed record Toggle(string Object, string From, string To, string Volume, float X, float Y, float Z = float.NaN);

    /// <summary>How a gate opens.</summary>
    internal enum Opener { Beat, Step, Click, Other }

    /// <summary>
    /// A gate or door that opens later: its trigger, its spot, what opens it, the spot to step on or the thing to click,
    /// and the object states a puzzle needs first.
    /// </summary>
    internal sealed record Gate(string Object, string Trigger, float X, float Y, Opener By, string Volume, float VX, float VY,
        string Lever, float LX, float LY, List<(string Object, string State)> Needs, string Through = "", float TX = float.NaN, float TY = float.NaN,
        float Z = float.NaN, float VZ = float.NaN, float LZ = float.NaN);

    /// <summary>What the area's files say about its floors.</summary>
    internal sealed record Book(List<Circle> Circles, List<Gate> Gates, List<Toggle> Toggles, Dictionary<string, string> Starts) {
        internal static readonly Book Empty = new([], [], [], new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        internal bool IsEmpty => Circles.Count == 0 && Gates.Count == 0;

        /// <summary>Every teleporter on (not one back or out), by its volume's name and spot.</summary>
        internal List<(string Name, float X, float Y)> Ahead { get; init; } = [];
    }

    /// <summary>One floor duty: step on a spot, fight by a spot, or click the lever there.</summary>
    internal sealed record Duty(string Kind, float X, float Y, string What, string Key, string Why);

    internal const string STEP = "step";
    internal const string FIGHT = "fight";
    internal const string CLICK = "click";

    // How near a room's things count as "this room" (Big Ben rooms are 6000 across with the circle in the middle).
    internal const float ROOM = 3200f;
    // How far a puzzle gate may be from the plates and lever that open it.
    private const float PUZZLE = 6500f;
    // Close enough to a spot to have stepped on it.
    internal const float ON_SPOT = 120f;
    // How near a door its teleporter on may be (Big Ben: the pad up stands 190 past each floor's door).
    private const float THROUGH = 900f;
    // Tries per spot or lever before it is left be.
    internal const int TRIES = 3;

    private static readonly ConcurrentDictionary<string, Book> s_books = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The area's floor book (empty for an area without battle circles or gates).</summary>
    internal static Book BookOf(string zonePath) {
        if (string.IsNullOrEmpty(zonePath)) {
            return Book.Empty;
        }

        return s_books.GetOrAdd(zonePath, path => {
            try {
                if (!ResourceManager.TryLoadArchive(path, out var wad)) {
                    return Book.Empty;
                }

                return Build(UltimusPads.Read<WizZoneTriggers>(wad.OpenFile("triggers.xml")?.ToArray()),
                    UltimusPads.Read<WizZoneVolumes>(wad.OpenFile("volumes.xml")?.ToArray()),
                    UltimusPads.Read<WizZoneData>(ResourceManager.OpenZoneData(wad)?.ToArray()));
            }
            catch (Exception ex) {
                Logger.Debug("Bot floors: {0} could not be read: {1}", Logger.Args(path, ex.Message));
                return Book.Empty;
            }
        });
    }

    /// <summary>Reads the floors from an area's triggers, volumes and objects.</summary>
    internal static Book Build(WizZoneTriggers triggerFile, WizZoneVolumes volumeFile, WizZoneData zoneData) {
        var triggers = triggerFile?.m_triggers?.Where(t => t is not null).ToList() ?? [];
        var volumes = volumeFile?.m_volumes?.Where(v => v is not null).ToList() ?? [];

        Volume VolumeOf(string fire)
            => volumes.FirstOrDefault(v => v.m_enterEvents?.Any(e => (string) e == fire) == true || "Enter_" + (string) v.m_volumeName == fire);

        List<string> Fires(Trigger t) => [.. t.m_fireEvents?.Select(e => e.ToString()).Where(e => !string.IsNullOrEmpty(e)) ?? []];

        List<Result> Results(Trigger t) => [.. t.m_results?.m_results?.Where(r => r is not null) ?? []];

        List<Requirement> Needs(Trigger t) => [.. t.m_requirements?.m_requirements?.Where(r => r is not null) ?? []];

        // Start states: the trigger objects, then the area's object list.
        var starts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var spots = new Dictionary<string, (float X, float Y, float Z)>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in triggers) {
            if (t.m_triggerObjInfo is { } info && info.m_templateID != 0) {
                foreach (string name in new[] { (string) info.m_zoneTag, (string) info.m_triggerObjName }) {
                    if (!string.IsNullOrEmpty(name)) {
                        if (!string.IsNullOrEmpty(info.m_startState)) {
                            starts.TryAdd(name, info.m_startState);
                        }

                        spots.TryAdd(name, (info.m_location.X, info.m_location.Y, info.m_location.Z));
                    }
                }
            }
        }

        foreach (var o in zoneData?.m_objectList?.Where(o => o is not null) ?? []) {
            string tag = o.m_zoneTag;
            if (string.IsNullOrEmpty(tag)) {
                continue;
            }

            if (!string.IsNullOrEmpty(o.m_startState)) {
                starts.TryAdd(tag, o.m_startState);
            }

            spots.TryAdd(tag, (o.m_location.X, o.m_location.Y, o.m_location.Z));
        }

        // Battle circles.
        var circles = new List<Circle>();
        foreach (var t in triggers.Where(t => Results(t).Any(r => r is ResInitiateCombat))) {
            foreach (var fire in Fires(t)) {
                if (VolumeOf(fire) is { } v) {
                    circles.Add(new Circle((string) t.m_triggerName ?? "", (string) v.m_volumeName ?? "", v.m_locationX, v.m_locationY, v.m_locationZ));
                }
            }
        }

        // Puzzle plates: a volume that flips an object it needs in a state to another state.
        var toggles = new List<Toggle>();
        foreach (var t in triggers) {
            var needs = Needs(t).OfType<ReqTriggerObjectState>().ToList();
            foreach (var set in Results(t).OfType<ResModifyTriggerObject>()) {
                string obj = set.m_triggerObjName;
                var from = needs.FirstOrDefault(n => string.Equals((string) n.m_triggerObjName, obj, StringComparison.OrdinalIgnoreCase));
                if (from is null || from.m_applyNOT || string.IsNullOrEmpty(obj)) {
                    continue;
                }

                foreach (var fire in Fires(t)) {
                    if (VolumeOf(fire) is { } v) {
                        toggles.Add(new Toggle(obj, from.m_triggerObjState, set.m_triggerObjState, (string) v.m_volumeName ?? "", v.m_locationX, v.m_locationY, v.m_locationZ));
                    }
                }
            }
        }

        // Gates and doors that open later, and what opens them.
        var gates = new List<Gate>();
        foreach (var t in triggers) {
            foreach (var set in Results(t).OfType<ResModifyTriggerObject>()) {
                string obj = set.m_triggerObjName;
                if (string.IsNullOrEmpty(obj) || !Opens(set.m_triggerObjState) || OverlayQuestPath.IsLiftState(set.m_triggerObjState)
) {
                    continue;
                }

                if (!spots.TryGetValue(obj, out var at)) {
                    continue;
                }

                var gate = new Gate(obj, (string) t.m_triggerName ?? "", at.X, at.Y, Opener.Other, "", float.NaN, float.NaN, "", float.NaN, float.NaN, [], Z: at.Z);
                foreach (var fire in Fires(t)) {
                    if (string.Equals(fire, "Monster_Killed", StringComparison.OrdinalIgnoreCase)) {
                        gate = gate with { By = Opener.Beat };
                        break;
                    }

                    if (VolumeOf(fire) is { } v) {
                        gate = gate with { By = Opener.Step, Volume = (string) v.m_volumeName ?? "", VX = v.m_locationX, VY = v.m_locationY, VZ = v.m_locationZ };
                        break;
                    }

                    // "Lever.Down.EnterState": the lever itself turned.
                    var bits = fire.Split('.');
                    if (bits.Length == 3 && bits[2].Equals("EnterState", StringComparison.OrdinalIgnoreCase) && spots.TryGetValue(bits[0], out var lever)) {
                        gate = gate with { By = Opener.Click, Lever = bits[0], LX = lever.X, LY = lever.Y, LZ = lever.Z };
                        break;
                    }

                    // "ActivateFrom_X": another trigger X, fired by a click on a lever, enabled once its puzzle is right.
                    if (fire.StartsWith("ActivateFrom_", StringComparison.OrdinalIgnoreCase)
                        && triggers.FirstOrDefault(x => string.Equals((string) x.m_triggerName, fire["ActivateFrom_".Length..], StringComparison.OrdinalIgnoreCase)) is { } from) {
                        if (Fires(from).Select(VolumeOf).FirstOrDefault(v => v is not null) is { } step) {
                            gate = gate with { By = Opener.Step, Volume = (string) step.m_volumeName ?? "", VX = step.m_locationX, VY = step.m_locationY, VZ = step.m_locationZ };
                            break;
                        }

                        var enabler = triggers.FirstOrDefault(p => Results(p).OfType<ResPostEvent>()
                            .Any(r => string.Equals((string) r.m_eventName, "Enable_" + (string) from.m_triggerName, StringComparison.OrdinalIgnoreCase)));
                        var needs = enabler is null
                            ? []
                            : Needs(enabler).OfType<ReqTriggerObjectState>().Where(n => !n.m_applyNOT)
                                .Select(n => ((string) n.m_triggerObjName, (string) n.m_triggerObjState)).ToList();
                        var near = needs.Count > 0 && toggles.FirstOrDefault(g => needs.Any(n => n.Item1 == g.Object)) is { } plate ? (plate.X, plate.Y) : (at.X, at.Y);
                        var leverName = spots.Keys.Where(k => IsLever(k))
                            .OrderBy(k => Far(spots[k].X, spots[k].Y, near.Item1, near.Item2)).FirstOrDefault();
                        if (leverName is not null && Far(spots[leverName].X, spots[leverName].Y, at.X, at.Y) <= PUZZLE) {
                            gate = gate with {
                                By = Opener.Click, Lever = leverName,
                                LX = spots[leverName].X, LY = spots[leverName].Y, LZ = spots[leverName].Z, Needs = needs,
                            };
                        }

                        break;
                    }
                }

                gates.Add(gate);
            }
        }

        // The teleporter on just past each door (not one back or out): once the door is open, the way on is through it.
        var pads = new List<(string Name, float X, float Y)>();
        foreach (var t in triggers.Where(t => Results(t).Any(r => r is ResTeleport))) {
            string name = (string) t.m_triggerName ?? "";
            if (name.Contains("Exit", StringComparison.OrdinalIgnoreCase) || name.Contains("Out", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Back", StringComparison.OrdinalIgnoreCase) || name.Contains("Hub", StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            foreach (var fire in Fires(t)) {
                if (VolumeOf(fire) is { } v) {
                    pads.Add(((string) v.m_volumeName ?? name, v.m_locationX, v.m_locationY));
                }
            }
        }

        for (var i = 0; i < gates.Count; i++) {
            var g = gates[i];
            var by = pads.Where(p => Far(p.X, p.Y, g.X, g.Y) <= THROUGH).OrderBy(p => Far(p.X, p.Y, g.X, g.Y)).ToList();
            if (by.Count > 0) {
                gates[i] = g with { Through = by[0].Name, TX = by[0].X, TY = by[0].Y };
            }
        }

        return new Book(circles.DistinctBy(c => c.Trigger + "|" + c.Volume).ToList(), gates.DistinctBy(g => g.Object + "|" + g.Trigger).ToList(),
            toggles.DistinctBy(g => g.Object + "|" + g.From + "|" + g.Volume).ToList(), starts) { Ahead = pads };
    }

    // A thing a player pulls or presses.
    private static bool IsLever(string name)
        => name.Contains("Lever", StringComparison.OrdinalIgnoreCase) || name.Contains("Switch", StringComparison.OrdinalIgnoreCase)
           || name.Contains("Crank", StringComparison.OrdinalIgnoreCase) || name.Contains("Button", StringComparison.OrdinalIgnoreCase);

    // A gate or door that opens (Open, OpenIdle, Opening, Unlock ...).
    internal static bool Opens(string state)
        => !string.IsNullOrEmpty(state)
           && (state.Contains("Open", StringComparison.OrdinalIgnoreCase) || state.Contains("Unlock", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The nearest teleporter on from (x, y) on the same ground, not behind a door still shut (the floor's duty opens that
    /// one first), or null. With no way on the map to a step on a later floor, this is the way a player goes: the floor's
    /// own teleporter up.
    /// </summary>
    internal static (string Name, float X, float Y)? PadAhead(Book book, Live live, float x, float y) {
        if (book is null || book.Ahead.Count == 0) {
            return null;
        }

        string StateOf(string obj) => live?.StateOf?.Invoke(obj) is { Length: > 0 } st ? st : book.Starts.GetValueOrDefault(obj) ?? "";
        // Behind a shut door, or a door's pad he is already past (the room before's way up lands by this room's start).
        var skip = book.Gates.Where(g => g.Through.Length > 0
                                         && (!Opens(StateOf(g.Object)) || ((x - g.TX) * (g.TX - g.X)) + ((y - g.TY) * (g.TY - g.Y)) >= 0))
            .Select(g => g.Through).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var pad in book.Ahead.Where(p => Far(x, y, p.X, p.Y) <= ROOM * 2 && !skip.Contains(p.Name)).OrderBy(p => Far(x, y, p.X, p.Y))) {
            if (live?.SameGround?.Invoke(pad.X, pad.Y) != false) {
                return pad;
            }
        }

        return null;
    }

    // How far up or down a thing may be and still be on the wizard's floor (Counterweight East's floors are 900 apart).
    internal const float FLOOR = 300f;

    /// <summary>Whether a thing at height <paramref name="oz"/> is on the floor of a wizard at height <paramref name="z"/> (unknown: yes).</summary>
    internal static bool OnFloor(float z, float oz) => float.IsNaN(z) || float.IsNaN(oz) || MathF.Abs(z - oz) <= FLOOR;

    internal static float Far(float x1, float y1, float x2, float y2) => MathF.Sqrt(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)));

    /// <summary>What the bot sees of the zone right now: object states, trigger fires and creatures (null when unknown).</summary>
    internal sealed record Live(Func<string, string> StateOf, Func<string, int> FiresOf, Func<string, bool?> EnabledOf, Func<float, float, bool> FoesNear,
        Func<float, float, bool> SameGround = null);

    /// <summary>
    /// The next floor duty for a wizard at (x, y), or null when the floor is done: the room's battle circle not yet
    /// started, then creatures by a fight-gate or a started circle, then a gate's spot to step on, then a puzzle's plates
    /// and its lever. <paramref name="tries"/> counts the steps and clicks already made, by key (kept by the bot per zone copy).
    /// </summary>
    internal static Duty Next(Book book, Live live, float x, float y, IReadOnlyDictionary<string, int> tries, float z = float.NaN) {
        if (book is null || book.IsEmpty) {
            return null;
        }

        // Only this floor's things (Niduus, 2026-11-16, Counterweight East: "He killed the boss, then decided to fight the
        // mobs a floor down, then hit the lever. He should: Fight the mobs, Flip the lever, Ascend a floor/Fight the boss").
        // The floors stand one over another, so a circle or lever right above or below is not this room's.
        bool Here(float oz) => OnFloor(z, oz);

        int Tried(string key) => tries?.GetValueOrDefault(key) ?? 0;
        string StateOf(string obj) => live?.StateOf?.Invoke(obj) is { Length: > 0 } s ? s : book.Starts.GetValueOrDefault(obj) ?? "";
        bool Closed(Gate g) => !Opens(StateOf(g.Object));
        bool Off(string trigger) => live?.EnabledOf?.Invoke(trigger) == false;

        // 1. The room's battle circle, when it has not started its fight in this copy of the area.
        foreach (var circle in book.Circles.Where(c => Far(x, y, c.X, c.Y) <= ROOM && Here(c.Z)).OrderBy(c => Far(x, y, c.X, c.Y))) {
            var key = "circle|" + circle.Trigger;
            if ((live?.FiresOf?.Invoke(circle.Trigger) ?? 0) > 0 || Tried(key) >= 1 || Off(circle.Trigger)) {
                continue;
            }

            return new Duty(STEP, circle.X, circle.Y, $"the room's battle circle \"{circle.Volume}\"", key,
                "each room's fight starts on its battle circle, and the way on opens once it is won");
        }

        var near = book.Gates.Where(g => Closed(g) && Far(x, y, g.X, g.Y) <= ROOM && Here(g.Z)).OrderBy(g => Far(x, y, g.X, g.Y)).ToList();

        // 2. Creatures by a closed door that opens on a fight, or by the room's circle once its fight started.
        foreach (var gate in near.Where(g => g.By == Opener.Beat)) {
            if (live?.FoesNear?.Invoke(gate.X, gate.Y) == true) {
                return new Duty(FIGHT, gate.X, gate.Y, $"the creatures by \"{gate.Object}\"", "beat|" + gate.Object,
                    $"\"{gate.Object}\" opens once the room's creatures are beaten");
            }
        }

        // Only a circle whose fight has started: before that its creatures are not out for a player (Niduus, 2026-11-16, Big
        // Ben's twentieth floor: "didn't show mobs on my screen but Ultimus started the combat with them").
        foreach (var circle in book.Circles.Where(c => Far(x, y, c.X, c.Y) <= ROOM && Here(c.Z) && (live?.FiresOf is null || live.FiresOf(c.Trigger) > 0))) {
            if (live?.FoesNear?.Invoke(circle.X, circle.Y) == true) {
                return new Duty(FIGHT, circle.X, circle.Y, $"the creatures of the room's battle circle \"{circle.Volume}\"", "room|" + circle.Trigger,
                    "the room's creatures are beaten before going on");
            }
        }

        // 3. A spot that opens a door here.
        foreach (var gate in near.Where(g => g.By == Opener.Step && !Off(g.Trigger) && Here(g.VZ))) {
            var key = "step|" + gate.Volume;
            if (Tried(key) < TRIES) {
                return new Duty(STEP, gate.VX, gate.VY, $"\"{gate.Volume}\", which opens \"{gate.Object}\"", key,
                    $"stepping on it opens \"{gate.Object}\"");
            }
        }

        // 4. A puzzle: the plates that are wrong, one at a time, then the lever.
        foreach (var gate in book.Gates.Where(g => g.By == Opener.Click && Closed(g) && !Off(g.Trigger)
                                                   && Far(x, y, g.LX, g.LY) <= PUZZLE && Here(g.LZ))
                     .OrderBy(g => Far(x, y, g.LX, g.LY))) {
            var wrong = gate.Needs.Where(n => !string.Equals(StateOf(n.Object), n.State, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var (obj, state) in wrong) {
                var now = StateOf(obj);
                var plate = book.Toggles.FirstOrDefault(t => t.Object == obj && string.Equals(t.From, now, StringComparison.OrdinalIgnoreCase)
                                                             && string.Equals(t.To, state, StringComparison.OrdinalIgnoreCase))
                            ?? book.Toggles.FirstOrDefault(t => t.Object == obj && string.Equals(t.From, now, StringComparison.OrdinalIgnoreCase));
                if (plate is null) {
                    continue;
                }

                var key = $"plate|{obj}|{now}";
                if (Tried(key) < TRIES) {
                    return new Duty(STEP, plate.X, plate.Y, $"the puzzle plate \"{obj}\" (now {now}, needs {state})", key,
                        $"the lever that opens \"{gate.Object}\" works once {string.Join(", ", gate.Needs.Select(n => $"{n.Object} is {n.State}"))}");
                }
            }

            if (wrong.Count == 0 && Tried("lever|" + gate.Lever) < TRIES + 2) {
                return new Duty(CLICK, gate.LX, gate.LY, $"the lever \"{gate.Lever}\", which opens \"{gate.Object}\"", "lever|" + gate.Lever,
                    gate.Needs.Count > 0 ? "the puzzle is right, so the lever now opens the door" : $"pulling it opens \"{gate.Object}\"");
            }
        }

        // 5. A door that opened in this copy of the area (it starts shut): through it to the teleporter on, as a player goes on
        // once the room is done. He beat Big Ben's sixth floor and its door opened, then walked back to the south wall
        // toward Meowiarty's spot for an hour, since the boss is on the same height (his log, 2026-11-16 03:14).
        foreach (var gate in book.Gates.Where(g => g.Through.Length > 0 && !Closed(g) && !Opens(book.Starts.GetValueOrDefault(g.Object) ?? "")
                                                   && Far(x, y, g.X, g.Y) <= ROOM && Here(g.Z) && Before(g) && live?.SameGround?.Invoke(g.X, g.Y) != false)
                     .OrderBy(g => Far(x, y, g.X, g.Y))) {
            var key = "through|" + gate.Object;
            if (Tried(key) < TRIES) {
                return new Duty(STEP, gate.TX, gate.TY, $"the teleporter \"{gate.Through}\" past \"{gate.Object}\"", key,
                    $"\"{gate.Object}\" is open now, so the way on is through it");
            }
        }

        return null;

        // On the door's side of its teleporter, not already past it (the next room's landing is near the same door).
        bool Before(Gate g) => ((x - g.TX) * (g.TX - g.X)) + ((y - g.TY) * (g.TY - g.Y)) < 0;
    }

}
