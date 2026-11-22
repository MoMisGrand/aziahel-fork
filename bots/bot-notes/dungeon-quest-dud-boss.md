# Dungeon quest boss that starts no battle

Fixed: a bot no longer goes back to a dungeon quest's boss that it stood on with no battle starting, and it starts the dungeon over instead.

Idunn stood on Shoji in Shirataki Temple, no battle started, and she set Shoji aside for 10 minutes. The "dungeon's own quest first" and "another quest right here" choices still picked Shoji every few seconds, so she went back and forth with the way out about 700 times in ten minutes. Both choices now skip a creature that was set aside. When that creature belongs to the dungeon's own quest, she walks out and back in for a fresh copy, as she already did for her main step's creature.

Where: bots/Ultimus/UltimusDirector.Smart.cs (DungeonQuestFirst, WhileHere), bots/Ultimus/UltimusDirector.Bounds.cs (ResetCopy, QuickReset, FirstQuestDud).

Test: watch a TCO7, Kitharas or Ultimus bot in a dungeon whose boss starts no battle. It should say "stood on this ... no battle started" once, then "walks out ... for a fresh copy", with no flip-flop between the boss and the exit.

## Follow-up (2026-11-22 3:45 round): dud marked by step too

On v160, with #1649 in, Idunn still went back and forth in Shirataki Temple. At 03:34 she set Shoji aside ("I leave it alone for 10 minutes"). Two minutes later "another quest right here" picked "Defeat and Collect Shoji" again, and she then alternated with "to Shirataki Temple (Talk to Din Ho)" every 2.5 seconds, about 140 times by 03:46.

The #1649 skip only looked at the creature mark (`DudCreatures.IsDud(GlobalId)`). The step's target either had no live creature (GlobalId 0, which `Mark` ignores) or picked a different creature object than the one she stood on, so the skip never matched.

The fix adds `_dudSteps` in `UltimusDirector.Bounds.cs`, which marks the step itself (quest|goal) for the same 10 minutes. `DungeonQuestFirst` and `WhileHere` skip a step marked this way through `DudStep`.

Test: after "I leave it alone for 10 minutes", Idunn should go on to Din Ho and not come back to Shoji for at least 10 minutes.
