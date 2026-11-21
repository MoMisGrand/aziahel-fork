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
using System.Numerics;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.Game.Zone.Core;

namespace Imlight.CoreLib.Game.Ultimus;

internal sealed partial class UltimusDirectorActor {
    // How much longer than the straight line the game's own way may be and still be taken (it goes round, never mad).
    private const float NAV_ACROSS_LONGER = 8f;

    // Areas and goals already said in the log, once each.
    private readonly HashSet<string> _navAcrossTold = new(StringComparer.Ordinal);

    /// <summary>
    /// The game's own walk map's way (zone.nav) to a spot on another piece of our ground map, with its heights, or null.
    /// Our ground map can show two pieces of floor as cut apart where the game has a long way round between them (a drop
    /// and a ramp up again). The way finder then walks the loose map, walls only, and that can head off the wrong way.
    /// Niduus's log, 2026-11-20 19:40-20:32: in the Tower Archives (Dragonspyre, Portal Combat) Ultimus walked north to
    /// y 12000-30000 again and again on his way to the "ToWizT" teleporter at (9177, -7685), whose floor our map shows
    /// apart from the entrance. The game's walk map goes there in one go: down past (1680, -7431) and up onto the pad.
    /// </summary>
    private (List<(float X, float Y)> Points, float[] Heights)? NavAcross(string zone, float x, float y, float z, float toX, float toY, float toZ) {
        if (string.IsNullOrEmpty(zone) || UltimusPlanner.Far(x, y, toX, toY) < 300) {
            return null;
        }

        var here = OverlayQuestPath.IslandOf(zone, x, y);
        var there = OverlayQuestPath.IslandOf(zone, toX, toY);
        if (here < 0 || there < 0 || here == there || ZoneNav.Graph(zone) is not { } graph) {
            return null;
        }

        // The step's height when not known: the game walk point nearest it (a heightless goal spoils the search).
        if (!float.IsFinite(toZ)) {
            var near = graph.Nodes.Where(n => float.IsFinite(n.X) && float.IsFinite(n.Y) && float.IsFinite(n.Z))
                .Select(n => (n.Z, Far: UltimusPlanner.Far(n.X, n.Y, toX, toY))).Where(n => n.Far <= 500).OrderBy(n => n.Far).ToList();
            if (near.Count == 0) {
                return null;
            }

            toZ = near[0].Z;
        }

        if (!float.IsFinite(z)) {
            return null;
        }

        if (ZoneNav.Route(graph, new Vector3(x, y, z), new Vector3(toX, toY, toZ)) is not { Count: > 1 } nav) {
            return null;
        }

        var length = 0f;
        for (var i = 1; i < nav.Count; i++) {
            length += UltimusPlanner.Far(nav[i - 1].X, nav[i - 1].Y, nav[i].X, nav[i].Y);
        }

        if (length > NAV_ACROSS_LONGER * UltimusPlanner.Far(x, y, toX, toY)) {
            return null;
        }

        if (_navAcrossTold.Add($"{zone}|{toX:0}|{toY:0}")) {
            Imlight.Common.Logger.Debug("Ultimus: in {0}, ({1:0}, {2:0}) and ({3:0}, {4:0}) are apart on our ground map; the game's own walk map has the way ({5} points, {6:0} long).",
                Imlight.Common.Logger.Args(zone, x, y, toX, toY, nav.Count, length));
        }

        return (nav.Select(p => (p.X, p.Y)).ToList(), nav.Select(p => p.Z).ToArray());
    }
}
