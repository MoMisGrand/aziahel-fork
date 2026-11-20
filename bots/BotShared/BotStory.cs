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
using System.Threading;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// Which quests the Quest Team and Ultimus work on (Niduus, 2026-11-13: "Some quests are marked as main quests and they
/// do not always allow you to reject picking them up. Unless they are a Story Main quest, they are to be picked up but
/// IGNORED unless required to complete to advance in the Story Main Quest Line"). Story quests and dungeon quests are
/// played; any other quest (main or side) is played only when a story quest needs it done first. A main quest is
/// always taken when offered (the game may not let it be left), then ignored unless needed.
/// </summary>
internal static class BotStory {

    /// <summary>The rank of a quest the bots leave alone.</summary>
    internal const int IGNORED = -1;

    private static readonly Lock s_lock = new();
    private static HashSet<string> s_needed;
    private static int s_version = -1;

    /// <summary>How much the bots want a quest: 3 story, 2 dungeon, 1 needed by the story first, -1 ignored.</summary>
    internal static int Rank(QuestTemplate quest) {
        if (quest is null) {
            return IGNORED;
        }

        return QuestStory.KindOf(quest) switch {
            QuestStory.STORY => 3,
            QuestStory.INSTANCE => 2,
            _ => NeededByStory(quest.m_questName) ? 1 : IGNORED,
        };
    }

    /// <summary>Whether the bots work on this quest at all.</summary>
    internal static bool Plays(QuestTemplate quest) => Rank(quest) > 0;

    /// <summary>Whether to take an offered quest: one they play, or any main quest (taken, then ignored when not needed).</summary>
    internal static bool Takes(QuestTemplate quest) => quest is not null && (Plays(quest) || QuestStory.IsMain(quest) || KeepsSide(quest));

    /// <summary>
    /// Side quests offered as an optional pickup are declined (Niduus, 2026-11-15: "for side quests, it is an option to pick
    /// them up, if so, auto decline. We will be changing this later for Spell quests"). The one place to let a kind of side
    /// quest through: none for now. A side quest the story needs first is played (Plays) and is not declined.
    /// </summary>
    internal static bool KeepsSide(QuestTemplate quest) => false;

    /// <summary>Whether a story quest needs this quest done first (any depth, side quests too).</summary>
    internal static bool NeededByStory(string questName) {
        if (string.IsNullOrEmpty(questName)) {
            return false;
        }

        var version = Quests.QuestOptions.Version;
        HashSet<string> needed;
        lock (s_lock) {
            needed = s_version == version ? s_needed : null;
        }

        if (needed is null) {
            needed = BuildNeeded();
            lock (s_lock) {
                (s_needed, s_version) = (needed, version);
            }
        }

        return needed.Contains(questName);
    }

    /// <summary>
    /// A dungeon the story needs that the wizard skipped: its quest is not done, every quest it needs is done (or comes
    /// earlier in the same dungeon), and the furthest world first. Walking in gives its first quest, as it does a player.
    /// Cyndr left Katzenstein's Lab without "If You Build It..." and "It's Alive!", so "Fetch Bones!" (which needs them)
    /// was never offered and he stood with "no World or Story quest left" (2026-11-20). Null when there is none.
    /// </summary>
    internal static (string Zone, QuestTemplate Quest)? SkippedDungeon(Wizard wizard) {
        if (wizard is null) {
            return null;
        }

        (string Zone, QuestTemplate Quest)? best = null;
        var bestRank = (World: -1, Chain: int.MaxValue);
        foreach (var quest in QuestTemplateCollection.GetAllQuests()) {
            if (quest is null || !DungeonQuestIndex.IsDungeonQuest(quest.m_questName) || !NeededByStory(quest.m_questName)
                || Quests.QuestOptions.IsOff(quest.m_questName) || wizard.HasQuest(quest.m_questName) || wizard.HasCompletedQuest(quest.m_questName)
                || InZone(quest) is not { Length: > 0 } zone) {
                continue;
            }

            var world = MainWorlds.Number(zone.Split('/')[0]);
            if (world <= 0) {
                continue;
            }

            var chain = 0;
            var ready = true;
            foreach (var need in QuestGuide.NeededQuests(quest.m_requirements)) {
                if (wizard.HasCompletedQuest(need)) {
                    continue;
                }

                if (DungeonQuestIndex.IsDungeonQuest(need) && QuestTemplateCollection.GetQuestByName(need) is { } before
                    && string.Equals(InZone(before), zone, StringComparison.OrdinalIgnoreCase)) {
                    chain++;
                    continue;
                }

                ready = false;
                break;
            }

            if (!ready) {
                continue;
            }

            if (world > bestRank.World || (world == bestRank.World && chain < bestRank.Chain)) {
                best = (zone, quest);
                bestRank = (world, chain);
            }
        }

        return best;
    }

    private static string InZone(QuestTemplate quest) {
        foreach (var requirement in quest?.m_requirements?.m_requirements ?? []) {
            if (requirement is ReqInZone { m_applyNOT: false } inZone && !string.IsNullOrEmpty((string) inZone.m_zoneName)) {
                return ((string) inZone.m_zoneName).Trim();
            }
        }

        return "";
    }

    private static HashSet<string> BuildNeeded() {
        var needed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try {
            var waiting = new Stack<string>(QuestStory.Story());
            while (waiting.Count > 0) {
                if (QuestTemplateCollection.GetQuestByName(waiting.Pop()) is not { } quest) {
                    continue;
                }

                foreach (var need in QuestGuide.NeededQuests(quest.m_requirements)) {
                    if (needed.Add(need)) {
                        waiting.Push(need);
                    }
                }
            }
        }
        catch (Exception ex) {
            Imlight.Common.Logger.Warning("Bots: the quests the story needs could not be worked out: {0}", Imlight.Common.Logger.Args(ex.Message));
        }

        return needed;
    }
}
