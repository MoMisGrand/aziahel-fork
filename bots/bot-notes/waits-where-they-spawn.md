Fixed: Bots turning back and forth by an empty spawn spot

When none of the creatures a quest step needs were up, a bot walked to where they spawn, and once it got there it walked off toward the quest arrow, then back again, every half second. Now it waits at the spawn spot for the next one. Inside a dungeon, where a beaten creature never comes back in the same copy, a bot that has waited there 5 minutes with none of them anywhere in the copy walks out and back in for a fresh copy.

Where: any quest step that asks to defeat creatures, for example Battle of Evermore in Crimson Fields (MooShu).
Test: Watch Mythis in Crimson Fields on "Clear 5 Otomo Spotter or Otomo Soldier": he should stand at the spawn spot, and within about 5 minutes walk out and back in if no Otomo is left in his copy.
