using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Imcodec.IO;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Game.Services;
using Imlight.CoreLib.Game.Zone.Core;
using Imlight.CoreLib.Shared.Resources;
using Imlight.CoreLib.WizardData;
using Imlight.CoreLib.WizardData.Collections;
using Imlight.CoreLib.WizardData.Models.Player;

namespace Imlight.CoreLib.Game.Quests;

internal static class QuestLeftovers {
    private const float RADIUS = 900f;
    private static readonly Regex s_blocker = new(
        "(^|[^a-z])(ice|icy|frozen|freeze|frost|collision|barrier|blocker|block|boulder|rubble|stone|cage|web|thorn|bramble|brambles|lock|wall|shell|crystal|snow|sealed|gate)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex s_rescueTitle = new(
        "^\\s*(save|free|rescue|release|untie|unchain|cure|revive|wake|heal|cut|chop|clear|slash|smash|destroy)\\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly bool s_off =
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IMLIGHT_NO_AUTO_HIDE"));
    private static readonly object s_lock = new();
    private static readonly Dictionary<string, bool> s_rescueKeys = new(StringComparer.OrdinalIgnoreCase);
    private static Tuple<int, Dictionary<string, List<string>>> s_index =
        Tuple.Create(-1, new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase));
    private static Tuple<int, Dictionary<string, List<(string Quest, string Goal)>>> s_usage =
        Tuple.Create(-1, new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase));

    public static ResultList RescueHides(GoalTemplate goal) {
        if (s_off || goal is null || goal.m_goalType != GOAL_TYPE.GOAL_TYPE_USAGE) return null;
        var tags = goal.m_clientTags;
        if (tags is null || tags.Count == 0) return null;
        if ((goal.m_completeResults?.m_results ?? []).Any(r => r is ResAddDynaMod or ResRemoveDynaMod)) return null;

        var title = goal.m_goalTitle;
        if (string.IsNullOrWhiteSpace(title)) return null;
        var input = title.Contains(' ') ? title : (Locale.FindInAnyTable(title) ?? title);
        if (!s_rescueTitle.IsMatch(input)) return null;

        var results = new ResultList { m_results = [] };
        foreach (ByteString tag in tags) {
            var name = (string)tag;
            if (string.IsNullOrEmpty(name)) continue;
            results.m_results.Add(new ResAddDynaMod {
                m_dynaModClientTag = name.EndsWith(" instance", StringComparison.OrdinalIgnoreCase) ? name : name + " instance",
                m_dynaModState = "",
                m_zoneName = "",
                m_bShowsObject = false,
                m_dynaModRemove = true,
                m_persist = true,
            });
        }
        return results.m_results.Count > 0 ? results : null;
    }

    public static bool LooksLikeBlocker(string zoneTag)
        => !s_off && !string.IsNullOrEmpty(zoneTag) && s_blocker.IsMatch(zoneTag);

    private static string Norm(string tag) {
        if (tag is null) return null;
        return tag.EndsWith(" instance", StringComparison.OrdinalIgnoreCase)
            ? tag[..^" instance".Length]
            : tag;
    }

    public static bool ActiveUsageNames(Wizard wizard, string zoneTag) {
        var tag = Norm(zoneTag);
        if (s_off || wizard is null || string.IsNullOrEmpty(tag)) return false;
        foreach (var (usageTag, sources) in UsageGoals()) {
            if (string.Equals(Norm(usageTag), tag, StringComparison.OrdinalIgnoreCase)
                && sources.Any(g => GoalActive(wizard, g.Quest, g.Goal))) return true;
        }
        return false;
    }

    private static bool GoalActive(Wizard wizard, string quest, string goal) =>
        wizard.QuestBehavior?.CurrentQuestInstances?
            .FirstOrDefault(q => q != null && string.Equals(q.QuestName, quest, StringComparison.OrdinalIgnoreCase))
            ?.IsGoalActive(goal) ?? false;

    private static bool GoalDone(Wizard wizard, string quest, string goal) =>
        wizard.QuestBehavior?.CurrentQuestInstances?
            .FirstOrDefault(q => string.Equals(q.QuestName, quest, StringComparison.OrdinalIgnoreCase))
            ?.IsGoalCompleted(goal) ?? false;

    private static Dictionary<string, List<(string Quest, string Goal)>> UsageGoals() {
        if (s_usage.Item1 == QuestOptions.Version) return s_usage.Item2;
        lock (s_lock) {
            var version = QuestOptions.Version;
            if (s_usage.Item1 == version) return s_usage.Item2;
            var index = new Dictionary<string, List<(string, string)>>(StringComparer.OrdinalIgnoreCase);
            foreach (var quest in SpiralDB.QuestTemplates) {
                foreach (var goal in quest?.m_goals ?? []) {
                    if (goal is null || goal.m_goalType != GOAL_TYPE.GOAL_TYPE_USAGE) continue;
                    foreach (ByteString tag in goal.m_clientTags ?? []) {
                        var name = (string)tag;
                        if (string.IsNullOrEmpty(name)) continue;
                        if (!index.TryGetValue(name, out var goals)) index[name] = goals = [];
                        goals.Add((quest.m_questName, goal.m_goalName));
                    }
                }
            }
            s_usage = Tuple.Create(version, index);
            return index;
        }
    }

    public static bool ShouldHide(Zone zone, string zoneTag, Wizard wizard) {
        if (s_off || wizard is null || string.IsNullOrEmpty(zoneTag)) return false;
        var objects = zone?.ZoneData?.m_objectList;
        if (objects is null) return false;
        var target = objects.FirstOrDefault(o => string.Equals(o.m_zoneTag, zoneTag, StringComparison.OrdinalIgnoreCase));
        if (target is null) return false;
        foreach (var goalObject in objects) {
            var goalTag = (string?)goalObject?.m_zoneTag;
            if (string.IsNullOrEmpty(goalTag) || !UsageGoals().TryGetValue(Norm(goalTag), out var sources)) continue;
            var dx = goalObject.m_location.X - target.m_location.X;
            var dy = goalObject.m_location.Y - target.m_location.Y;
            var dz = goalObject.m_location.Z - target.m_location.Z;
            if (dx * dx + dy * dy + dz * dz <= RADIUS * RADIUS
                && sources.Any(g => GoalDone(wizard, g.Quest, g.Goal))) return true;
        }
        return false;
    }

    public static List<string> FinishedUsageBlockerTags(string zonePath, GoalTemplate goal) {
        var result = new List<string>();
        if (string.IsNullOrEmpty(zonePath) || goal is null || goal.m_goalType != GOAL_TYPE.GOAL_TYPE_USAGE) return result;
        if (!ResourceManager.TryLoadArchive(zonePath, out var wad)) return result;
        var data = ResourceManager.OpenZoneData(wad)?.ToArray();
        if (data is null || !new BindSerializer().Deserialize<WizZoneData>(data, 1u, out var zone) || zone?.m_objectList is null) return result;

        var usageTags = new HashSet<string>(goal.m_clientTags?.Select(t => Norm((string)t)) ?? [], StringComparer.OrdinalIgnoreCase);
        foreach (var usage in zone.m_objectList) {
            if (!usageTags.Contains(Norm((string?)usage?.m_zoneTag))) continue;
            var blocker = zone.m_objectList
                .Where(o => !string.IsNullOrEmpty((string?)o?.m_zoneTag) && LooksLikeBlocker((string)o.m_zoneTag))
                .Select(o => (Object: o, Distance: DistanceSquared(o.m_location, usage.m_location)))
                .Where(x => x.Distance <= 400000000d)
                .OrderBy(x => x.Distance)
                .FirstOrDefault().Object;
            if (blocker is null) continue;
            var name = (string)blocker.m_zoneTag;
            result.Add(name);
            result.Add(name.EndsWith(" instance", StringComparison.OrdinalIgnoreCase) ? name : name + " instance");
        }
        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static ResultList FinishedUsageHides(string zonePath, GoalTemplate goal) {
        if (s_off || string.IsNullOrEmpty(zonePath) || goal is null || goal.m_goalType != GOAL_TYPE.GOAL_TYPE_USAGE) return null;
        if (!ResourceManager.TryLoadArchive(zonePath, out var wad)) return null;
        var data = ResourceManager.OpenZoneData(wad)?.ToArray();
        if (data is null || !new BindSerializer().Deserialize<WizZoneData>(data, 1u, out var zone) || zone?.m_objectList is null) return null;

        var tags = new HashSet<string>(goal.m_clientTags?.Select(t => Norm((string)t)) ?? [], StringComparer.OrdinalIgnoreCase);
        foreach (var usage in zone.m_objectList) {
            if (!tags.Contains(Norm((string?)usage?.m_zoneTag))) continue;
            var blocker = zone.m_objectList
                .Where(o => !string.IsNullOrEmpty((string?)o?.m_zoneTag) && LooksLikeBlocker((string)o.m_zoneTag))
                .Select(o => (Object: o, Distance: DistanceSquared(o.m_location, usage.m_location)))
                .Where(x => x.Distance <= 25000000d)
                .OrderBy(x => x.Distance)
                .FirstOrDefault().Object;
            if (blocker is not null) tags.Add(Norm((string)blocker.m_zoneTag));
        }

        var results = new ResultList { m_results = [] };
        foreach (var tag in tags) {
            results.m_results.Add(new ResAddDynaMod {
                m_dynaModClientTag = tag + " instance",
                m_dynaModState = "",
                m_zoneName = "",
                m_bShowsObject = false,
                m_dynaModRemove = true,
                m_persist = true,
            });
        }
        return results.m_results.Count > 0 ? results : null;
    }

    public static List<string> HideTagsBlockingActiveUsage(Wizard wizard) {
        var result = new List<string>();
        if (s_off) return result;
        foreach (var dynamod in wizard?.DynamodSet?.Dynamods ?? []) {
            if (dynamod is not null && !string.IsNullOrEmpty(dynamod.ClientTag)
                && IsUsageHide(dynamod.ModState) && ActiveUsageNames(wizard, dynamod.ClientTag)) {
                result.Add(dynamod.ClientTag);
            }
        }
        return result;
    }

    public static void ReleaseHidesBlockingActiveUsage(Wizard wizard) {
        if (wizard is null) return;
        foreach (var tag in HideTagsBlockingActiveUsage(wizard)) wizard.RemoveDynamod(tag);
    }

    public static void ReleaseHidesBlockingActivePersonaGoals(Wizard wizard) {
        if (wizard is null) return;
        var personas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var quest in wizard.QuestBehavior?.CurrentQuestInstances ?? []) {
            QuestTemplate questTemplate;
            try { questTemplate = QuestTemplateCollection.GetQuestByName(quest.QuestName); }
            catch { continue; }
            foreach (var progress in quest.GoalProgress ?? []) {
                if (quest.IsGoalActive(progress.GoalName)
                    && questTemplate?.m_goals?.FirstOrDefault(g => g.m_goalName == progress.GoalName) is PersonaGoalTemplate persona
                    && !string.IsNullOrEmpty(persona.m_personaName)) {
                    personas.Add(NormalizePersona(persona.m_personaName));
                }
            }
        }
        foreach (var dynamod in wizard.DynamodSet?.Dynamods?.ToList() ?? []) {
            if (dynamod is null || string.IsNullOrEmpty(dynamod.ClientTag)) continue;
            if (personas.Contains(NormalizePersona(Norm(dynamod.ClientTag)))) wizard.RemoveDynamod(dynamod.ClientTag);
        }
    }

    private static bool IsUsageHide(string state) =>
        string.IsNullOrEmpty(state) || state.Equals("Off", StringComparison.OrdinalIgnoreCase)
        || state.Equals("Gone", StringComparison.OrdinalIgnoreCase);

    private static string NormalizePersona(string name) {
        if (string.IsNullOrEmpty(name)) return "";
        name = Norm(name);
        return name.EndsWith("_Persona", StringComparison.OrdinalIgnoreCase)
            ? name[..^"_Persona".Length]
            : name;
    }

    private static double DistanceSquared(Imcodec.Math.Vector3 a, Imcodec.Math.Vector3 b) {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return dx * dx + dy * dy + dz * dz;
    }
}
