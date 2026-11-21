Fixed: Bots could not find the way into a tower's later rooms

Some story towers in Dragonspyre are a row of rooms, each its own area, with a teleporter to the next one. The bots' way finder treated every one of those rooms as a place never to walk through, so a quest step in a later room (like "The Art of War" in room 7 of Halina Warhawk's Tower) had no way to it, and Ultimus walked back out to the Plaza of Conquests and got stuck. Now a tower's own rooms count as the way to its other rooms, while other towers and event gauntlets are still never used as a short cut.

Where: Halina Warhawk's Tower and the other Dragonspyre towers with numbered rooms.
Test: Have Ultimus take "The Art of War" from Halina Warhawk: he should go into her tower, through the door and on through the rooms to the last one, instead of walking out to the Plaza of Conquests.
