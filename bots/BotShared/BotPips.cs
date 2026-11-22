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
using Imlight.CoreLib.Game.Combat;
using Imlight.CoreLib.Game.Spells;
using Imlight.CoreLib.Shared.Behaviors;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// How school pips work, for every bot (Ultimus, the Calling of 7, the Kitharas, the Quest Team and the Bot Players), as
/// the game does it (Niduus, 2026-11-22: "Have Ultimus know how School Pips work and how to change them in battle. This is
/// also shared to everyone else"):
///   - Some cards cost school pips: Celestial Intervention's treasure card costs 2 pips and an Ice pip. Only a school pip
///     of that school pays it ([Battle] AnySchoolPips lets any school's pay).
///   - School pips come from the Power Orb (Archmastery). A wizard has it from level 50, or earlier with Archmastery on gear,
///     jewels or pets. Each round the orb fills; each full point turns one power pip into a school pip of the orb's school.
///     Without the orb no school pip ever comes, of any school ([Battle] GainSchoolPips gives everyone a slower gauge).
///   - The orb's school is the deck's pick, else his own school, and a click on the orb changes it in battle: the next
///     school pips come in the new school. Any of the seven schools can be picked, so a card of another school's pip is
///     castable once the orb is turned to that school and a power pip turns.
///   - A school pip of the spell's own school counts like a power pip toward its rank; one of another school counts only
///     as a normal pip. So the orb stays on his own school, and turns away only while a card in hand needs that school's
///     pips, then back.
/// </summary>
internal static class BotPips {

    private static readonly (string School, MagicSchool Id)[] s_schools = [
        ("Balance", MagicSchool.Balance), ("Death", MagicSchool.Death), ("Fire", MagicSchool.Fire), ("Ice", MagicSchool.Ice),
        ("Life", MagicSchool.Life), ("Myth", MagicSchool.Myth), ("Storm", MagicSchool.Storm),
    ];

    private static bool AnySchool => ConfigurationManager.GetValue("Battle.AnySchoolPips", false);
    private static bool Gauge => ConfigurationManager.GetValue("Battle.GainSchoolPips", false);

    /// <summary>The school pips a card costs, by school (only the ones above 0).</summary>
    internal static List<(string School, int Cost)> Costs(SpellRank rank) {
        if (rank is null) {
            return [];
        }

        (string School, int Cost)[] all = [
            ("Balance", rank.m_balancePips), ("Death", rank.m_deathPips), ("Fire", rank.m_firePips), ("Ice", rank.m_icePips),
            ("Life", rank.m_lifePips), ("Myth", rank.m_mythPips), ("Storm", rank.m_stormPips),
        ];

        return [.. all.Where(c => c.Cost > 0)];
    }

    /// <summary>
    /// Whether a wizard of this level gets school pips at all, before any gear: the Power Orb from level 50 (or every level
    /// with [Archmastery] AllLevels), or the server's school pip gauge. Archmastery on gear, jewels or pets is seen in battle.
    /// </summary>
    internal static bool OrbAt(int level)
        => Gauge || Archmastery.AllLevels || (Archmastery.FromLevel && level >= Archmastery.LEVEL) || Archmastery.BonusRating > 0;

    /// <summary>
    /// A card he can never cast for want of school pips: it costs some, and at his level no Power Orb gives him any. With
    /// the orb every school's pips can be had (it is turned to that school in battle), so nothing is out of reach.
    /// Celestial Intervention's treasure card (an Ice pip) for Ultimus at level 38: he saved his pips for it round after
    /// round against Vasek Ashweaver and never cast it (2026-11-22 01:00-01:20).
    /// </summary>
    internal static bool OutOfReach(SpellRank rank, int level)
        => Costs(rank).Count > 0 && !OrbAt(level);

    /// <summary>In this battle: whether he has the Power Orb (or the server's gauge), so school pips come.</summary>
    internal static bool OrbIn(CombatDuelSubCircle me) {
        try {
            return Gauge || me?.ArchmasteryRating > 0;
        }
        catch {
            return false;
        }
    }

    private static int Have(PipCount pips, string school) => school switch {
        "Balance" => pips.m_balancePips,
        "Death" => pips.m_deathPips,
        "Fire" => pips.m_firePips,
        "Ice" => pips.m_icePips,
        "Life" => pips.m_lifePips,
        "Myth" => pips.m_mythPips,
        "Storm" => pips.m_stormPips,
        _ => 0,
    };

    private static string NameOf(uint id) => s_schools.FirstOrDefault(s => (uint) s.Id == id).School;

    /// <summary>
    /// Turns the Power Orb, as a player clicks it in the planning phase, toward the school pips his hand needs: the school
    /// a card in his hand lacks the most pips of, else his own school. Null when it stays as it is; else what he did and why.
    /// </summary>
    internal static string Turn(CombatDuelSubCircle me) {
        if (me?.CombatParticipant is not { } participant || !OrbIn(me) || AnySchool
            || ConfigurationManager.GetValue("Battle.RandomSchoolPips", false)) {
            return null;
        }

        var ownId = (uint) participant.m_primaryMagicSchoolID;
        var own = NameOf(ownId);
        if (own is null) {
            return null;
        }

        // What his hand still lacks, school by school, and the card that wants it most.
        var pips = participant.m_pipCount;
        var lack = new Dictionary<string, (int Pips, string Card)>(StringComparer.Ordinal);
        for (byte i = 0; i < 24; i++) {
            if (me.GetSpellFromLastHand(i) is not { } spell) {
                continue;
            }

            foreach (var (school, cost) in Costs(spell.m_pipCost)) {
                var missing = cost - Have(pips, school);
                if (missing <= 0) {
                    continue;
                }

                var name = RewrittenAI.Read(spell)?.Shown ?? "a card";
                lack[school] = lack.TryGetValue(school, out var known) ? (known.Pips + missing, known.Card) : (missing, name);
            }
        }

        // His own school first when it lacks pips too: they also count as power pips for his own spells.
        var want = lack.Count == 0 ? own
            : lack.ContainsKey(own) ? own
            : lack.OrderByDescending(l => l.Value.Pips).ThenBy(l => l.Key, StringComparer.Ordinal).First().Key;
        var wantId = (uint) s_schools.First(s => s.School == want).Id;
        uint now;
        try {
            now = me.ArchmasterySchool;
        }
        catch {
            return null;
        }

        if (now == wantId) {
            return null;
        }

        me.SetArchmasterySchool(wantId);

        return want == own
            ? lack.Count == 0
                ? $"turns its Power Orb back to {own}: no card in its hand needs another school's pips, and {own} pips count as power pips for its own spells"
                : $"turns its Power Orb to {own}: {lack[own].Card} needs {own} pips"
            : $"turns its Power Orb to {want}: {lack[want].Card} needs {lack[want].Pips} {want} pip(s), and the orb turns its power pips into pips of the school it is set to";
    }
}
