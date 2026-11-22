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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Game.BotShared;
using Imlight.CoreLib.Game.Combat;
using Imlight.CoreLib.Game.Spells;
using Imlight.CoreLib.Shared.Character;
using Imlight.CoreLib.Shared.Resources;
using CombatResolver = Imlight.CoreLib.Game.Combat.CombatResolver;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// How Zoom, the quest scout, fights (Niduus, 2026-11-22: "able to one shot all enemies as much as he sees fit"). His hand is
/// dealt each round as True Form's is (TrueFormBattle), from every spell at the top level: the dev spells that one-shot
/// first (one that hits every enemy when more than one stands), then the biggest real hits that hit all and that hit one.
/// He casts the one that ends the fight soonest: a one-shot when he has one he can pay for, else the biggest hit, at the
/// boss or the creature with the most health. Nothing else: no blades, no heals, no shields. [Zoom] OneShot = false keeps
/// him to real spells only (live).
/// </summary>
internal static class ZoomBattle {

    private const int HAND = TrueFormBattle.HAND;

    /// <summary>Whether he may use the dev one-shots ([Zoom] OneShot, on unless turned off; live).</summary>
    internal static bool OneShots => ConfigurationManager.GetValue("Zoom.OneShot", true);

    // Whether a spell's effects land on enemies, and on all of them, read once per spell.
    private static readonly ConcurrentDictionary<uint, (bool Enemy, bool All)> s_reach = new();

    private static (bool Enemy, bool All) Reach(uint id) => s_reach.GetOrAdd(id, key => {
        try {
            if (CoreObjectFactory.GetCoreTemplate(key) is not SpellTemplate t) {
                return (false, false);
            }

            var leaves = CombatEnchantments.Leaves(t.m_effects ?? []).Where(x => x is not null).ToList();
            var all = leaves.Any(x => x.m_effectTarget is kEffectTarget.kEnemyTeam or kEffectTarget.kEnemyTeamAllAtOnce or kEffectTarget.kMultiTargetEnemy);
            var one = leaves.Any(x => x.m_effectTarget is kEffectTarget.kEnemySingle);
            return (all || one, all);
        }
        catch (Exception) {
            return (false, false);
        }
    });

    // His spells at the top level: the one-shots he may use, then the real hits (no damage-per-pip spells, which do
    // nothing with few pips, and no treasure cards: a dealt hand never runs out).
    private static List<TrueFormPool.Entry> OneShotsNow()
        => OneShots ? [.. TrueFormPool.DevOneShotList.Where(e => Reach(e.Id).Enemy && SpellFactory.GetSpell(e.Id) is not null)] : [];

    private static List<TrueFormPool.Entry> RealHits() {
        var top = Math.Clamp(MagicLevelsConfig.MaxLevel, 1, 255);
        return [.. TrueFormPool.At(top, dev: false).Where(e => e.Card.Hit && e.Card.PerPip == 0 && !e.Treasure && !BotCards.NotVanilla(e.Card))];
    }

    /// <summary>
    /// His seven cards for this round in place of the hand he drew, or null when there is nothing to deal yet (his spells
    /// are still being read). Says what was dealt.
    /// </summary>
    internal static string Deal(CombatDuelSubCircle me, IReadOnlyList<CombatDuelSubCircle> circles, int round) {
        if (me?._combatDeck is not { } deck || !TrueFormPool.Ready) {
            return null;
        }

        var pips = RewrittenAI.PipsOf(me);
        var many = circles.Count(c => c.OccupiedTeam != me.OccupiedTeam && c.IsAlive) > 1;
        var ids = new List<uint>();
        void Add(IEnumerable<TrueFormPool.Entry> list, int most) {
            foreach (var e in list.Take(most)) {
                if (ids.Count < HAND && !ids.Contains(e.Id)) {
                    ids.Add(e.Id);
                }
            }
        }

        // The one-shots he can pay for now, the ones that hit every enemy first when more than one stands.
        var shots = OneShotsNow().Where(e => e.Card.Rank <= pips)
            .OrderByDescending(e => many && Reach(e.Id).All ? 1 : 0).ThenBy(e => e.Card.Rank).ToList();
        Add(shots, 3);
        var hits = RealHits();
        var castable = hits.Where(e => e.Card.Rank <= pips).ToList();
        Add(castable.Where(e => e.Card.HitsAll).OrderByDescending(e => e.Card.BaseDamage), many ? 2 : 1);
        Add(castable.Where(e => !e.Card.HitsAll).OrderByDescending(e => e.Card.BaseDamage), 2);
        // The rest of the hand: the biggest hits for when his pips are up.
        Add(hits.OrderByDescending(e => e.Card.BaseDamage), HAND);
        if (ids.Count == 0) {
            return null;
        }

        deck.ReturnHandToDeck();
        var shown = new List<string>();
        foreach (var id in ids) {
            if (SpellFactory.GetSpell(id) is not { } spell) {
                continue;
            }

            deck.AddExtraCardToHand(spell);
            shown.Add(TrueFormPool.Of(id)?.Shown ?? RewrittenAI.ReadTemplate(id)?.Shown ?? id.ToString());
        }

        return $"Zoom is dealt {(shots.Count > 0 ? "one-shots and " : "")}his biggest hits ({pips} pips): {string.Join(", ", shown)}";
    }

    /// <summary>His pick for the round: the card that ends the fight soonest, at the boss or the creature with the most health.</summary>
    internal static UltimusTactics.Choice Decide(CombatDuelSubCircle me, IReadOnlyList<CombatDuelSubCircle> circles, CombatResolver resolver, int round) {
        var enemies = circles.Where(c => c.OccupiedTeam != me.OccupiedTeam && c.IsAlive).ToList();
        if (enemies.Count == 0) {
            return new(UltimusTactics.Move.Pass, 0, -1, "no enemy is left");
        }

        var target = enemies.OrderByDescending(e => RewrittenAI.IsBoss(e) ? 1 : 0).ThenByDescending(RewrittenAI.Health).First();
        var many = enemies.Count > 1;
        var hand = UltimusTactics.Hand(me).Where(h => h.Castable).ToList();

        var shot = hand.Where(h => TrueFormPool.IsDevOneShot(h.Card.Id) && Reach(h.Card.Id).Enemy)
            .OrderByDescending(h => many && Reach(h.Card.Id).All ? 1 : 0).FirstOrDefault();
        if (shot is not null) {
            var all = Reach(shot.Card.Id).All;
            return new(UltimusTactics.Move.Cast, shot.Index, all ? -1 : target.SlotIndex,
                $"one-shots {(all ? "every enemy" : UltimusTactics.Name(target))} with {shot.Card.Shown} (quest scout: fastest way through)");
        }

        var hits = hand.Where(h => h.Card.Hit).ToList();
        var wide = many ? hits.Where(h => h.Card.HitsAll).OrderByDescending(h => h.Card.BaseDamage).FirstOrDefault() : null;
        if (wide is not null) {
            return new(UltimusTactics.Move.Cast, wide.Index, -1, $"hits every enemy with {wide.Card.Shown}");
        }

        var big = hits.OrderByDescending(h => h.Card.BaseDamage).FirstOrDefault();
        if (big is not null) {
            return new(UltimusTactics.Move.Cast, big.Index, big.Card.HitsAll ? -1 : target.SlotIndex,
                $"hits {(big.Card.HitsAll ? "every enemy" : UltimusTactics.Name(target))} with {big.Card.Shown}");
        }

        return new(UltimusTactics.Move.Pass, 0, -1, "builds pips for his next big hit");
    }

}
