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

using Imlight.Common;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.World;

/// <summary>
/// Where a server bot (Ultimus, the Quest Team, the TCO7) starts when it logs in. A player's login goes through the
/// login server, which never puts a wizard back inside an instance (GameTransitionService): it starts at the last spot
/// outside. The bots attach by themselves and used to skip that rule, so after a server restart Ultimus came back in
/// the middle of Big Ben in a brand new copy, on a floor whose way in was never taken and with the dungeon's quest
/// reset to room 1 (Niduus, 2026-11-17: "wrong floor and was entered through the exit as a enterance").
/// Now a bot logging in to a dungeon copy that still stands goes back into it (a short drop, the floors done stay
/// done); when the copy is gone it starts outside, at the dungeon's door, and walks in by the real entrance.
/// </summary>
internal static class BotLoginSpot {
    internal static (string Zone, string Location) For(Wizard wizard) {
        var zone = wizard.Zone ?? "";
        var location = Util.GetCompactStringFromVector(wizard.Location, wizard.Orientation);
        if (!wizard.ZoneIsInstance || string.IsNullOrEmpty(wizard.SafeZone) || string.IsNullOrEmpty(wizard.SafeLocation)) {
            return (zone, location);
        }

        if (DungeonRecall.CopyStands(wizard.CharId, zone)) {
            Logger.Information("Bot {0} logs back into {1}: its copy is still open.", Logger.Args(wizard.CharId, zone));

            return (zone, location);
        }

        Logger.Information("Bot {0} logged out inside instance {1}, whose copy is gone; logging in at {2} instead, so it walks in by the real entrance.",
            Logger.Args(wizard.CharId, zone, wizard.SafeZone));

        return (wizard.SafeZone, wizard.SafeLocation);
    }
}
