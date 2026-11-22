/*
 * PEOPLE ONLY AN EARLIER STEP BRINGS OUT
 *
 * Some dungeons bring a person out on an event that an earlier step of the quest sends in that same copy of the
 * dungeon. Big Ben is one: after "Talk to Travis Pawman" in the first room, the event "MovePawman" (trigger
 * "MoveTravisPawman") takes him away and switches on his spawner on floor 5 (MB-MUSE3-NPC05_Maintenance), where
 * "Stealthy Stuff" needs him next. When the wizard came back in a new copy of Big Ben (a relog, a server restart, a
 * fresh copy after a reset), that event had gone to the old copy: floor 5 stood empty and the step could never be
 * done. TCO7 Garm was stuck there 92 times on 2026-11-20 and Idunn again on 2026-11-21.
 *
 * When a wizard enters a zone, or a step of theirs starts while they are in it, this looks for spawners of the zone
 * that start switched off and whose person is the one an open "talk to" step of the wizard needs. If that person is
 * held by exactly one spawner here and is not up, the spawner is switched on once for this copy, as the earlier
 * step's event would have done. Nothing is skipped (the wizard still has to talk to them), and the game files are
 * not changed.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Imcodec.ObjectProperty.TypeCache;
using Imlight.Common;
using Imlight.CoreLib.Game.Madlibs;
using Imlight.CoreLib.Shared.Networking;
using Imlight.CoreLib.Shared.Packets;
using Imlight.CoreLib.WizardData.Collections;

namespace Imlight.CoreLib.Game.Zone.Supervisors;

internal sealed partial class ZoneTriggerSupervisor {

    // Person template -> the one switched-off spawner of this zone that holds them (read once when the zone loads).
    private readonly Dictionary<ulong, SpawnObject> _eventPeopleSpawners = [];
    private readonly HashSet<ulong> _eventPeopleWoken = [];

    /*
     * When each person was last switched on here. On a street (one shared zone for everyone) the first wizard's fight
     * used the person up, and the woken list then kept them away from every later wizard on the same step for as long
     * as the server ran: Ultimus beat Iona Pyrelance in The Crucible at 03:40 on 2026-11-22 ("Justice Hurts", spawner
     * woken at 03:25), and from 04:55 TCO7 Ra stood at her spot with "no creature for this step" for over half an hour.
     * Now a person who is not up is switched on again for the next wizard who needs them, at most once every
     * EVENT_PEOPLE_AGAIN_SECONDS; the spawn still happens only when the spawner holds nobody (OnlyIfAbsent), so a
     * person who is up, or in a battle, is never doubled. Null in a zone that was running before this went in by a
     * live patch: that zone then switches the person on again whenever a wizard needs them (still only if absent).
     */
    private readonly Dictionary<ulong, DateTime> _eventPeopleWokenAt = [];
    private const int EVENT_PEOPLE_AGAIN_SECONDS = 60;

    private void NoteEventPeopleSpawners(IEnumerable<SpawnObject> spawners) {
        if (_eventPeopleSpawners is null || _eventPeopleWoken is null) {
            return;
        }

        _eventPeopleSpawners.Clear();
        try {
            var holders = new Dictionary<ulong, List<SpawnObject>>();
            foreach (var spawner in spawners ?? []) {
                if (spawner?.m_spawnList is null) {
                    continue;
                }

                foreach (var template in spawner.m_spawnList.Select(i => i?.m_objectInfo?.m_templateID.Full ?? 0).Where(t => t != 0).Distinct()) {
                    if (!holders.TryGetValue(template, out var list)) {
                        holders[template] = list = [];
                    }

                    list.Add(spawner);
                }
            }

            foreach (var (template, list) in holders) {
                // One spawner only, so the person never comes out twice; a spawner that starts on needs no help.
                if (list.Count == 1 && list[0] is { m_active: false } spawner) {
                    _eventPeopleSpawners[template] = spawner;
                }
            }
        }
        catch (Exception ex) {
            Logger.Debug("Zone {0}: the event people's spawners could not be read: {1}", Logger.Args(Zone.ZonePath, ex.Message));
        }
    }

    // A zone that was already running when this code went in by a live patch has no lists (its fields were added after it
    // was made, so they are null): it holds none. Reading them unguarded crashed every such zone's trigger supervisor on each
    // arrival, and doors and teleporters stopped taking anyone on (2026-11-21 00:31-00:46, v0.10.941 live patch).
    private bool HoldsEventPeopleSpawners()
        => _eventPeopleSpawners is { } spawners && _eventPeopleWoken is { } woken
            && spawners.Count > 0;

    // True when this person was switched on here before and may not be switched on again yet.
    private bool EventPersonWokenLately(ulong person) {
        if (!_eventPeopleWoken.Contains(person)) {
            return false;
        }

        // A zone patched in while running has no times: it asks again on every arrival or new step, which OnlyIfAbsent keeps
        // harmless (those come seconds apart at most, never in a loop).
        return _eventPeopleWokenAt is { } at && at.TryGetValue(person, out var last)
            && DateTime.UtcNow - last < TimeSpan.FromSeconds(EVENT_PEOPLE_AGAIN_SECONDS);
    }

    /*
     * A boss an earlier step summons is the same: the Summoning Crystal in Dragonspyre's Plaza of Conquests switches on the
     * spawner of the Earth Elemental or the Sea Lord, and a server restart switches it off again while the wizard's quest
     * already has the crystal step done. Ultimus stood at the Sea Lord's spot with "no creature for this step" after the
     * 12:55 restart on 2026-11-21. A "defeat" step for one foe whose creature is held by exactly one switched-off spawner
     * here gets it switched on the same way. Only a step for a single foe: "defeat 4" of a kind is never a summoned boss.
     */
    private ulong OneFoeOfStep(BountyGoalTemplate foe) {
        if (foe.m_bountyTotal > 1 || foe.m_npcAdjectives is not { Count: > 0 } wanted) {
            return 0;
        }

        var kinds = wanted.Select(a => (string) a).Where(a => !string.IsNullOrEmpty(a)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var matches = _eventPeopleSpawners.Keys.Where(template =>
            (Imlight.CoreLib.Shared.Resources.CoreObjectFactory.GetCoreTemplate((uint) template) as GameObjectTemplate)?.m_adjectiveList?
            .Any(a => kinds.Contains(a.ToString())) == true).Take(2).ToList();

        return matches.Count == 1 ? matches[0] : 0;
    }

    private void WakeEventPeopleForTalkSteps(WaitingSpawnWizard message) {
        if (!HoldsEventPeopleSpawners()) {
            return;
        }

        try {
            foreach (var instance in message.Wizard?.QuestBehavior?.CurrentQuestInstances ?? []) {
                var template = instance is null ? null : QuestTemplateCollection.GetQuestByName(instance.QuestName);
                if (template?.m_goals is null) {
                    continue;
                }

                foreach (var progress in instance.GoalProgress ?? []) {
                    if (progress is null || !progress.DoesPlayerHaveGoal() || progress.IsGoalCompleted()) {
                        continue;
                    }

                    var goal = template.m_goals.FirstOrDefault(g => g is not null
                        && string.Equals(g.m_goalName, progress.GoalName, StringComparison.Ordinal));
                    var person = goal switch {
                        PersonaGoalTemplate talk => QuestMadlibs.SpeakerTemplateOf(talk.m_personaName, 0),
                        BountyGoalTemplate foe => OneFoeOfStep(foe),
                        _ => 0UL,
                    };
                    if (person == 0 || EventPersonWokenLately(person) || !_eventPeopleSpawners.TryGetValue(person, out var spawner)) {
                        continue;
                    }

                    var again = !_eventPeopleWoken.Add(person);
                    if (_eventPeopleWokenAt is { } wokenAt) {
                        wokenAt[person] = DateTime.UtcNow;
                    }

                    if (again) {
                        Logger.Debug("Zone {0}: {1} switched on again for step '{2}' of '{3}' (spawner \"{4}\"), if nobody holds it.",
                            Logger.Args(Zone.ZonePath, CreatureName(person), (string) goal.m_goalName, instance.QuestName, (string) spawner.m_name));
                    }
                    else {
                        Logger.Information("Zone {0}: {1} is only brought out here by an event of an earlier step, sent to another copy "
                            + "of this zone; switched on now for step '{2}' of '{3}' (spawner \"{4}\").",
                            Logger.Args(Zone.ZonePath, CreatureName(person), (string) goal.m_goalName, instance.QuestName, (string) spawner.m_name));
                    }

                    ZoneRef.Tell(new ZONE_102_PROTOCOL.MSG_ZONEBROADCAST {
                        Messages = [new ZONE_102_PROTOCOL.MSG_ZONEPATHSPAWN {
                            SpawnObjectID = (uint) spawner.m_id.Full,
                            Activate = true,
                            OnlyIfAbsent = true,
                            PlayerActor = message.PlayerActor,
                            PlayerObject = message.PlayerObject,
                            PlayerWizard = message.Wizard,
                        }],
                        Targets = ZoneBroadcastTarget.Paths,
                    });
                }
            }
        }
        catch (Exception ex) {
            Logger.Warning("Zone {0}: the check for people an earlier step brings out failed: {1}", Logger.Args(Zone.ZonePath, ex.Message));
        }
    }

}
