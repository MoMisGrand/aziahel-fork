Fixed: Ultimus and the Minimap line go round corners to tomb exits
Some exits sit just past the edge of an area's floor map, like the way into the Djeserit Family Tomb in the Well of Spirits. The way finder gave up on those and drew a straight line into the wall, so Ultimus stood stuck at the corner. Now the way goes round the corners to the edge of the map by the exit, and the exit takes him on from there. The Minimap and Quest Tracker line, the Quest Team and Bot Players all use the same way.
Where: Ultimus, Quest Team, Bot Players, Minimap quest line. No setting.
Test: Send Ultimus to the Djeserit Family Tomb from the Well of Spirits. He walks round the corner and goes in without getting stuck.
