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
    private string _floorZone = "";
    // The spot he stands on now (stepped on once); he steps off before stepping on it again.
    private string _floorOn;

    private UltimusPlanner.Target FloorDuty(UltimusMember.Status leader, UltimusPlanner.Target target) {
        if (target?.Kind == UltimusPlanner.FIGHT || string.IsNullOrEmpty(leader?.Zone) || !InDungeon(leader)) {
            return null;
        }

        var zoneKey = leader.Zone + "|" + (leader.ZoneActor?.Path.ToString() ?? "");
        if (zoneKey != _floorZone) {
            _floorZone = zoneKey;
            _floorTries.Clear();
            _floorOn = null;
        }

        var book = BotFloors.BookOf(leader.Zone);
        if (book.IsEmpty) {
            return null;
        }

        var state = ZoneStates.Of(leader.ZoneActor);
        var live = new BotFloors.Live(
            name => state?.GetObjectState(name),
            trigger => state?.GetTriggerFires(trigger) ?? 0,
            trigger => state is null ? null : state.IsTriggerEnabled(trigger),
            (x, y) => BotUseGuards.Near(leader.ZoneActor, x, y).Any(m => DudCreatures.Fightable(m.Object.m_globalID.Full)));
        if (BotFloors.Next(book, live, leader.X, leader.Y, _floorTries) is not { } duty) {
            _floorOn = null;
            return null;
        }

        var far = UltimusPlanner.Far(leader.X, leader.Y, duty.X, duty.Y);
        Think($"This floor first, as a player plays it: {duty.What}, because {duty.Why}. The teleporter on can wait until the floor is done.",
            "floor|" + duty.Key);

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
                    _actors[0].Tell(new UltimusMember.Interact(lever, "Interact", 0, duty.What));
                }
                else if (lever == 0 && CanClick("floor|" + duty.Key + "|none")) {
                    Clicked("floor|" + duty.Key + "|none");
                    _floorTries[duty.Key] = _floorTries.GetValueOrDefault(duty.Key) + 1;
                    Note("Ultimus", $"{leader.Name} is at {duty.What}, but it offers nothing to pull yet.", UltimusLink.ABNORMAL);
                }

                return new UltimusPlanner.Target(UltimusPlanner.SPOT, leader.X, leader.Y, $"at {duty.What}", Key: "floor|" + duty.Key);
            }
            case BotFloors.CLICK:
                return new UltimusPlanner.Target(UltimusPlanner.SPOT, duty.X + 60, duty.Y, $"to {duty.What}", Key: "floor|" + duty.Key);
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

}
