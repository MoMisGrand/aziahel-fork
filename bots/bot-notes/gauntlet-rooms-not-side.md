Fixed: Gauntlet rooms are not a wrong dungeon
When a quest step waited in the last room of a tower gauntlet, the bots called the earlier rooms a side dungeon they should walk out of, and wrote that down as a mistake. Every room of the same gauntlet now counts as part of the step's own dungeon, so they keep going room by room without a false mistake.
Where: Dragonspyre, Halina Warhawk's Tower ("The Art of War").
Test: Watch Ultimus go through Halina Warhawk's Tower rooms 2 to 7; the log should not say "a side dungeon" in those rooms.
