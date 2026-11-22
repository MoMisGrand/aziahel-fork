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
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imlight.CoreLib.Game.BotShared;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.Game.Zone.Core;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// Dungeon rooms, as a player plays them (Niduus, 2026-11-16: "In order to TP he has to complete the battles for that
/// floor for the zone TP to unlock. This also has a puzzle in the dungeon"). In a dungeon or instance he steps on each
/// room's battle circle and wins its fight, beats the creatures by a door that opens on a fight, steps on the spots that
/// open doors, and sets a puzzle's plates right before pulling its lever (BotShared/BotFloors), before going on to his
/// step's teleporter. He hopped the teleporters past every floor before, since the server knows where each one lands.
/// </summary>
internal sealed partial class UltimusDirectorActor {

    // Steps and clicks made per floor duty, for this copy of the area.
    private readonly Dictionary<string, int> _floorTries = new(StringComparer.Ordinal);

    // The floor duty spot he is on his way to, and since when (UnreachableFor).
    private (string Key, DateTime Since) _floorChase;
    private static readonly TimeSpan UnreachableFor = TimeSpan.FromMinutes(5);
    private string _floorBehindSaid;
    private string _floorZone = "";
    // The spot he stands on now (stepped on once); he steps off before stepping on it again.
    private string _floorOn;
    // Pieces he has pulled in this copy, and the one he is stepping back from to have it offered again (StepOutFrom).
    private readonly HashSet<string> _floorPulled = new(StringComparer.Ordinal);
    private string _floorOut;

    // Floor teleporters walked to for a step with no way on the map, and when (each one tried again only after a while).
    private readonly Dictionary<string, DateTime> _padAheadAt = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// No way on the map to a step on a later floor of a dungeon: at once to the floor's own teleporter on, instead of
    /// standing about until he counts as stuck (his log, 2026-11-16 05:21: 43 seconds at Big Ben's fifth floor before the
    /// stuck fix sent him up).
    /// </summary>
    private bool FloorPadAhead(UltimusMember.Status leader, UltimusPlanner.Target target, string what) {
        if (!InDungeon(leader) || string.IsNullOrEmpty(leader.Zone) || target is null
            || BotFloors.Far(leader.X, leader.Y, target.X, target.Y) <= BotFloors.ROOM) {
            return false;
        }

        var book = BotFloors.BookOf(leader.Zone);
        var live = FloorLive(leader);
        if (BotFloors.PadAhead(book, live, leader.X, leader.Y) is not { } pad) {
            return false;
        }

        var key = leader.Zone + "|" + pad.Name;
        if (_padAheadAt.TryGetValue(key, out var at) && DateTime.UtcNow - at > TimeSpan.FromSeconds(60) && DateTime.UtcNow - at < TimeSpan.FromMinutes(5)) {
            // Walked to it a minute ago and still here with no way: the other fixes get their turn.
            return false;
        }

        if (!_padAheadAt.ContainsKey(key) || DateTime.UtcNow - at >= TimeSpan.FromMinutes(5)) {
            _padAheadAt[key] = DateTime.UtcNow;
            Think($"There is no way on the map from here to \"{what}\", which is on a later floor. The way a player goes is this floor's " +
                  $"teleporter on, \"{pad.Name}\", so I take it now.", "padahead|" + key);
        }

        var points = OverlayQuestPath.Walk(leader.CharId, leader.Zone, leader.X, leader.Y, pad.X, pad.Y, body: true);
        // The teleporter is just past a door open now: the map still has the door shut, so the walk ends at the door. He
        // goes on straight through it onto the teleporter, as a player does. In a group he walked to the open Door21 behind
        // Walter, stopped 300 short of "Level20 to Boss" and never went into Meowiarty's room (Niduus, 2026-11-17: "he
        // didn't go in the final room").
        if (BotFloors.OpenDoorBefore(book, live, pad.Name) is { } door
            && (points.Count == 0 || BotFloors.Far(points[^1].X, points[^1].Y, pad.X, pad.Y) > 150)) {
            points = points.Count == 0 ? [(leader.X, leader.Y)] : points;
            points.Add((door.X, door.Y));
            points.Add((pad.X, pad.Y));
            Think($"\"{pad.Name}\" is just past \"{door.Object}\", which is open now. The map still has the door shut, so I walk " +
                  "straight through it.", "padahead-door|" + key);
        }

        _sent[0] = (target, DateTime.UtcNow, leader.X, leader.Y);
        _actors[0].Tell(new UltimusMember.WalkTo(points, $"the floor's teleporter on, toward {what}"));
        _leaderLastTarget = target;

        return true;
    }

    // What the floor rules read live: the doors' states in his copy and his own ground.
    private static BotFloors.Live FloorLive(UltimusMember.Status leader) {
        var state = ZoneStates.Of(leader.ZoneActor);

        return new BotFloors.Live(name => state?.GetObjectState(name), _ => 0, _ => null, (_, _) => false,
            (x, y) => OverlayQuestPath.IslandOf(leader.Zone, leader.X, leader.Y) is var a && OverlayQuestPath.IslandOf(leader.Zone, x, y) is var b
                      && (a < 0 || b < 0 || a == b));
    }

    /// <summary>
    /// Whether the step (not a fight) is cut off on a floor before this one: the teleporter toward it is not one of the
    /// area's teleporters on, so it goes back.
    /// </summary>
    private bool StepBehind(UltimusMember.Status leader, UltimusPlanner.Target target, BotFloors.Book book) {
        if (target is null || target.Kind == UltimusPlanner.FIGHT || (target.Key ?? "").StartsWith("floor|", StringComparison.Ordinal)
            || !OverlayQuestPath.Apart(leader.Zone, leader.X, leader.Y, target.X, target.Y)) {
            return false;
        }

        // Behind only when the teleporter toward it is one of the area's own teleporters back (an "Exit" one). One that is
        // neither (a teleporter he learned by walking into it, whose spot the files have a little apart) is not proof: the
        // floor is played as on the way up. Big Ben, 2026-11-17 04:04: the teleporter he learned at room 3's elevator was
        // not on the list of ways on, so the second Travis Pawman on floor 5 read as "behind", his floor fights were turned
        // off, and he walked past rooms 1 to 3 without a fight (Niduus: "he's just ignoring the fights").
        var pad = UltimusPads.Toward(leader.Zone, leader.X, leader.Y, target.X, target.Y, PadsLeft());
        return pad is not null && book.Back.Any(b => BotFloors.Far(b.X, b.Y, pad.X, pad.Y) <= 300)
               && !book.Ahead.Any(a => BotFloors.Far(a.X, a.Y, pad.X, pad.Y) <= 300);
    }

    // A talk step whose person stands in this area, on his floor and ground, with the walk there going through no door
    // still shut (one that is shut is the floor's to open first, as in Big Ben).
    private static bool TalkHere(UltimusMember.Status leader, UltimusPlanner.Target target, BotFloors.Book book, BotFloors.Live live)
        => target is { Kind: UltimusPlanner.TALK, GlobalId: not 0 } && WayClear(leader, target, book, live);

    /// <summary>
    /// The way out of this area toward his step (an exit or door here) is a walk on his floor and ground through no door
    /// still shut: no puzzle of this floor stands between him and it, so he goes. Katzenstein's Lab, 2026-11-21 22:00-22:12:
    /// Libra, right by Watson on the open west side, was sent east to the plaques that open Gate02 (2) (the next quest's
    /// gate, "Gate is Enough") though Firegut's teleporter lay west with nothing shut on the way; after the plaques he stood
    /// on the gates' side of the lab, the way back to Firegut went through the shut Gate02, and he waited there 11 minutes
    /// (flagged 31 times at that spot today, with Umbella, Hydra and Consumer).
    /// </summary>
    private static bool ExitClear(UltimusMember.Status leader, UltimusPlanner.Target target, BotFloors.Book book, BotFloors.Live live)
        => target is { Kind: UltimusPlanner.EXIT or UltimusPlanner.DOOR } && !(target.Key ?? "").StartsWith("floor|", StringComparison.Ordinal)
           && WayClear(leader, target, book, live);

    private static bool WayClear(UltimusMember.Status leader, UltimusPlanner.Target target, BotFloors.Book book, BotFloors.Live live) {
        if (!live.SameGround(target.X, target.Y) || !BotFloors.OnFloor(leader.Z, TargetZ(leader, target))) {
            return false;
        }

        try {
            var points = OverlayQuestPath.Walk(leader.CharId, leader.Zone, leader.X, leader.Y, target.X, target.Y, body: true);
            if (points is not { Count: > 0 } || UltimusPlanner.Far(points[^1].X, points[^1].Y, target.X, target.Y) > 400) {
                return false;
            }

            BotFloors.CutAtShutDoor(book, live.StateOf, leader.X, leader.Y, leader.Z, points, out var door);
            return door is null;
        }
        catch (Exception) {
            return false;
        }
    }

    // Talks with a dungeon's person for a quest that opens a gate, per quest in this copy: a talk that changes nothing is not
    // tried for ever.
    private const int GATE_TALK_TRIES = 3;
    private readonly Dictionary<string, DateTime> _gateTalkAt = [];

    /// <summary>
    /// The talk that opens a shut gate here: the gate waits on a dungeon quest (its Encounter) that this copy handed him and
    /// that he holds, and that quest's open step is a talk to a person in this copy. Null when there is none. Katzenstein's
    /// Lab, 2026-11-21: Gate02 opens on "MB-YARD3-C01-002", which each copy hands out on the way in, a talk to Watson by the
    /// door that posts "OpenFiregutGate". The bots whose own step was Watson's talked to him within a minute and went on;
    /// the ones whose step was past the gate ("The Right Combination") never talked to him, so the gate stayed locked and
    /// they stood at it for 10 minutes and more (Consumer 16:17-16:28, AlchemiZist flagged it 17:17). Every dungeon whose
    /// gate opens by a hidden talk quest is handled the same way.
    /// </summary>
    private UltimusPlanner.Target HeldGateTalk(UltimusMember.Status leader, BotFloors.Book book, BotFloors.Live live) {
        if (leader.Wizard is not { } wizard) {
            return null;
        }

        string StateOf(string obj) => live.StateOf?.Invoke(obj) is { Length: > 0 } st ? st : book.Starts.GetValueOrDefault(obj) ?? "";
        foreach (var quest in book.Gates.Where(g => g.Encounter.Length > 0 && !BotFloors.Opens(StateOf(g.Object)))
                     .Select(g => g.Encounter).Distinct(StringComparer.OrdinalIgnoreCase)) {
            bool holds;
            try {
                holds = wizard.HasQuest(quest);
            }
            catch (Exception) {
                holds = false;
            }

            var key = "gatetalk|" + quest;
            if (!holds || _floorTries.GetValueOrDefault(key) >= GATE_TALK_TRIES
                || UltimusPlanner.ForQuest([leader], quest) is not { Kind: UltimusPlanner.TALK, GlobalId: not 0 } talk
                || !live.SameGround(talk.X, talk.Y)) {
                continue;
            }

            // Each 20 seconds beside them counts as a try (the talk itself is the usual talk step).
            if (UltimusPlanner.Far(leader.X, leader.Y, talk.X, talk.Y) <= 150
                && (!_gateTalkAt.TryGetValue(key, out var last) || DateTime.UtcNow - last >= TimeSpan.FromSeconds(20))) {
                _gateTalkAt[key] = DateTime.UtcNow;
                _floorTries[key] = _floorTries.GetValueOrDefault(key) + 1;
            }

            Think($"A gate here stays shut until I talk to someone in this dungeon (its quest \"{quest}\" is still open for me), so I " +
                  "talk to them first, then go on.", "gatetalk|" + quest + "|" + leader.Zone);

            return talk with { What = $"{talk.What} (it opens a gate on my way)", Key = key };
        }

        return null;
    }

    private UltimusPlanner.Target FloorDuty(UltimusMember.Status leader, UltimusPlanner.Target target) {
        if (string.IsNullOrEmpty(leader?.Zone) || !InDungeon(leader) || IsSideDungeon(leader.Zone)) {
            return null;
        }

        // Leaving the copy to start it over (its creature is not there, or every fix failed): this run's puzzles no longer
        // matter. Before, the floor's next piece and the way out took turns every second (2026-11-21 5:59-6:28 AM,
        // AlchemiZist of Kitharas and Lamia in Katzenstein's Lab: the Gate02 Tree plaque, then the way out, for 30 minutes).
        if (target?.Key?.StartsWith("resetcopy", StringComparison.Ordinal) == true) {
            _floorOn = null;

            return null;
        }

        // A quest fight in this room goes first; one on another floor or further on waits until this floor is done: its
        // creatures, its lever, then up (Niduus, 2026-11-16: he beat Counterweight East's boss a floor up first).
        if (target?.Kind == UltimusPlanner.FIGHT && UltimusPlanner.Far(leader.X, leader.Y, target.X, target.Y) <= BotFloors.ROOM
            && BotFloors.OnFloor(leader.Z, TargetZ(leader, target))) {
            return null;
        }

        var zoneKey = leader.Zone + "|" + (leader.ZoneActor?.Path.ToString() ?? "");
        if (zoneKey != _floorZone) {
            _floorZone = zoneKey;
            _floorTries.Clear();
            _floorPulled.Clear();
            _floorOn = null;
            _floorOut = null;
        }

        var book = BotFloors.BookOf(leader.Zone);
        if (book.IsEmpty) {
            return null;
        }

        // His step is on a floor before this one (a talk back at the dungeon's start): he goes straight back by the
        // teleporters down, not fighting and solving every floor on the way. After the update restart his Big Ben copy came
        // back fresh with "Stealthy Stuff" at its first step (Talk to Travis Pawman, room 1), and he fought rooms 15, 14, 13
        // and 12 and stepped on their puzzle plates on his way down, an hour of going backwards (Niduus, 2026-11-17: "The
        // zone transfers are messed up in BIG BEN").
        if (StepBehind(leader, target, book)) {
            if (_floorOn is not null || zoneKey != _floorBehindSaid) {
                _floorBehindSaid = zoneKey;
                Think($"My step ({target.What}) is on a floor before this one, so I go straight back to it by the teleporters down. " +
                      "This floor's fights and puzzles are for the way up, not the way back.", "floor-behind|" + target.Key);
            }

            _floorOn = null;
            return null;
        }

        var state = ZoneStates.Of(leader.ZoneActor);
        var live = new BotFloors.Live(
            name => state?.GetObjectState(name),
            trigger => state?.GetTriggerFires(trigger) ?? 0,
            trigger => state is null ? null : state.IsTriggerEnabled(trigger),
            (x, y) => BotUseGuards.Near(leader.ZoneActor, x, y).Any(m => DudCreatures.Fightable(m.Object.m_globalID.Full)
                                                                    && BotFloors.OnFloor(leader.Z, m.Object.m_location.Z)),
            (x, y) => OverlayQuestPath.IslandOf(leader.Zone, leader.X, leader.Y) is var a && OverlayQuestPath.IslandOf(leader.Zone, x, y) is var b
                      && (a < 0 || b < 0 || a == b)) {
            EncounterDone = BotFloors.EncounterDoneFor(leader.Wizard),
            HasEntry = BotFloors.HasEntryFor(leader.Wizard),
            StepEvents = BotFloors.StepEventsOf(target?.Quest, target?.Goal),
            Step = target is null ? null : (target.X, target.Y),
            ChainMine = gate => (BotFloors.ClaimChain(leader.ZoneActor?.Path.ToString(), gate, leader.Name, out var holder), holder),
        };
        // A person to talk to, standing here on his own ground, comes before the floor's puzzles: a player talks first, and
        // the dungeon's own quest says what the puzzles are for (the Calling of 7, 2026-11-18 19:28-22:20: Shalek the Wise
        // stood by Emperor's Retreat's start for "Guardians at the Gate", and floor duty sent them to the far Snake obelisk
        // every other second, for two hours, until the watchdog stopped them).
        if (TalkHere(leader, target, book, live)) {
            _floorOn = null;
            return null;
        }

        // Likewise the way on toward his step, when nothing shut stands between (ExitClear): another quest's puzzle can wait.
        if (ExitClear(leader, target, book, live)) {
            if (_floorOn is not null) {
                Think($"The way to {target.What} is open from here, with no shut door on it, so I go there now; this floor's " +
                      "puzzles are not on my way.", "floor-clear|" + target.Key);
            }

            _floorOn = null;
            return null;
        }

        // A shut gate waits on a dungeon quest this copy handed him, and that quest is a talk to someone here: he talks to them,
        // as a player does when the dungeon's people say "come see me" (HeldGateTalk).
        if (HeldGateTalk(leader, book, live) is { } gateTalk) {
            _floorOn = null;
            return gateTalk;
        }

        if (BotFloors.Next(book, live, leader.X, leader.Y, _floorTries, leader.Z) is not { } duty) {
            _floorOn = null;
            return null;
        }

        // Through an opened door only when the step is not in this room.
        if (duty.Key.StartsWith("through|", StringComparison.Ordinal) && target is not null
            && BotFloors.Far(leader.X, leader.Y, target.X, target.Y) <= BotFloors.ROOM && live.SameGround(target.X, target.Y)) {
            return null;
        }

        // Nor when the step is on a floor before this one (a talk back at the dungeon's start): the way there is a teleporter
        // back, not this floor's way on. His log, 2026-11-16 14:28: after a restart reset "Stealthy Stuff", Travis Pawman
        // stood in Big Ben's first room; he walked back by the Level 14 teleporter down, then floor duty sent him on up past
        // Door35, and so on for 4 minutes, 7 times a minute.
        if (duty.Key.StartsWith("through|", StringComparison.Ordinal) && StepBehind(leader, target, book)) {
            return null;
        }

        var far = UltimusPlanner.Far(leader.X, leader.Y, duty.X, duty.Y);
        Think(duty.Key.StartsWith("through|", StringComparison.Ordinal)
                ? $"This floor is done: {duty.Why}. I go on to {duty.What}, not back toward my step from here."
                : $"This floor first, as a player plays it: {duty.What}, because {duty.Why}. The teleporter on can wait until the floor is done.",
            "floor|" + duty.Key);

        // Stepping back from a piece he pulled, until he is out of its reach; then he walks in again and it is offered anew.
        if (_floorOut is not null && (_floorOut != BotFloors.PieceOf(duty.Key) || far >= STEP_OUT)) {
            _floorOut = null;
        }

        if (_floorOut == BotFloors.PieceOf(duty.Key) && StepOutFrom(leader, duty, live) is { } back) {
            return back;
        }

        // Another wizard in this copy sets this in-order puzzle: he stands clear of its pieces and waits (not stuck).
        if (duty.Kind == BotFloors.WAIT) {
            _floorOn = null;
            return far > 80
                ? new UltimusPlanner.Target(UltimusPlanner.SPOT, duty.X, duty.Y, duty.What, Key: "floor|" + duty.Key + "|clear")
                : new UltimusPlanner.Target(UltimusPlanner.WAIT, leader.X, leader.Y, duty.What, Key: "floor|" + duty.Key);
        }

        switch (duty.Kind) {
            case BotFloors.FIGHT: {
                var foe = BotUseGuards.Near(leader.ZoneActor, duty.X, duty.Y)
                    .Where(m => DudCreatures.Fightable(m.Object.m_globalID.Full))
                    .OrderBy(m => UltimusPlanner.Far(leader.X, leader.Y, m.Object.m_location.X, m.Object.m_location.Y)).FirstOrDefault();
                if (foe is null) {
                    return null;
                }

                return new UltimusPlanner.Target(UltimusPlanner.FIGHT, foe.Object.m_location.X, foe.Object.m_location.Y,
                    $"{foe.Name}, {duty.What} (the floor's fight comes before the teleporter on)", _currentQuest ?? "", Template: foe.Template,
                    GlobalId: foe.Object.m_globalID.Full, Key: "floor|fight|" + foe.Object.m_globalID.Full);
            }
            case BotFloors.CLICK when far <= 220: {
                var lever = leader.Options.Where(kv => kv.Value.Any(o => o.Service == "Interact")).Select(kv => kv.Key).FirstOrDefault();
                if (lever != 0 && CanClick("floor|" + duty.Key)) {
                    Clicked("floor|" + duty.Key);
                    _floorTries[duty.Key] = _floorTries.GetValueOrDefault(duty.Key) + 1;
                    Note("Ultimus", $"{leader.Name} pulls {duty.What}: {duty.Why}.", UltimusLink.NOTABLE);
                    _floorPulled.Add(BotFloors.PieceOf(duty.Key));
                    _actors[0].Tell(new UltimusMember.Interact(lever, "Interact", 0, duty.What));
                }
                else if (lever == 0 && CanClick("floor|" + duty.Key + "|none")) {
                    Clicked("floor|" + duty.Key + "|none");
                    _floorTries[duty.Key] = _floorTries.GetValueOrDefault(duty.Key) + 1;
                    Note("Ultimus", $"{leader.Name} is at {duty.What}, but it offers nothing to pull yet.", UltimusLink.ABNORMAL);
                }

                // Nothing to pull: a piece like Katzenstein's Lab's plaques turns when a wizard walks onto it, so he steps right
                // onto it instead of standing beside it (the Calling of 7, 2026-11-20: the plaques never offered a click, and
                // turned only when a bot happened to walk over one).
                // A piece he pulled already and that needs another pull offers nothing while he stands by it: the game gives its
                // pull again when a wizard comes back into reach. Kitharas Lamia, The Vault of Ice, 2026-11-20 20:47-20:48:
                // Brazier3 went Fire to Ice, then offered nothing for 30 seconds until she walked off and back, and the Frost
                // door opened on the next pull; AlchemiZist stood on it at 21:34, gave up, and sat at the shut door 10 minutes.
                if (lever == 0 && _floorPulled.Contains(BotFloors.PieceOf(duty.Key)) && StepOutFrom(leader, duty, live) is { } stepOut) {
                    _floorOut = BotFloors.PieceOf(duty.Key);
                    return stepOut;
                }

                if (lever == 0) {
                    return new UltimusPlanner.Target(UltimusPlanner.SPOT, duty.X, duty.Y, $"onto {duty.What} (it turns when stepped on)", Key: "floor|" + duty.Key + "|onto");
                }

                return new UltimusPlanner.Target(UltimusPlanner.SPOT, leader.X, leader.Y, $"at {duty.What}", Key: "floor|" + duty.Key);
            }
            case BotFloors.CLICK:
                return new UltimusPlanner.Target(UltimusPlanner.SPOT, duty.X + 60, duty.Y, $"to {duty.What}", Key: "floor|" + duty.Key);
        }

        // A spot he has had for minutes and never reached counts as tried out: Katzenstein's Lab's "Gate 1 Open" lies past its own
        // gate, a floor below, with no way to it from the entrance side; Cyndr stood 20 minutes on it, twice (2026-11-20).
        if (far > BotFloors.ON_SPOT && _floorOn != duty.Key) {
            if (_floorChase.Key != duty.Key) {
                _floorChase = (duty.Key, DateTime.UtcNow);
            }
            else if (DateTime.UtcNow - _floorChase.Since > UnreachableFor) {
                _floorTries[duty.Key] = BotFloors.TRIES;
                _floorChase = default;
                Note("Ultimus", $"{leader.Name} gives up on {duty.What}: no way there in {UnreachableFor.TotalMinutes:0} minutes, so it is not his to step on from here.", UltimusLink.ABNORMAL);

                return null;
            }
        }

        // A spot to step on: counted once on arrival; stepped off first when it has to be stepped on again.
        if (far <= BotFloors.ON_SPOT && _floorOn != duty.Key) {
            _floorOn = duty.Key;
            _floorTries[duty.Key] = _floorTries.GetValueOrDefault(duty.Key) + 1;
            Note("Ultimus", $"{leader.Name} steps on {duty.What}: {duty.Why}.", UltimusLink.NOTABLE);
        }

        if (_floorOn == duty.Key && far <= 300) {
            if (_floorTries.GetValueOrDefault(duty.Key) >= BotFloors.TRIES || duty.Key.StartsWith("circle|", StringComparison.Ordinal)) {
                return null;
            }

            // Off the spot and back on, as a player re-steps a plate that did not turn.
            var (dx, dy) = (leader.X - duty.X, leader.Y - duty.Y);
            var len = MathF.Max(1, MathF.Sqrt((dx * dx) + (dy * dy)));
            return new UltimusPlanner.Target(UltimusPlanner.SPOT, duty.X + (dx / len * 380), duty.Y + (dy / len * 380),
                $"off {duty.What}, to step on it again", Key: "floor|off|" + duty.Key);
        }

        if (far > 300 && _floorOn == duty.Key) {
            _floorOn = null;
        }

        return new UltimusPlanner.Target(UltimusPlanner.SPOT, duty.X, duty.Y, $"onto {duty.What}", Key: "floor|" + duty.Key);
    }


    // How far he steps back from a pulled piece to be out of its reach (a pull is offered within about 220).
    private const float STEP_OUT = 320f;

    /// <summary>
    /// A spot out of a pulled piece's reach, on his own ground: away on his own side of it first, then the other ways round.
    /// </summary>
    private UltimusPlanner.Target StepOutFrom(UltimusMember.Status leader, BotFloors.Duty duty, BotFloors.Live live) {
        var dx = leader.X - duty.X;
        var dy = leader.Y - duty.Y;
        var len = MathF.Sqrt((dx * dx) + (dy * dy));
        var start = len < 30 ? MathF.PI : MathF.Atan2(dy, dx);
        for (var i = 0; i < 8; i++) {
            var a = start + (i % 2 == 0 ? 1 : -1) * ((i + 1) / 2) * (MathF.PI / 4);
            var x = duty.X + (MathF.Cos(a) * (STEP_OUT + 40));
            var y = duty.Y + (MathF.Sin(a) * (STEP_OUT + 40));
            if (live?.SameGround is null || live.SameGround(x, y)) {
                return new UltimusPlanner.Target(UltimusPlanner.SPOT, x, y,
                    $"a few steps back from {duty.What} (it offers its pull again when I come back in reach)", Key: "floor|" + duty.Key + "|out");
            }
        }

        return null;
    }
}
