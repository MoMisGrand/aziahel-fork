Fixed: Bots could not find the teleporter up a ramp the map missed
Our ground map sometimes reads a ramp as a wall. Then the bots saw no way to a teleporter at the top of it, and so no way to a step that only that teleporter reaches. Now the bots also use the game's own walk map to work out which teleporters lead where, so they walk up the ramp and take it. This works the same for Ultimus, the Calling of 7, the Quest Team and the Bot Players.
Where: any area, first seen in The Tower Archives in Dragonspyre, where the Blue Crystal for Portal Combat is only reached by the Blue Tower's teleporter.
Test: on Portal Combat, a bot in The Tower Archives walks up the ramp to the Blue Tower's teleporter, rides it up, and uses the Blue Crystal.
