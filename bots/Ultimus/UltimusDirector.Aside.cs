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
using System.Linq;
using Imlight.CoreLib.Game.BotShared;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.Game.Zone.Components;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// A step set aside stays set aside (Niduus, 2026-11-16, "all" to the pitch "make set aside real": he said he set the
/// Big Ben step aside, then walked straight back to it within a minute, as it was the only step open). While it waits and no
/// other story step is open, he does something useful: the free creatures of the area first (a room's fights are part of
/// the play), else he rests out of the way for a few minutes, and only then goes back to it. No unstick moves meanwhile.
/// </summary>
internal sealed partial class UltimusDirectorActor {

    // Resting while a step waits: not "stuck".
    private bool _asideWaiting;

    // When the rest for a set-aside step began (per step key).
    private (string Key, DateTime Since)? _asideRest;

    // With nothing else to do, he rests this long at most before going back to the step.
    private static readonly TimeSpan s_asideRestMost = TimeSpan.FromMinutes(5);

    // How far a creature may be for a fight while the step waits.
    private const float ASIDE_FIGHT_REACH = 4000f;

    // A creature whose walk goes through a door still shut in his copy is out of reach for a fight while the step waits (his
    // log, 2026-11-19: Garm picked the Street Sweepers behind The Ironworks' shut Gate2 60 times in an hour, stopping at the
    // door each time).
    private static bool BehindShutDoor(UltimusMember.Status leader, float x, float y) {
        try {
            var book = BotFloors.BookOf(leader.Zone);
            if (book.Gates.Count == 0) {
                return false;
            }

            var points = OverlayQuestPath.Walk(leader.CharId, leader.Zone, leader.X, leader.Y, x, y, body: true);
            if (points is not { Count: > 0 }) {
                return false;
            }

            BotFloors.CutAtShutDoor(book, Zone.Core.ZoneStates.Of(leader.ZoneActor) is { } doors ? doors.GetObjectState : null,
                leader.X, leader.Y, leader.Z, points, out var door);
            return door is not null;
        }
        catch (Exception) {
            return false;
        }
    }

    // The step a set-aside rest is for: its quest and goal, so the step's other moves count as the same step.
    private static string AsideStep(UltimusPlanner.Target target)
        => target is null ? "" : target.Quest is { Length: > 0 } || target.Goal is { Length: > 0 } ? target.Quest + "|" + target.Goal : target.Key ?? "";

    private UltimusPlanner.Target AsideWork(UltimusMember.Status leader, UltimusPlanner.Target target) {
        _asideWaiting = false;
        if (target?.Key is not { Length: > 0 } key || key.StartsWith("floor|", StringComparison.Ordinal) || key.StartsWith("aside|", StringComparison.Ordinal)
            || !UltimusPlanner.IsSetAside(key, out var until)) {
            // Another move of the same step (its look-up, its spawn spot) keeps the rest's clock running; only a new step
            // ends it. The two took turns every half second in Krokotopia (2026-11-21, "De-Cipher the Ahnics"), so the rest
            // started over each time, its note came 7 times a minute and the 5 minutes never ran out.
            if (_asideRest is { } was && was.Key != AsideStep(target)) {
                _asideRest = null;
            }

            return null;
        }

        // 1. A free creature in the area: a fight is play, and often what the area wants anyway. On the streets only one his
        // quests name (Niduus, 2026-11-19: "only fight mobs named after the targets of his quests").
        var streets = leader.Wizard?.ZoneIsInstance != true;
        var foe = MinimapIndex.In(leader.ZoneActor)
            .Where(m => m.Kind == MinimapIndex.MOB && DudCreatures.Fightable(m.Object.m_globalID.Full)
                        && (!streets || UltimusPlanner.QuestFoe(leader.Wizard, m))
                        && !CombatDuelComponent.IsInBattle(m.Object.m_globalID.Full)
                        && UltimusPlanner.Far(leader.X, leader.Y, m.Object.m_location.X, m.Object.m_location.Y) <= ASIDE_FIGHT_REACH)
            .OrderBy(m => UltimusPlanner.Far(leader.X, leader.Y, m.Object.m_location.X, m.Object.m_location.Y))
            .Take(6)
            .FirstOrDefault(m => !BehindShutDoor(leader, m.Object.m_location.X, m.Object.m_location.Y));
        if (foe is not null) {
            Think($"\"{target.What}\" is set aside until {until.ToLocalTime():h:mm tt} and I have no other story step open, so I fight " +
                  $"{foe.Name} here meanwhile instead of trying the same moves again.", "aside-fight|" + key);
            return new UltimusPlanner.Target(UltimusPlanner.FIGHT, foe.Object.m_location.X, foe.Object.m_location.Y,
                $"{foe.Name}, while \"{target.What}\" waits", _currentQuest ?? "", Template: foe.Template,
                GlobalId: foe.Object.m_globalID.Full, Key: "aside|fight|" + foe.Object.m_globalID.Full);
        }

        // 2. Nothing to fight: a short rest out of the way, then back to the step (its set-aside ends early).
        if (_asideRest is not { } rest || rest.Key != AsideStep(target)) {
            _asideRest = rest = (AsideStep(target), DateTime.UtcNow);
            Note("Ultimus", $"{leader.Name} sets \"{target.What}\" aside for real: no other story step is open and nothing to fight here, " +
                            $"so he rests out of the way for up to {(int) s_asideRestMost.TotalMinutes} minutes before trying it fresh.", UltimusLink.NOTABLE);
        }

        if (DateTime.UtcNow - rest.Since >= s_asideRestMost) {
            UltimusPlanner.EndAside(key);
            _asideRest = null;
            Think($"I rested from \"{target.What}\". I try it again now, fresh.", "aside-back|" + key);
            return null;
        }

        _asideWaiting = true;
        _asideStep = target;
        return new UltimusPlanner.Target(UltimusPlanner.WAIT, leader.X, leader.Y, $"resting while \"{target.What}\" waits", Key: "aside|wait|" + key);
    }

}
