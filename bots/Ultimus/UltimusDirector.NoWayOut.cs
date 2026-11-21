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
using Imlight.CoreLib.Game.Zone.Components;

namespace Imlight.CoreLib.Game.Ultimus;

internal sealed partial class UltimusDirectorActor {
    // How often he presses the Commons button while no way out of an area is on record.
    private static readonly TimeSpan s_noWayOutPress = TimeSpan.FromMinutes(3);

    private DateTime _noWayOutPressed = DateTime.MinValue;

    /// <summary>
    /// The way to the step's area starts in an area with no way out on record: the game's own exit there is not one the way
    /// finder knows, so he presses the Commons button, as a player does, and goes on from The Commons. His spot is not
    /// marked: there is nothing to come back to in an area he could not leave.
    /// Niduus's log, 2026-11-20 06:11-21:30: Kitharas Sentence followed a teammate into Ravenwood's tutorial copy
    /// (WizardCity/WC_Ravenwood_Lite, whose game file holds only its look, no exits) and stood there 15 hours, "no way from
    /// Ravenwood to Unicorn Way could be found", logging back in there after every restart.
    /// </summary>
    private void NoWayOut(UltimusMember.Status leader, UltimusPlanner.Target target) {
        if (!target.Problem.StartsWith("no way from", StringComparison.Ordinal) || leader.Wizard is not { } wizard || wizard.ZoneIsInstance
            || CombatDuelComponent.IsInBattle(leader.GlobalId) || (wizard.MagicSchoolBehavior?.Level ?? 1) < 2
            || DateTime.UtcNow - _noWayOutPressed < s_noWayOutPress || DateTime.UtcNow - _hubPressedAt < TimeSpan.FromSeconds(20)) {
            return;
        }

        _noWayOutPressed = _hubPressedAt = DateTime.UtcNow;
        _buttonFromZone = leader.Zone;
        Note("Ultimus", $"ABNORMAL: {leader.Name} finds no way out of {UltimusText.Zone(leader.Zone)} on record: presses the Commons button and goes on from there, as a player does.",
            UltimusLink.ABNORMAL);
        Think($"I can't find a way out of {UltimusText.Zone(leader.Zone)} toward \"{target.What}\". A player presses the Commons button here, so I do too and go on from The Commons.",
            "nowayout|" + leader.Zone);
        Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1.5), _actors[0], new UltimusMember.PressCommons(), Self);
    }
}
