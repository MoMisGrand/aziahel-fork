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
using Akka.Actor;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.Game.Zone.Core;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// A dungeon room that holds a fight step's creature, read from the room's own spawn files (shared by Ultimus, the TCO7 and
/// the Quest Team). A boss that waits off-screen in another room of the dungeon (it comes out once its battle circle is
/// stepped on) is not on the map until then, and the quest names no area for it, so the quest arrow, which points at the
/// teleporter into that room, pulled the bots back out of the very room the boss is in. The Tree of Life, 2026-11-20:
/// Nomoonaga waits in the Spirit World room behind "Activator Volume (1)"; Ultimus went in, read the room as the wrong
/// area, backed out and went in again for 20 minutes. Any dungeon room built the same way is covered.
/// </summary>
internal static class BotRoomFoes {

    /// <summary>The creature tags a fight step asks for (empty when the step is not a fight).</summary>
    internal static string[] AdjectivesOf(string quest, string goal) {
        if (string.IsNullOrEmpty(quest) || string.IsNullOrEmpty(goal)
            || QuestTemplateCollection.GetQuestByName(quest)?.m_goals?.FirstOrDefault(g => g is not null && (string) g.m_goalName == goal)
                is not BountyGoalTemplate bounty) {
            return [];
        }

        return [.. (bounty.m_npcAdjectives ?? []).Select(a => (string) a).Where(a => !string.IsNullOrEmpty(a))];
    }

    /// <summary>
    /// The quest of an open fight step whose creature this room's spawn files bring out (null when none): a room of the
    /// dungeon the step is played in, so walking in was the right way, not a wrong turn to back out of.
    /// </summary>
    internal static string StepFoeRoom(Wizard wizard, string zone) {
        try {
            foreach (var quest in wizard?.QuestBehavior?.CurrentQuestInstances?.Where(q => q is not null).ToList() ?? []) {
                foreach (var goal in QuestTemplateCollection.GetQuestByName(quest.QuestName)?.m_goals?.OfType<BountyGoalTemplate>() ?? []) {
                    if (quest.IsGoalActive((string) goal.m_goalName)
                        && SpawnsIn(zone, [.. (goal.m_npcAdjectives ?? []).Select(a => (string) a).Where(a => !string.IsNullOrEmpty(a))])) {
                        return quest.QuestName;
                    }
                }
            }
        }
        catch (Exception) {
            // A quest list changing under it: no answer this time.
        }

        return null;
    }

    /// <summary>
    /// The room a door of this area leads to whose own spawn files hold a creature with one of these tags ("" when none):
    /// the step's boss waits behind that door (the Tree of Life's spirit portal leads to Kagemoosha's Spirit World room).
    /// </summary>
    internal static string RoomBehindDoor(IEnumerable<MinimapIndex.Mark> live, string zone, IReadOnlyCollection<string> adjectives) {
        if (live is null || adjectives is not { Count: > 0 }) {
            return "";
        }

        return live.Where(m => m.Kind == MinimapIndex.DOOR && !string.IsNullOrEmpty(m.To)
                               && !string.Equals(m.To.Trim(), zone, StringComparison.OrdinalIgnoreCase)
                               && OverlayQuestRoute.IsDungeon(m.To.Trim()))
            .Select(m => m.To.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(room => SpawnsIn(room, adjectives)) ?? "";
    }

    /// <summary>Whether the room's own spawn files bring out a creature with one of these tags.</summary>
    internal static bool SpawnsIn(string zone, IReadOnlyCollection<string> adjectives) {
        if (string.IsNullOrEmpty(zone) || adjectives is not { Count: > 0 }) {
            return false;
        }

        try {
            return QuestStepSpots.Zone(zone).Spawned.Any(id => CoreObjectFactory.GetCoreTemplate(id) is GameObjectTemplate template
                && (template.m_adjectiveList ?? []).Any(a => adjectives.Contains((string) a, StringComparer.Ordinal)));
        }
        catch (Exception) {
            return false;
        }
    }

    /// <summary>
    /// The room's battle circle whose fight has not started yet in this copy, nearest first (null when none): the step's
    /// creature comes out on it.
    /// </summary>
    internal static BotFloors.Circle WaitingCircle(string zone, IActorRef zoneActor, float x, float y) {
        if (string.IsNullOrEmpty(zone)) {
            return null;
        }

        var state = ZoneStates.Of(zoneActor);
        return BotFloors.BookOf(zone).Circles
            .Where(c => (state?.GetTriggerFires(c.Trigger) ?? 0) == 0 && state?.IsTriggerEnabled(c.Trigger) != false)
            .OrderBy(c => BotFloors.Far(x, y, c.X, c.Y))
            .FirstOrDefault();
    }

}
