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

    private void NoteEventPeopleSpawners(IEnumerable<SpawnObject> spawners) {
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

    private bool HoldsEventPeopleSpawners() => _eventPeopleSpawners.Count > _eventPeopleWoken.Count;

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

                    var goal = template.m_goals.OfType<PersonaGoalTemplate>()
                        .FirstOrDefault(g => string.Equals(g.m_goalName, progress.GoalName, StringComparison.Ordinal));
                    if (goal is null) {
                        continue;
                    }

                    var person = QuestMadlibs.SpeakerTemplateOf(goal.m_personaName, 0);
                    if (person == 0 || _eventPeopleWoken.Contains(person) || !_eventPeopleSpawners.TryGetValue(person, out var spawner)) {
                        continue;
                    }

                    _eventPeopleWoken.Add(person);
                    Logger.Information("Zone {0}: {1} is only brought out here by an event of an earlier step, sent to another copy "
                        + "of this zone; switched on now for step '{2}' of '{3}' (spawner \"{4}\").",
                        Logger.Args(Zone.ZonePath, CreatureName(person), (string) goal.m_goalName, instance.QuestName, (string) spawner.m_name));

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
