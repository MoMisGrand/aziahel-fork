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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imlight.CoreLib.Game.Tools;
using Imlight.CoreLib.Game.Zone.Components;

namespace Imlight.CoreLib.Game.BotShared;

/// <summary>
/// A quest thing that won't work yet because its boss or creatures are still standing (Niduus, 2026-11-14, Odji's Chamber:
/// "he has to defeat the boss to get the quest item ... the bot should recognize that he would try for the item and if it
/// fails, to fight the boss/mobs, if any, then try again after they are defeated"). A server-played wizard (Ultimus, the
/// Quest Team) tries the thing first, as some bosses can be skipped; when the game offers nothing to click there, or the
/// click does nothing, the creatures by it are fought, and then the thing is tried again. At most a few rounds a step.
/// </summary>
internal static class BotUseGuards {

    /// <summary>Creatures this close to the thing are the ones that guard it (a dungeon room).</summary>
    internal const float RANGE = 1500f;

    /// <summary>Rounds of "fight them, try again" for one step before the usual reports take over.</summary>
    private const int MOST_ROUNDS = 3;

    private sealed class Round {
        public string Zone = "";
        public float X;
        public float Y;
        public int Rounds;
    }

    private static readonly ConcurrentDictionary<string, Round> s_rounds = new(StringComparer.OrdinalIgnoreCase);

    private static string Key(ulong charId, string quest, string goal) => $"{charId}|{quest}|{goal}";

    /// <summary>The creatures (bosses too) within <see cref="RANGE"/> of (x, y), with those in no battle first.</summary>
    internal static List<MinimapIndex.Mark> Near(IActorRef zoneActor, float x, float y)
        => zoneActor is null
            ? []
            : [.. MinimapIndex.In(zoneActor).Where(m => m.Kind is MinimapIndex.MOB or MinimapIndex.BOSS
                                                        && Far(x, y, m.Object.m_location.X, m.Object.m_location.Y) <= RANGE)];

    /// <summary>
    /// The thing at (x, y) did not work for the step: true when creatures stand by it, so they are fought first (a round
    /// begins); false when there are none or the rounds for this step are used up.
    /// </summary>
    internal static bool Failed(ulong charId, string quest, string goal, string zone, IActorRef zoneActor, float x, float y) {
        if (Near(zoneActor, x, y).Count == 0) {
            return false;
        }

        var round = s_rounds.GetOrAdd(Key(charId, quest, goal), _ => new Round());
        lock (round) {
            if (round.X != 0 || round.Y != 0) {
                // A round already on: its creatures are still to beat.
                return true;
            }

            if (round.Rounds >= MOST_ROUNDS) {
                return false;
            }

            round.Rounds++;
            round.Zone = zone ?? "";
            round.X = x;
            round.Y = y;

            return true;
        }
    }

    /// <summary>
    /// While a round is on: the creature to fight next by the thing (the nearest free one to <paramref name="fromX"/>,
    /// <paramref name="fromY"/>), or null. <paramref name="cleared"/> is true once none stand there any more: the thing
    /// is tried again (its "used" marks forgotten).
    /// </summary>
    internal static MinimapIndex.Mark Next(ulong charId, string quest, string goal, string zone, IActorRef zoneActor, float fromX, float fromY,
        out bool cleared) {
        cleared = false;
        if (!s_rounds.TryGetValue(Key(charId, quest, goal), out var round) || round.X == 0 && round.Y == 0
            || !string.Equals(round.Zone, zone ?? "", StringComparison.OrdinalIgnoreCase)) {
            return null;
        }

        var near = Near(zoneActor, round.X, round.Y);
        var free = near.Where(m => !CombatDuelComponent.IsInBattle(m.Object.m_globalID.Full))
            .OrderBy(m => Far(fromX, fromY, m.Object.m_location.X, m.Object.m_location.Y)).FirstOrDefault();
        if (near.Count == 0) {
            cleared = true;
            lock (round) {
                round.X = 0;
                round.Y = 0;
            }

            BotCollect.ForgetStep(charId, quest, goal);
        }

        return free;
    }

    /// <summary>Whether a round is on for this step (its creatures still to beat).</summary>
    internal static bool On(ulong charId, string quest, string goal)
        => s_rounds.TryGetValue(Key(charId, quest, goal), out var round) && (round.X != 0 || round.Y != 0);

    private static float Far(float x1, float y1, float x2, float y2) => MathF.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

}
