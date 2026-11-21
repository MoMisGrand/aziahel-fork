using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Imlight.CoreLib.Shared.Packets;

namespace Imlight.CoreLib.Game.Zone.Core;

/// <summary>
/// The dungeon quest progress of one instance: the quests it finished and the goals it completed, and the zones
/// its container holds. Every zone of an instance container shares one, so a quest keeps its progress when the
/// player moves between the instance's zones, and the zones tell which quests belong to the instance.
/// </summary>
public sealed class InstanceQuestProgress {

    private readonly HashSet<string> _claims = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _completedQuests = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _startedQuests = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> _completedGoals = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _zones = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _goalCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lock = new();

    /// <summary>
    /// Records that the container holds the zone.
    /// </summary>
    public void AddZone(string zonePath) {
        lock (_lock) {
            _zones.Add(zonePath);
        }
    }

    /// <summary>
    /// Records that the container dropped the zone.
    /// </summary>
    public void RemoveZone(string zonePath) {
        lock (_lock) {
            _zones.Remove(zonePath);
        }
    }

    /// <summary>
    /// Records a quest step and returns whether it is the first time the instance saw it.
    /// </summary>
    /// <summary>Only these rooms stay the dungeon's (the run is over when none is left).</summary>
    public void KeepOnly(IReadOnlyCollection<string> zonePaths) {
        lock (_lock) {
            _zones.RemoveWhere(z => !zonePaths.Contains(z, StringComparer.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// A fresh run of a dungeon: what the last run did on these quests is forgotten, so they start from their first step
    /// in the fresh rooms. Returns how many quests had something to forget.
    /// </summary>
    public int ForgetQuests(IEnumerable<string> questNames) {
        var forgotten = 0;
        lock (_lock) {
            foreach (var quest in questNames) {
                if (string.IsNullOrEmpty(quest)) {
                    continue;
                }

                var mid = "|" + quest + "|";
                var had = _completedGoals.Remove(quest) | _completedQuests.Remove(quest) | _startedQuests.Remove(quest);
                had |= _claims.RemoveWhere(c => c.Contains(mid, StringComparison.OrdinalIgnoreCase)) > 0;
                foreach (var key in _goalCounts.Keys.Where(k => k.Contains(mid, StringComparison.OrdinalIgnoreCase)).ToList()) {
                    had |= _goalCounts.Remove(key);
                }

                if (had) {
                    forgotten++;
                }
            }
        }

        return forgotten;
    }

    public bool TryClaimStep(InstanceQuestClaimKind kind, string questName, string goalName) {
        lock (_lock) {
            if (!_claims.Add($"{kind}|{questName}|{goalName}")) {
                return false;
            }

            switch (kind) {
                case InstanceQuestClaimKind.GoalComplete:
                    if (!_completedGoals.TryGetValue(questName, out var goals)) {
                        goals = [];
                        _completedGoals[questName] = goals;
                    }

                    goals.Add(goalName);
                    break;
                case InstanceQuestClaimKind.QuestStart:
                    _startedQuests.Add(questName);
                    break;
                case InstanceQuestClaimKind.QuestComplete:
                    _completedQuests.Add(questName);
                    break;
            }

            return true;
        }
    }

    /// <summary>A player's count on a counted step of this run (the highest one is kept).</summary>
    public void NoteGoalCount(ulong charId, string questName, string goalName, int count) {
        if (string.IsNullOrEmpty(questName) || string.IsNullOrEmpty(goalName) || count <= 0) {
            return;
        }

        lock (_lock) {
            var key = $"{charId}|{questName}|{goalName}";
            if (!_goalCounts.TryGetValue(key, out var had) || count > had) {
                _goalCounts[key] = count;
            }
        }
    }

    /// <summary>
    /// The zones of the container whose dungeon quests apply in <paramref name="here"/>. A dungeon's rooms share its quests,
    /// but one owner's container also holds his rooms that are not a dungeon at all (a school house, the Headmistress's
    /// house): such a room only has its own quests. Before, Katzenstein's Lab's quests went to anyone in the owner's
    /// Headmistress House while his teammates were still in his Lab copy, and walking out took them away again: Kitharas
    /// Andromeda came for Ambrose's "Colossal Trouble" and was sent out after Firegut, 439 times from 09:25 on 2026-11-21.
    /// A zone the zone list does not know keeps every zone, as before.
    /// </summary>
    internal static string[] ZonesSharingQuests(string here, IEnumerable<string> zones) {
        var all = (zones ?? []).ToArray();
        if (string.IsNullOrEmpty(here) || !NotADungeon(here)) {
            return all;
        }

        return [here];
    }

    // The zone list marks the zone as a street or a room ("S", "T"), not a dungeon ("D").
    private static bool NotADungeon(string zone) {
        var known = World.ZoneNames.All.FirstOrDefault(z => z.Code.Equals(zone, StringComparison.OrdinalIgnoreCase));

        return known is not null && (known.Type == "S" || known.Type == "T");
    }

    /// <summary>
    /// The quests this instance finished and, per quest, the goals it completed in order.
    /// </summary>
    public ZONE_102_PROTOCOL.MSG_QUERYINSTANCEQUESTSRSP Snapshot() {
        lock (_lock) {
            return new() {
                IsInstance = true,
                Zones = [.. _zones],
                CompletedQuests = [.. _completedQuests],
                StartedQuests = [.. _startedQuests],
                CompletedGoals = _completedGoals.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()),
                GoalCounts = new Dictionary<string, int>(_goalCounts, StringComparer.OrdinalIgnoreCase),
            };
        }
    }

}
