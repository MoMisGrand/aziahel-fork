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
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.World;
using Imlight.CoreLib.Shared.Items;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// Jewels for a questing wizard, got and used as a player does (Niduus, 2026-11-21: "since he's level 15+, he can now use
/// jewels as well", while he asked Ultimus for the most damage he can pump out). The open sockets of the gear he wears (not
/// his pet's) are filled with jewels from his backpack, the best for his school and style first (BotJewelPicker, the same
/// measure True Form's jewels use); a socket that none of his jewels fits is shopped for: a jewel a shop sells that he can
/// pay for. He sockets it himself (the game's socket request), so the jewel leaves his backpack as a player's does.
/// </summary>
internal static class BotJewelPlan {

    /// <summary>An open socket of a worn piece and the jewel from his backpack that goes in it.</summary>
    internal sealed record Fit(WizClientObjectItem Item, int Socket, JewelSocketType Shape, WizClientObjectItem Jewel, string JewelName,
                               string ItemName, double Score);

    /// <summary>A jewel a shop sells for one of his open sockets.</summary>
    internal sealed record Buy(uint JewelId, string Name, int Gold, IReadOnlyList<uint> Vendors, string ItemName, JewelSocketType Shape, double Score);

    // The open sockets of what he wears: pieces with sockets, sockets with no jewel (a pet's socket is the pet's matter).
    internal static IEnumerable<(WizClientObjectItem Item, WizItemTemplate Template, int Socket, JewelSocketType Shape)> OpenSockets(Wizard wizard) {
        foreach (var item in (wizard?.EquipmentBehavior?.EquippedItems ?? []).ToList()) {
            var template = ItemHelper.GetItemTemplate(item);
            var sockets = template is null ? [] : JewelSockets.GetSockets(template);
            if (sockets.Count == 0 || Pet.PetProgress.GetBehavior(item) is not null) {
                continue;
            }

            var filled = JewelSockets.GetPairs(item).Where(p => p is not null && p.m_jewelTemplateID != 0).Select(p => p.m_socketNumber).ToHashSet();
            for (var socket = 0; socket < sockets.Count; socket++) {
                if (sockets[socket]?.m_socketType is { } shape && shape != JewelSocketType.SOCKETTYPE_PET && !filled.Contains((uint) socket)) {
                    yield return (item, template, socket, shape);
                }
            }
        }
    }

    /// <summary>The best jewel from his backpack for one of his open sockets, or null.</summary>
    internal static Fit FromBackpack(Wizard wizard, string school, BotStyle style) {
        var known = BotJewels.All;
        if (wizard is null || known.Count == 0) {
            return null;
        }

        var held = (wizard.InventoryBehavior?.Items ?? []).Select(i => (Item: i, Template: ItemHelper.GetItemTemplate(i)))
            .Where(x => x.Template is not null && JewelSockets.GetShape(x.Template) is { } shape && shape != JewelSocketType.SOCKETTYPE_PET)
            .ToList();
        if (held.Count == 0) {
            return null;
        }

        var need = BotJewelPicker.For(wizard, school, style);
        Fit best = null;
        foreach (var (item, template, socket, shape) in OpenSockets(wizard)) {
            foreach (var (jewelItem, jewelTemplate) in held) {
                var id = (uint) Math.Min((ulong) jewelItem.m_templateID, uint.MaxValue);
                if (known.FirstOrDefault(j => j.Id == id) is not { } jewel || jewel.Shape != shape
                    || JewelSockets.CheckSocket(wizard, item, jewelTemplate, socket) is not null) {
                    continue;
                }

                // Anything it gives him is better than an empty socket; the best of them first.
                var score = Math.Max(need.Score(jewel, shape), need.Fallback(jewel));
                if (score > 0 && (best is null || score > best.Score)) {
                    best = new Fit(item, socket, shape, jewelItem, jewel.Name, BotJewels.NameOf(template), score);
                }
            }
        }

        return best;
    }

    /// <summary>
    /// The jewels shops sell for his open sockets that no jewel in his backpack fills, that he can wear now and pay for
    /// with <paramref name="gold"/>, best first (one for each socket). Without <paramref name="needVendor"/>, any jewel a
    /// player can get (for the Bazaar, where players sell what dropped for them too).
    /// </summary>
    internal static List<Buy> FromShops(Wizard wizard, string school, BotStyle style, int gold, ISet<uint> skip = null, bool needVendor = true) {
        var found = new List<Buy>();
        var known = BotJewels.All;
        if (wizard is null || known.Count == 0 || gold <= 0) {
            return found;
        }

        var heldShapes = (wizard.InventoryBehavior?.Items ?? []).Select(i => ItemHelper.GetItemTemplate(i))
            .Where(t => t is not null).Select(t => JewelSockets.GetShape(t)).Where(s => s is not null).Select(s => s.Value).ToList();
        var need = BotJewelPicker.For(wizard, school, style);
        foreach (var (item, template, socket, shape) in OpenSockets(wizard)) {
            // A jewel of this shape already in his backpack goes in first (FromBackpack).
            if (heldShapes.Remove(shape)) {
                continue;
            }

            var pick = known.Where(j => j.Shape == shape && (j.Sold || !needVendor) && skip?.Contains(j.Id) != true)
                .Select(j => (Jewel: j, Piece: BotGear.Read(j.Id), Score: Math.Max(need.Score(j, shape), need.Fallback(j))))
                .Where(x => x.Score > 0 && x.Piece is { Gold: > 0 } p && p.Gold <= gold && !p.CrownsOnly && (!needVendor || ItemSources.VendorsOf(x.Jewel.Id).Count > 0)
                            && JewelSockets.CheckSocket(wizard, item, x.Jewel.Template, socket) is null)
                .OrderByDescending(x => x.Score).ThenBy(x => x.Piece.Gold).FirstOrDefault();
            if (pick.Jewel is not null && found.All(f => f.JewelId != pick.Jewel.Id)) {
                found.Add(new Buy(pick.Jewel.Id, pick.Jewel.Name, pick.Piece.Gold, ItemSources.VendorsOf(pick.Jewel.Id), BotJewels.NameOf(template), shape,
                    pick.Score));
            }
        }

        return [.. found.OrderByDescending(f => f.Score)];
    }
}
