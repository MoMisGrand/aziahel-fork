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
using Imlight.CoreLib.Game.Combat;
using Imlight.CoreLib.Game.Zone.Components;
using Imlight.CoreLib.Shared.Resources;
using Card = Imlight.CoreLib.Game.Combat.RewrittenAI.Card;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// Ultimus Core (Niduus, 2026-11-14: "Ultimus Core" picked): the battle brain Ultimus grew, shared so the Quest Team and
/// the Bot Players think with it too. Each bot keeps its own goals and its own jobs (Ultimus plays alone, the Quest Team
/// plays its set team's roles, street bots fight like players); what they share is how a fight is read: the creatures'
/// habits from the shared brain (BotKnowledge), who heals and must go first, what is coming at them this round, a hit
/// estimated with pierce, and planning two rounds ahead. Every lesson a bot writes carries who it played with (solo or
/// its team), so solo data and team data never mix. Each bot also has its own mood (BotMood).
/// </summary>
internal static class BotCore {

    /// <summary>A card in a hand: its place in the hand, what it does, and whether the pips are there now.</summary>
    internal sealed record InHand(int Index, Card Card, bool Castable);

    /// <summary>How a bot weighs the next round: what a round later is worth, and how much less when risky; a trap's size when unknown.</summary>
    internal sealed record Lookahead(double Delay, double Risky, float TrapGuess);

    /// <summary>A two-round plan's first move (null First: save the pips), its target and the reason.</summary>
    internal sealed record Plan(InHand First, int Target, string Why);

    /// <summary>What the shared brain knows of a creature's habits.</summary>
    internal static BotKnowledge.Habit HabitOf(CombatDuelSubCircle enemy)
        => enemy?.ParticipantObject is { } who ? BotKnowledge.Of((uint) Math.Min((ulong) who.m_templateID, uint.MaxValue)) : null;

    /// <summary>The enemy that heals at least this share of its casts (most first), or null: it goes first.</summary>
    internal static CombatDuelSubCircle Healer(IEnumerable<CombatDuelSubCircle> enemies, double share)
        => enemies.Select(e => (Who: e, Habit: HabitOf(e)))
            .Where(x => x.Habit is not null && x.Habit.Casts >= 4 && x.Habit.Chance(BotKnowledge.HEAL) >= share)
            .OrderByDescending(x => x.Habit.Chance(BotKnowledge.HEAL)).Select(x => x.Who).FirstOrDefault();

    /// <summary>The share of a bot's health the enemies' habits say they take this round, and who hits hardest.</summary>
    internal static (double Share, string Hardest) Incoming(IEnumerable<CombatDuelSubCircle> enemies, int round, Func<CombatDuelSubCircle, string> name) {
        double total = 0;
        var hardest = ("", 0.0);
        foreach (var enemy in enemies) {
            if (HabitOf(enemy) is not { Casts: >= 3 } habit) {
                continue;
            }

            var expect = habit.ExpectedHitOn(round);
            total += expect;
            if (expect > hardest.Item2) {
                hardest = (name(enemy), expect);
            }
        }

        return (total, hardest.Item1.Length > 0 ? hardest.Item1 : "they");
    }

    /// <summary>
    /// The pips an X-pip hit (Judgement: more pips, more damage) would be paid with once every pip slot is full: what it
    /// would take now, plus one for each empty slot (a new pip is worth at least one). Niduus, 2026-11-21: "trap and blade
    /// stack as much as possible, then hit with a full pip enchanted judgement for the kill"; "Judgement gets more damage the
    /// more pips he has" (he cast it at 1 power pip on the Sea Lord, about 446).
    /// </summary>
    internal static int FullPips(Card card, CombatDuelSubCircle me) {
        try {
            var now = card?.Spell is null ? RewrittenAI.PipsOf(me) : CombatActionResolver.GetXPipCost(card.Spell, me);
            return now + PipSlotsLeft(me);
        }
        catch (Exception) {
            return RewrittenAI.PipsOf(me) + PipSlotsLeft(me);
        }
    }

    /// <summary>His empty pip slots (seven in all; a power pip takes one slot).</summary>
    internal static int PipSlotsLeft(CombatDuelSubCircle me) {
        var pips = me?.CombatParticipant?.m_pipCount;
        return pips is null ? 0 : Math.Max(0, CombatDuelSubCircle.MAX_PIP_COUNT - pips.m_genericPips - pips.m_powerPips);
    }

    /// <summary>A hit as it would land: an X-pip hit with full pips (FullPips), any other as it is now.</summary>
    internal static int EstimateFull(Card card, CombatDuelSubCircle me, CombatDuelSubCircle target, float blades = 0, float traps = 0)
        => Estimate(card, me, target, blades, traps, card is { XPip: true } ? FullPips(card, me) : null);

    /// <summary>The damage boost (percent) the battlefield's global gives a hit of this school now; 0 with none.</summary>
    internal static int GlobalUp(CombatDuelSubCircle me, string school) {
        try {
            var bubbles = me?._duelActor?.Duel?.m_duelModifier?.m_battlefieldEffects;
            if (bubbles is null || string.IsNullOrEmpty(school)) {
                return 0;
            }

            lock (bubbles) {
                return bubbles.Where(x => x is not null && x.m_effectType == kSpellEffects.kModifyOutgoingDamage
                                          && Fits((string) x.m_sDamageType, school)).Sum(x => x.m_effectParam);
            }
        }
        catch (Exception) {
            return 0;
        }
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, (string School, int Percent)> s_globals = new();

    /// <summary>
    /// The damage a global card puts on the battlefield for one school (or all): (school, percent), or (null, 0) when the
    /// card is no damage global (Niduus, 2026-11-21: "He should be able to cast Globals as well").
    /// </summary>
    internal static (string School, int Percent) GlobalOf(Card card) {
        if (card is null || card.Hit) {
            return (null, 0);
        }

        return s_globals.GetOrAdd(card.Id, id => {
            try {
                if (CoreObjectFactory.GetCoreTemplate(id) is not SpellTemplate template) {
                    return (null, 0);
                }

                var best = ((string) null, 0);
                foreach (var e in CombatEnchantments.Leaves(template.m_effects ?? []).Where(e => e is not null && e.m_effectTarget == kEffectTarget.kGlobal
                                                                                                && e.m_effectType == kSpellEffects.kModifyOutgoingDamage && e.m_effectParam > 0)) {
                    if (e.m_effectParam > best.Item2) {
                        best = (((string) e.m_sDamageType) ?? "", e.m_effectParam);
                    }
                }

                return best;
            }
            catch (Exception) {
                return (null, 0);
            }
        });
    }

    /// <summary>
    /// The biggest hit an enemy has landed on bots (a share of their health), when it holds the pips for a hit that big now
    /// (BIG_HIT_PIPS, a power pip worth 2), and who it is; else 0. The Sea Lord (2026-11-21 21:23) saved 1 + 6 power pips
    /// over two rounds of small hits, then took Ultimus from 1,638 to 4 health with one Kraken hit while he stacked a blade.
    /// </summary>
    internal static (double Share, string Who) BigHitReady(IEnumerable<CombatDuelSubCircle> enemies, Func<CombatDuelSubCircle, string> name) {
        var best = (0.0, "");
        foreach (var enemy in enemies) {
            if (enemy is not { IsAlive: true } || RewrittenAI.PipsOf(enemy) < BIG_HIT_PIPS || HabitOf(enemy) is not { HitsOnBots: >= 2 } habit) {
                continue;
            }

            var share = Math.Min(1.0, habit.BiggestHitShare);
            if (share > best.Item1) {
                best = (share, name(enemy));
            }
        }

        return best;
    }

    internal const int BIG_HIT_PIPS = 7;

    /// <summary>
    /// The damage over time on this participant that ticks at the start of its next turn, before its card (the game's
    /// order), as a share of its health. Heck Hound's burn knocked Ultimus out at 4 health before his shield went up.
    /// </summary>
    internal static double BurnOn(CombatDuelSubCircle circle) {
        if (circle?._hangingEffects is not { } hanging) {
            return 0;
        }

        lock (hanging) {
            var tick = hanging.Where(x => x is not null && x.m_effectType == kSpellEffects.kDamageOverTime && x.m_numRounds > 0)
                .Sum(x => Math.Max(0, x.m_paramPerRound));

            return tick / (double) RewrittenAI.MostHealth(circle);
        }
    }

    /// <summary>
    /// A hit as the game lands it on this target: its resist taken down by the caster's pierce for that school (resist
    /// can't go under nothing).
    /// </summary>
    internal static int Estimate(Card card, CombatDuelSubCircle me, CombatDuelSubCircle target, float blades = 0, float traps = 0, int? xPips = null) {
        var raw = RewrittenAI.Estimate(card, me, target, blades, traps, xPips: xPips);
        // The global on the battlefield for the hit's school (Darkwind, Supercharge...): the server adds it (GetGlobalEffect),
        // the AI's estimate does not.
        if (GlobalUp(me, card.DamageSchool) is var bubble and not 0) {
            raw = (int) Math.Max(0, raw * (1 + (bubble / 100.0)));
        }

        var resist = RewrittenAI.ResistOf(target, card.DamageSchool);
        // A hit that lands as one of several schools at random (Spectral Blast: Fire, Ice or Storm) is worth the average of
        // them on this target, not the first one's (Niduus, 2026-11-21: "He also should not be using Elemental Blast ...
        // because it's a 1 of 3 to land on storm. If he's to use offbrand damage or spells, he should focus on maximum
        // damage"): it was rated as Fire against the Storm Sea Lord, which resists Storm.
        if (RandomSchools(card) is { Count: >= 2 } schools && resist < 0.95f) {
            var plain = raw / Math.Max(0.05, 1 - Through(resist, me, card.DamageSchool));
            return (int) schools.Average(s => plain * (1 - Through(RewrittenAI.ResistOf(target, s), me, s)));
        }

        if (resist <= 0 || resist >= 0.95f) {
            return raw;
        }

        var through = Math.Min(resist, PierceOf(me, card.DamageSchool));

        return (int) (raw * (1 - (resist - through)) / (1 - resist));
    }

    // The resist a hit of this school meets, after the caster's pierce (a weakness, under nothing, stays as it is).
    private static double Through(float resist, CombatDuelSubCircle me, string school)
        => resist <= 0 ? resist : Math.Min(0.95, resist - Math.Min(resist, PierceOf(me, school)));

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, IReadOnlyList<string>> s_randomSchools = new();

    /// <summary>The schools a hit lands as at random (one of its random choices each), or empty for a hit of one school.</summary>
    internal static IReadOnlyList<string> RandomSchools(Card card) {
        if (card is not { Hit: true }) {
            return [];
        }

        return s_randomSchools.GetOrAdd(card.Id, id => {
            try {
                if (CoreObjectFactory.GetCoreTemplate(id) is not SpellTemplate template) {
                    return [];
                }

                var schools = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                void Walk(IEnumerable<SpellEffect> effects, bool inRandom) {
                    foreach (var e in effects ?? []) {
                        switch (e) {
                            case null:
                                break;
                            case RandomSpellEffect random:
                                Walk(random.m_effectList, true);
                                break;
                            case EffectListSpellEffect list:
                                Walk(list.m_effectList, inRandom);
                                break;
                            default:
                                if (inRandom && CombatCharms.DamageEffects.Contains(e.m_effectType) && ((string) e.m_sDamageType) is { Length: > 0 } school) {
                                    schools.Add(school);
                                }
                                break;
                        }
                    }
                }

                Walk(template.m_effects, false);
                return schools.Count >= 2 ? [.. schools] : [];
            }
            catch (Exception) {
                return [];
            }
        });
    }

    internal static float PierceOf(CombatDuelSubCircle circle, string school) {
        try {
            var stats = circle?.ParticipantGameStats;

            return stats is null ? 0 : Math.Clamp(circle.GetStatBySchool(stats.m_apBonusPercent, school) + stats.m_apBonusPercentAll, 0f, 1f);
        }
        catch (Exception) {
            return 0;
        }
    }

    /// <summary>
    /// The card's blade or trap would count on <paramref name="on"/>: none of the same card from the same source with the
    /// same enchantment is there yet (the game's rule, Niduus, 2026-11-21: "normal blades can stack with TC ones, traps
    /// too"; "A Sharpen Blade Bladestorm DOES stack with non-Enchant and Treasure Card versions. Item cards too stack!": a
    /// Hex from the deck, a Hex treasure card, a Hex item card and a Sharpened Hex all count; two Hexes from the deck do not).
    /// </summary>
    internal static bool Stacks(CombatDuelSubCircle on, Card card) {
        if (on?._hangingEffects is not { } hanging || card is null) {
            return false;
        }

        var enchantment = card.Spell?.m_enchantment ?? 0;
        var source = SourceOf(card.Spell);
        lock (hanging) {
            return !hanging.Any(x => x is not null && x.m_spellTemplateID == card.Id && x.m_enchantmentSpellTemplateID == enchantment
                                     && SourceOf(x) == source);
        }
    }

    /// <summary>
    /// Where a card or a hanging charm came from as the server counts it for stacking (0 the deck, 1 a treasure card, 2 an
    /// item card). Read through reflection, so these bots also run on a server from before the stacking fix (0.10.960):
    /// there every copy counts as the deck's, just as that server plays it, and nothing waits for a full server update.
    /// </summary>
    internal static int SourceOf(Spell spell) => s_spellSource(spell);

    internal static int SourceOf(SpellEffect effect) => s_effectSource(effect);

    private static readonly Func<Spell, int> s_spellSource = SourceReader<Spell>();
    private static readonly Func<SpellEffect, int> s_effectSource = SourceReader<SpellEffect>();

    private static Func<T, int> SourceReader<T>() {
        try {
            var method = typeof(CombatCharms).GetMethod("SourceOf", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
                                                                    | System.Reflection.BindingFlags.NonPublic, [typeof(T)]);
            if (method is not null && method.ReturnType == typeof(int)) {
                return method.CreateDelegate<Func<T, int>>();
            }
        }
        catch (Exception) {
            // An older server: every copy counts as the deck's.
        }

        return _ => 0;
    }

    /// <summary>How many blades on <paramref name="me"/> and traps on <paramref name="target"/> are stacked now.</summary>
    internal static int Stacked(CombatDuelSubCircle me, CombatDuelSubCircle target) {
        int Count(CombatDuelSubCircle c, kSpellEffects type) {
            if (c?._hangingEffects is not { } hanging) {
                return 0;
            }

            lock (hanging) {
                return hanging.Count(x => x is not null && x.m_effectType == type && x.m_effectParam > 0);
            }
        }

        return Count(me, kSpellEffects.kModifyOutgoingDamage) + Count(target, kSpellEffects.kModifyIncomingDamage);
    }

    /// <summary>
    /// Stacking up for one big hit against a foe that one hit can't end (Niduus, 2026-11-21: "Train feint, use that feint,
    /// then a TC feint into a blade then hit"; "look for the most maximum damage possible ... look at the maximum outcome he
    /// can pump out"). Each round: the biggest hit he can have ready soon, and every blade (on himself) or trap (on the
    /// foe) in hand that still stacks and fits that hit's school. A setup that adds enough to that hit is played, the one
    /// that adds most first; when none is left, or the stack is full, or the hit with all of it ends the foe, it is time to
    /// hit (null: the hit, heal and pip-saving rules take over). Pips are kept for the big hit.
    /// </summary>
    internal static Plan StackUp(CombatDuelSubCircle me, List<InHand> hand, List<InHand> whole, CombatDuelSubCircle focus,
                                 List<CombatDuelSubCircle> enemies, Func<CombatDuelSubCircle, string> Name, int most = STACK_MOST) {
        if (focus is null) {
            return null;
        }

        var pips = RewrittenAI.PipsOf(me);
        var health = RewrittenAI.Health(focus);
        // The big hit: the best he holds that his pips reach now or next round.
        var big = whole.Where(h => h.Card.Hit && !h.Card.Natural && !h.Card.HitsAll && (h.Card.XPip || h.Card.Rank <= pips + 1))
            .Select(h => (Hand: h, Damage: EstimateFull(h.Card, me, focus))).OrderByDescending(x => x.Damage).FirstOrDefault();
        if (big.Hand is null || big.Damage <= 0 || big.Damage >= health) {
            return null;
        }

        var stacked = Stacked(me, focus);
        if (stacked >= most) {
            return null;
        }

        var school = big.Hand.Card.DamageSchool;
        var setups = new List<(InHand Hand, int Target, int Gain, string Text)>();
        // An X-pip setup (Backdraft, Fire's X-pip trap; Niduus, 2026-11-21: "they potentially can use Backdraft as their feint,
        // but that's also X pip") takes every pip he has: only while he has few (2 or less), so it doesn't eat the pips the
        // finisher is saving.
        foreach (var h in hand.Where(h => h.Castable && !h.Card.Hit && (h.Card.XPip ? pips <= X_SETUP_PIPS : h.Card.Rank + big.Hand.Card.Rank <= pips + 2))) {
            if (h.Card.Blade && !h.Card.BladeOnlyTeammate && Fits(h.Card.BladeSchool, school) && Stacks(me, h.Card)) {
                var gain = EstimateFull(big.Hand.Card, me, focus, h.Card.BladePercent, 0) - big.Damage;
                setups.Add((h, h.Card.BladeWholeTeam ? -1 : me.SlotIndex, gain, $"{h.Card.Shown} on myself (+{h.Card.BladePercent}%)"));
            }
            else if (h.Card.Trap && Fits(h.Card.TrapSchool, school) && Stacks(focus, h.Card) && h.Card.TrapPercent > 0) {
                var gain = EstimateFull(big.Hand.Card, me, focus, 0, h.Card.TrapPercent) - big.Damage;
                setups.Add((h, h.Card.TrapAll ? -1 : focus.SlotIndex, gain, $"{h.Card.Shown} on {Name(focus)} (+{h.Card.TrapPercent}%)"));
            }
            // A global for the big hit's school, stronger than the one up, and none the enemies hit with (it boosts them too):
            // it lasts the whole fight for him and his team.
            else if (GlobalOf(h.Card) is { Percent: > 0 } bubble && Fits(bubble.School, school) && bubble.Percent > GlobalUp(me, school)
                     && !enemies.Any(e => e.IsAlive && Fits(bubble.School, RewrittenAI.SchoolOf(e)))) {
                var gain = (int) (big.Damage * (bubble.Percent - GlobalUp(me, school)) / 100.0);
                setups.Add((h, -1, gain, $"{h.Card.Shown} on the battlefield (+{bubble.Percent}% for the whole fight, my team too)"));
            }
        }

        // Worth a round: it adds at least a fifth of the hit (a 20% blade on a 1,000 hit adds 200).
        var best = setups.Where(x => x.Gain >= big.Damage * STACK_GAIN).OrderByDescending(x => x.Gain).FirstOrDefault();
        if (best.Hand is null) {
            return null;
        }

        var after = big.Damage + best.Gain;
        return new Plan(best.Hand, best.Target,
            $"stacks up for one big hit: {best.Text} now, so {big.Hand.Card.Shown} goes from about {big.Damage} to about {after} " +
            $"({stacked + 1} stacked; {Name(focus)} has {health:N0} health, too much for one hit yet)");
    }

    internal const int STACK_MOST = 5;
    private const int X_SETUP_PIPS = 2;
    private const double STACK_GAIN = 0.2;

    internal static bool Fits(string charmSchool, string hitSchool)
        => string.IsNullOrEmpty(charmSchool) || charmSchool.Equals("All", StringComparison.OrdinalIgnoreCase) || RewrittenAI.Same(charmSchool, hitSchool);

    /// <summary>
    /// Two rounds ahead (Niduus, 2026-11-14, "All" to: "before each round, he scores his whole hand over two rounds ahead
    /// and plays the best line, not just the best single card"): hit now and hit again; a blade now and a bigger hit next
    /// round; a trap now and a bigger hit next round; or his pips saved now for a bigger spell next round. Each line is worth
    /// the damage it deals over both rounds (no more than the creatures have), a round later counting a bit less, and much
    /// less when they could take him under his heal line first. The best line's first move is played, its reason said.
    /// </summary>
    internal static Plan TwoRounds(CombatDuelSubCircle me, List<InHand> hand, List<InHand> whole, CombatDuelSubCircle focus,
                                   List<CombatDuelSubCircle> enemies, int round, bool risky, Func<CombatDuelSubCircle, string> Name, Lookahead numbers) {
        var hits = whole.Where(h => h.Card.Hit && !h.Card.Natural).ToList();
        if (hits.Count == 0) {
            return null;
        }

        var pips = RewrittenAI.PipsOf(me);
        var later = numbers.Delay * (risky ? numbers.Risky : 1);
        // A creature that tends to shield or ward on this round soaks a hit now; building up is worth more.
        var focusHabit = HabitOf(focus);
        var shieldsNow = focusHabit is not null && focusHabit.HabitRound(BotKnowledge.SHIELD) == Math.Clamp(round, 1, 5);
        var lines = new List<(double Value, InHand First, int Target, string Text)>();

        double Deal(InHand h, float blade, float trap, int healthLeft) => h.Card.HitsAll
            ? enemies.Sum(e => Math.Min(RewrittenAI.Health(e), Estimate(h.Card, me, e, blade, trap)))
            : Math.Min(healthLeft, Estimate(h.Card, me, focus, blade, trap));

        bool CanNext(InHand h, int spent, InHand used) => h != used && h.Card.Rank <= pips - spent + 1;

        var focusHealth = RewrittenAI.Health(focus);

        // Hit now, then hit again.
        foreach (var now in hits.Where(h => h.Castable)) {
            var first = Deal(now, 0, 0, focusHealth) * (shieldsNow && !now.Card.HitsAll ? 0.6 : 1);
            var left = now.Card.HitsAll ? focusHealth : Math.Max(0, focusHealth - (int) first);
            // When the first hit finishes the target, the next one goes on the toughest one left.
            var other = enemies.Where(e => e != focus).OrderByDescending(RewrittenAI.Health).FirstOrDefault();
            var next = hits.Where(h => CanNext(h, now.Card.Rank, now)).Select(h => (Hand: h, Damage: left > 0 || h.Card.HitsAll
                ? Deal(h, 0, 0, left)
                : other is null ? 0 : Math.Min(RewrittenAI.Health(other), Estimate(h.Card, me, other)))).OrderByDescending(x => x.Damage).FirstOrDefault();
            var value = first + (next.Damage * later);
            lines.Add((value, now, now.Card.HitsAll ? -1 : focus.SlotIndex,
                $"{now.Card.Shown} now (about {first:0}){(next.Hand is null ? "" : $", then {next.Hand.Card.Shown} (about {next.Damage:0})")}"));
        }

        // A blade on myself now, then a bigger hit.
        foreach (var blade in hand.Where(h => h.Card.Blade && !h.Card.BladeOnlyTeammate && Fits(h.Card.BladeSchool, RewrittenAI.SchoolOf(me))
                                              && Stacks(me, h.Card))) {
            var next = hits.Where(h => CanNext(h, blade.Card.Rank, blade) && Fits(blade.Card.BladeSchool, h.Card.DamageSchool))
                .Select(h => (Hand: h, Damage: Deal(h, blade.Card.BladePercent, 0, focusHealth))).OrderByDescending(x => x.Damage).FirstOrDefault();
            if (next.Hand is not null) {
                lines.Add((next.Damage * later, blade, blade.Card.BladeWholeTeam ? -1 : me.SlotIndex,
                    $"{blade.Card.Shown} on myself now (+{blade.Card.BladePercent}%), then {next.Hand.Card.Shown} (about {next.Damage:0})"));
            }
        }

        // A trap on the target now, then a bigger hit.
        foreach (var trap in hand.Where(h => h.Card.Trap && Stacks(focus, h.Card))) {
            var percent = trap.Card.TrapPercent > 0 ? trap.Card.TrapPercent : numbers.TrapGuess;
            var next = hits.Where(h => CanNext(h, trap.Card.Rank, trap) && Fits(trap.Card.TrapSchool, h.Card.DamageSchool))
                .Select(h => (Hand: h, Damage: Deal(h, 0, percent, focusHealth))).OrderByDescending(x => x.Damage).FirstOrDefault();
            if (next.Hand is not null) {
                lines.Add((next.Damage * later, trap, trap.Card.TrapAll ? -1 : focus.SlotIndex,
                    $"{trap.Card.Shown} on {Name(focus)} now (+{percent:0}%), then {next.Hand.Card.Shown} (about {next.Damage:0})"));
            }
        }

        // Pips saved now for a bigger spell next round.
        var saved = hits.Where(h => !h.Castable && h.Card.Rank <= pips + 1)
            .Select(h => (Hand: h, Damage: Deal(h, 0, 0, focusHealth))).OrderByDescending(x => x.Damage).FirstOrDefault();
        if (saved.Hand is not null) {
            lines.Add((saved.Damage * later, null, -1, $"saving my pips now, then {saved.Hand.Card.Shown} (about {saved.Damage:0})"));
        }

        // A line worth nothing (a hit the target is immune to, or that its wards soak whole) is never played: it spends the pips
        // and the turn for nothing, and the heal, shield or pass after it is the better move. Ultimus against the Sea Lord,
        // 2026-11-21 18:17: round 3, "Ice Hound now (about 0)", the boss lost no health, and its next hits took him to half.
        lines.RemoveAll(l => l.Value <= 0);
        if (lines.Count == 0) {
            return null;
        }

        var ordered = lines.OrderByDescending(l => l.Value).ThenBy(l => l.First is null ? 1 : 0).ToList();
        var best = ordered[0];
        var runnerUp = ordered.Skip(1).FirstOrDefault(l => l.Text != best.Text);
        var compare = runnerUp.Text is null ? "" : $"; better than {runnerUp.Text}, worth {runnerUp.Value:0}";
        var habitNote = shieldsNow ? $"; {Name(focus)} tends to shield on round {Math.Clamp(round, 1, 5)}, so building up is worth more now" : "";
        var riskNote = risky ? "; they could hurt me before then, so a later hit counts for less" : "";
        var why = $"plans two rounds: {best.Text}, worth about {best.Value:0}{compare}{habitNote}{riskNote}";

        return new Plan(best.First, best.Target, why);
    }


}
