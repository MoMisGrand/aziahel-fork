Fixed: Bots step out to heal before a dungeon boss when badly hurt

A questing bot that had just lost to a dungeon boss once, or had the boss next while under half health, went straight back in hurt because the boss only counted as a rival after two losses. Ultimus did this with Drusilla Morningbane in Pyromancer's Tomb at 36% health and lost again. Now the bot steps out by the Commons button, heals, and its Dungeon Recall brings it back to the same copy. A bot that is only a little under full health no longer makes that trip for a rival.
Where: any questing bot inside a story dungeon, alone, hurt before a boss.
Test: watch a bot that loses to a dungeon boss; it should step out, heal to near full, and come back by Dungeon Recall instead of walking back in hurt.
