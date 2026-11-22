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


using System.Linq;
using Imlight.CoreLib.Game.Ultimus;
using Imlight.CoreLib.Game.World;

namespace Imlight.CoreLib.Game.Commands.Protocols;

/// <summary>
/// Zoom, the Story quest scout. ".zoom" says where he is and what quest he is on; ".zoom broken" lists the newest broken
/// quest steps he found (all of them are in logs/zoom/broken-quests.txt in the server folder).
/// </summary>
internal class CommandZoom : CommandProtocol {

    internal override string Group { get; set; } = "";

    [Command("zoom")]
    [Alias("scout")]
    private void ZoomCommand([Remainder] string words = "") {
        var first = (words ?? "").Trim().ToLowerInvariant();
        if (first is "broken" or "bugs" or "found") {
            Broken();
            return;
        }

        var members = UltimusDirector.MembersOf(UltimusSeat.Zoom);
        if (members.Count == 0 || members[0].State != "playing") {
            InformSenderClient("Zoom is not in the game right now. He is turned on in Imlight.ini ([Zoom] Enabled).");
            return;
        }

        var now = UltimusNow.Of(UltimusSeat.Zoom);
        var found = ZoomBook.All().Count;
        InformSenderClient($"Zoom is in {ZoneNames.Shown(members[0].Zone)}" +
                           (now.Quest.Length > 0 ? $", on {UltimusText.Title(now.Quest)}" : "") +
                           (now.Doing.Length > 0 ? $": {now.Doing}" : "") +
                           $". Broken quest steps found so far: {found}{(found > 0 ? " (.zoom broken)" : "")}.");
    }

    private void Broken() {
        var all = ZoomBook.All();
        if (all.Count == 0) {
            InformSenderClient("Zoom has found no broken quest steps yet.");
            return;
        }

        InformSenderClient($"Zoom found {all.Count} broken quest step{(all.Count == 1 ? "" : "s")}. The newest:");
        foreach (var b in Enumerable.Reverse(all).Take(5)) {
            InformSenderClient($"{b.Title}, step {b.Step}: {b.Goal} ({b.Zone})");
        }
    }

}
