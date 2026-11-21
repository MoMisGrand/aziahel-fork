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
using Imlight.CoreLib.Game.BotShared;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.Game.World;
using Imlight.CoreLib.Game.Zone.Components;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Ultimus;

/// <summary>
/// Ultimus's trips to the trainers (forked from the Quest Team's): it learns Balance spells as its level allows them, and
/// spends training points on what its own fights showed it needs (BotTrainingPlan). Nothing is given: it walks there and
/// trains like a player, by the game's own trainer rules.
/// </summary>
internal static class UltimusTraining {

    internal const string TRAINER_ZONE = "WizardCity/WC_Ravenwood";

    internal sealed record Lesson(uint TrainerTemplate, uint SpellId, string Spell, float X, float Y, string Zone = TRAINER_ZONE);

    /// <summary>
    /// What this wizard can learn right now, own school first: at the trainers of the area it stands in (any trainer the
    /// team can reach, Niduus, 2026-11-12: "able to visit other trainers that are accessible and learn spells that are
    /// accessible at their level and if they have a training point"), then at the Ravenwood trainers.
    /// </summary>
    internal static List<Lesson> Learnable(UltimusMember.Status member) {
        var lessons = new List<Lesson>();
        if (member.Wizard is not { } wizard) {
            return lessons;
        }

        var zones = new List<string>();
        if (!string.IsNullOrEmpty(member.Zone) && member.Wizard?.ZoneIsInstance != true) {
            zones.Add(member.Zone);
        }

        if (!zones.Any(z => string.Equals(z, TRAINER_ZONE, StringComparison.OrdinalIgnoreCase))) {
            zones.Add(TRAINER_ZONE);
        }

        // Every other area a trainer stands in that he can reach (TrainerZones).
        zones.AddRange(TrainerZones(member.Zone).Where(z => !zones.Contains(z, StringComparer.OrdinalIgnoreCase)));

        try {
            foreach (var zone in zones) {
                Add(zone);
            }
        }
        catch (Exception e) {
            Imlight.Common.Logger.Debug("Ultimus: could not read the trainers: {0}", Imlight.Common.Logger.Args(e.Message));
        }

        return [.. lessons.DistinctBy(l => l.SpellId)
            .OrderBy(l => string.Equals(SchoolOfSpell(l.SpellId), member.Role, StringComparison.OrdinalIgnoreCase) ? 0 : 1)];

        void Add(string zone) {
            var placed = QuestStepSpots.Zone(zone).Objects ?? [];
            // Outside his school: only the next spell of the plan his own fights made (BotTrainingPlan).
            var next = BotTrainingPlan.NextId(wizard, member.CharId);
            foreach (var trainer in VanillaTrainers.All) {
                var spot = placed.FirstOrDefault(o => o.Template == trainer.TemplateId);
                if (spot.Template != trainer.TemplateId) {
                    continue;
                }

                foreach (var spell in trainer.Spells.Where(s => s.Kind == "School")) {
                    if (wizard.SpellbookBehavior.HasSpell(spell.Id) || CoreObjectFactory.GetCoreTemplate(spell.Id) is not SpellTemplate template) {
                        continue;
                    }

                    var own = string.Equals(spell.School, member.Role, StringComparison.OrdinalIgnoreCase);
                    var wanted = next != 0 && spell.Id == next;
                    if (!own && !wanted) {
                        continue;
                    }

                    if (!VanillaTrainers.Judge(wizard, spell, template).CanTrain) {
                        continue;
                    }

                    lessons.Add(new Lesson(trainer.TemplateId, spell.Id, UltimusText.SpellName(spell.Id), spot.Spot.X, spot.Spot.Y, zone));
                }
            }
        }
    }

    private static readonly Dictionary<int, (DateTime At, List<string> Zones)> s_trainerZones = [];

    /// <summary>
    /// The areas the real game's trainers stand in that a wizard in <paramref name="fromZone"/> can reach: the main worlds up
    /// to the one it stands in (Wizard City always), from the NPC location book. Only Ravenwood's square and the area he
    /// stood in were looked at before, but most school teachers stand inside their school houses (Moolinda Wu in
    /// WC_SchoolLife, Lydia Greyrose in WC_SchoolFrost, Dworgyn in WC_SchoolDeath) and the Balance ones in Krokotopia
    /// (Alhazred, Niles): in two days Ultimus never once trained a spell from them, kept 10 training points, wanted Satyr
    /// 570 times, and at level 37 still lacked Power Play, Spectral Blast, Tri Blade, Spirit Blade and the Balance traps
    /// (Niduus, 2026-11-21: "Ultimus seems to had forgot to train spells that he deems are required").
    /// </summary>
    internal static List<string> TrainerZones(string fromZone) {
        var reach = Math.Max(1, MainWorlds.Number(fromZone));
        lock (s_trainerZones) {
            if (s_trainerZones.TryGetValue(reach, out var known) && (known.Zones.Count > 0 || DateTime.UtcNow - known.At < TimeSpan.FromMinutes(2))) {
                return known.Zones;
            }
        }

        var zones = new List<string>();
        foreach (var trainer in VanillaTrainers.All) {
            foreach (var place in UltimusNpcBook.Of(trainer.TemplateId, fromZone)) {
                var world = MainWorlds.Number(place.Zone);
                if (world < 1 || world > reach || place.Zone.Contains("_Instanced", StringComparison.OrdinalIgnoreCase)
                    || zones.Contains(place.Zone, StringComparer.OrdinalIgnoreCase)) {
                    continue;
                }

                zones.Add(place.Zone);
            }
        }

        lock (s_trainerZones) {
            s_trainerZones[reach] = (DateTime.UtcNow, zones);
        }

        return zones;
    }

    /// <summary>
    /// Where a training trip goes when no trainer stands here: the area with a lesson in this world first (a shorter trip),
    /// then the one with the most lessons waiting.
    /// </summary>
    internal static string TripZone(IEnumerable<string> lessonZones, string fromZone) {
        var world = (fromZone ?? "").Split('/')[0];

        return lessonZones.Where(z => !string.IsNullOrEmpty(z))
            .GroupBy(z => z, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => string.Equals(g.Key.Split('/')[0], world, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(g => g.Count())
            .Select(g => g.Key).FirstOrDefault() ?? TRAINER_ZONE;
    }

    /// <summary>The index of a spell in a trainer's list (as its training window shows it), or -1.</summary>
    internal static int IndexAt(UltimusMember.Status member, ulong trainerGlobalId, uint spellId) {
        var mark = MinimapIndex.In(member.ZoneActor).FirstOrDefault(m => m.Object.m_globalID.Full == trainerGlobalId);
        var trainer = mark?.Entity?.GetComponentOfType<InteractTrainerComponent>();

        return trainer?.SpellInventory?.FindIndex(e => (uint) e.TemplateID == spellId) ?? -1;
    }

    private static string SchoolOfSpell(uint id)
        => CoreObjectFactory.GetCoreTemplate(id) is SpellTemplate template ? (string) template.m_sMagicSchoolName ?? "" : "";

}
