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

using System.Collections.Concurrent;
using Akka.Actor;

namespace Imlight.CoreLib.Game.Zone.Core;

/// <summary>
/// The live state of each running zone (its objects' states and which triggers fired), by the zone's actor, so the bots
/// can see what a player sees in a dungeon: which room's battle circle already started its fight, which door is open,
/// which puzzle plate is up or down (BotShared/BotFloors). Read only; only the zone itself changes its state.
/// </summary>
internal static class ZoneStates {

    private static readonly ConcurrentDictionary<IActorRef, ZoneScriptState> s_zones = new();

    internal static void Register(IActorRef zone, ZoneScriptState state) {
        if (zone is not null && state is not null) {
            s_zones[zone] = state;
        }
    }

    internal static void Unregister(IActorRef zone) {
        if (zone is not null) {
            s_zones.TryRemove(zone, out _);
        }
    }

    /// <summary>The state of the zone run by <paramref name="zone"/>, or null when it is not running here.</summary>
    internal static ZoneScriptState Of(IActorRef zone)
        => zone is not null && s_zones.TryGetValue(zone, out var state) ? state : null;

}
