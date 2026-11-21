/*
 * Imlight
 * Copyright (C) 2026 Revive101
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
using Imlight.CoreLib.Game.Tools;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// A side trip that dies on arrival: he takes the trip's way out of one area, and in the next area the trip gives him no
/// next leg, so the story walks him straight back (his log, 2026-11-19 10:42-11:01: Hametsu Village and Jade Palace every
/// 5 seconds for 20 minutes; the Bazaar sell trip took him up, and in Jade Palace only the story was left, which sent him
/// back down). Twice in 15 minutes for the same trip, and that trip is set aside for an hour.
/// </summary>
internal sealed partial class UltimusDirectorActor {

    // The last trip leg he took out of an area: what it was for, from where, to where, and when.
    private (string Purpose, string What, string From, string To, DateTime At)? _tripLeg;
    private readonly List<(string Purpose, DateTime At)> _tripBounces = [];

    private static bool TripLegOut(UltimusPlanner.Target target) =>
        target is { Kind: UltimusPlanner.EXIT or UltimusPlanner.DOOR or UltimusPlanner.SPIRAL } && !string.IsNullOrEmpty(target.To);

    /// <summary>Watches each pick for a trip leg out of an area answered, right after arriving, by the story walking back.</summary>
    private UltimusPlanner.Target WatchTripBounce(UltimusMember.Status leader, UltimusPlanner.Target target) {
        var zone = leader.Zone ?? "";
        if (target is null || zone.Length == 0) {
            return target;
        }

        if (s_tripSources.Contains(_pickSource)) {
            if (TripLegOut(target) && TripPurpose(target.What) is { Length: > 0 } purpose) {
                _tripLeg = (purpose, target.What, zone, target.To, DateTime.UtcNow);
            }
            // The trip did its work where the leg took him (trained, shopped, sold): walking back out after that is the trip
            // ending, not a bounce (his log, 2026-11-21 15:21: he trained Frost Beetle, Snow Serpent and Volcanic Shield in the
            // Ice school, spent his last point, walked out, and TRIP BOUNCE set training aside for an hour).
            else if (!TripLegOut(target) && _tripLeg is { } arrived && OverlayQuestTracker.SameZone(zone, arrived.To)) {
                _tripLeg = null;
            }

            return target;
        }

        if (_tripLeg is not { } leg || DateTime.UtcNow - leg.At > TimeSpan.FromSeconds(90)) {
            return target;
        }

        if (!OverlayQuestTracker.SameZone(zone, leg.To)) {
            return target;
        }

        // Arrived on the trip's leg, and the pick now is the way straight back where he came from.
        if (!TripLegOut(target) || !OverlayQuestTracker.SameZone(target.To, leg.From)) {
            return target;
        }

        _tripLeg = null;
        _tripBounces.RemoveAll(b => DateTime.UtcNow - b.At > TimeSpan.FromMinutes(15));
        _tripBounces.Add((leg.Purpose, DateTime.UtcNow));
        if (_tripBounces.Count(b => b.Purpose == leg.Purpose) < 2) {
            return target;
        }

        _tripBounces.RemoveAll(b => b.Purpose == leg.Purpose);
        _tripsOutOfReach[leg.Purpose] = DateTime.UtcNow.AddHours(1);
        Note("Ultimus", $"ABNORMAL: TRIP BOUNCE: the trip \"{leg.What}\" took him into {zone} and the way on from there was not there, " +
                        "twice; he sets that trip aside for an hour and stays with the story.", UltimusLink.ABNORMAL);
        Think($"Twice now \"{leg.What}\" took me into {zone}, and from there my trip had no way on, so my story sent me straight back. " +
              "That is a loop, not a trip: I set it aside for an hour and get on with the story.", "trip-bounce|" + leg.Purpose);

        return target;
    }
}
