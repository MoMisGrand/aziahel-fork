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
using Imlight.Common;
using Imlight.CoreLib.Game.BotShared;
using Imlight.CoreLib.Game.Combat;
using Imlight.CoreLib.Game.Spells;
using Imlight.CoreLib.Shared.Character;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// Zoom's spellbook and deck (Niduus, 2026-11-22: "give Zoom all vanilla spells for storm and enchants. His main tactic should
/// either be temp + Epic to clear or Glowbugs + Epic to clear"). His book holds every Storm spell and every enchantment a
/// player can have at the top level (no dev, monster or boss spells); fighting spells of the other schools he picked up
/// training come out. His deck is his tactic: Epic, Tempest and Glowbug Squall as many copies as the deck allows, then his
/// biggest Storm hits. The game's spell list is read in the background after a bots update (up to several minutes), so this
/// is checked again every half minute until it is done, instead of giving up when he logs in (his log, 2026-11-22 00:49:
/// "his spells were not read in time", so he kept the 31 spells he had trained and Ultimus's deck).
/// </summary>
internal static class ZoomSpells {

    internal const string EPIC = "Epic";
    internal static readonly string[] Clearers = ["Tempest", "Glowbug Squall"];

    internal static string DeckWords => "his tactic: Epic on Tempest or Glowbug Squall to clear";

    private static int Top => Math.Clamp(MagicLevelsConfig.MaxLevel, 1, 255);

    private static bool IsEnchant(uint id) => CoreObjectFactory.GetCoreTemplate(id) is SpellTemplate t && CombatEnchantments.IsEnchantment(t);

    private static bool Storm(TrueFormPool.Entry e) => e.School.Equals(UltimusSeat.Zoom.School.ToString(), StringComparison.OrdinalIgnoreCase);

    // His spells: every Storm spell and every enchantment a player can train at the top level.
    private static List<TrueFormPool.Entry> Wanted()
        => [.. TrueFormPool.Versions(Top, dev: false)
            .Where(e => !e.Dev && !e.Treasure && !TrueFormPool.NotForPlayers(e.Id) && !BotCards.NotVanilla(e.Card)
                        && SpellFactory.GetSpell(e.Id) is not null && (Storm(e) || IsEnchant(e.Id)))
            .GroupBy(e => e.Id).Select(g => g.First())];

    /// <summary>
    /// Puts every Storm spell and enchantment in his book and takes out other schools' fighting spells. False while the
    /// game's spells are still being read (try again later). Says what it did in the server log.
    /// </summary>
    internal static bool Ensure(Wizard wizard) {
        if (wizard?.SpellbookBehavior is not { } book || !TrueFormPool.Ready) {
            return false;
        }

        try {
            var wanted = Wanted();
            if (wanted.Count == 0) {
                return false;
            }

            var keep = wanted.Select(e => e.Id).ToHashSet();
            var learned = (book.LearnedSpellTemplateIds ?? []).ToHashSet();
            var add = keep.Where(id => !learned.Contains(id)).ToList();
            // Fighting spells of other schools (those the spell list knows) come out; utility spells (fishing, gardening) stay.
            var drop = learned.Where(id => !keep.Contains(id) && TrueFormPool.Of(id) is { Treasure: false } && !IsEnchant(id)).ToHashSet();
            if (add.Count > 0 || drop.Count > 0) {
                book.LearnedSpellTemplateIds ??= [];
                book.LearnedSpellTemplateIds.RemoveAll(drop.Contains);
                book.LearnedSpellTemplateIds.AddRange(add);
                WizardCollection.UpdateCharacter(wizard.CharId, saved => {
                    var list = saved.SpellbookBehavior.LearnedSpellTemplateIds ??= [];
                    list.RemoveAll(drop.Contains);
                    var known = list.ToHashSet();
                    list.AddRange(add.Where(known.Add));
                });
            }

            var clearers = Clearers.Where(n => wanted.Any(e => e.Shown.Equals(n, StringComparison.OrdinalIgnoreCase))).ToList();
            var epic = wanted.Any(e => e.Shown.Equals(EPIC, StringComparison.OrdinalIgnoreCase));
            Logger.Information("Zoom: his spellbook holds {0} Storm spells and {1} enchantments ({2} learned, {3} other-school spells taken out); "
                               + "Tempest {4}, Glowbug Squall {5}, Epic {6}.",
                Logger.Args(wanted.Count(Storm), wanted.Count(e => IsEnchant(e.Id)), add.Count, drop.Count,
                    clearers.Contains("Tempest") ? "in" : "NOT FOUND", clearers.Contains("Glowbug Squall") ? "in" : "NOT FOUND", epic ? "in" : "NOT FOUND"));
            return true;
        }
        catch (Exception e) {
            Logger.Warning("Zoom: could not fill his spellbook: {0}", Logger.Args(e.Message));
            return false;
        }
    }

    /// <summary>
    /// His deck, by spell and copies: Epic, Tempest and Glowbug Squall as many as the deck allows, then his biggest Storm hits
    /// (two of each), then his other Storm spells, until it is full. Only spells in his book. Null while the spells are read.
    /// </summary>
    internal static Dictionary<uint, int> Deck(Wizard wizard, int room, int schoolMost, int genericMost) {
        if (!TrueFormPool.Ready || wizard?.SpellbookBehavior is not { } book || room <= 0) {
            return null;
        }

        var learned = (book.LearnedSpellTemplateIds ?? []).Select(id => (uint) id).ToHashSet();
        var mine = Wanted().Where(e => learned.Contains(e.Id)).ToList();
        var deck = new Dictionary<uint, int>();
        void Put(TrueFormPool.Entry e, int copies) {
            if (e is null || deck.ContainsKey(e.Id)) {
                return;
            }

            var most = Storm(e) ? schoolMost : genericMost;
            var n = Math.Min(Math.Min(copies, Math.Max(1, most)), room - deck.Values.Sum());
            if (n > 0) {
                deck[e.Id] = n;
            }
        }

        TrueFormPool.Entry Named(string name) => mine.Where(e => e.Shown.Equals(name, StringComparison.OrdinalIgnoreCase)).OrderByDescending(e => e.Level).FirstOrDefault();

        // The best damage enchantment when the game has no Epic.
        Put(Named(EPIC) ?? mine.Where(e => BotEnchants.Of(e.Id) is { Kind: BotEnchants.DAMAGE }).OrderByDescending(e => BotEnchants.Of(e.Id).Amount).FirstOrDefault(), 99);
        foreach (var name in Clearers) {
            Put(Named(name), 99);
        }

        foreach (var hit in mine.Where(e => Storm(e) && e.Card.Hit && e.Card.PerPip == 0).OrderByDescending(e => e.Card.HitsAll).ThenByDescending(e => e.Card.BaseDamage)) {
            Put(hit, 2);
        }

        foreach (var other in mine.Where(e => Storm(e)).OrderByDescending(e => e.Level)) {
            Put(other, 1);
        }

        return deck;
    }

}
