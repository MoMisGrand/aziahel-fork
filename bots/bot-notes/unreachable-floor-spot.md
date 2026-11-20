Fixed: Bots stop chasing a dungeon floor spot they cannot reach
In some dungeons a spot that opens a gate lies past that gate or on a lower floor, with no way to it from where the bot stands. The bot kept trying to get onto it for 20 minutes at a time. Now a floor spot the bot has not reached in 5 minutes is dropped, and the bot goes on with the rest of the floor and its story step.
Where: any dungeon, first seen at the first gate of Katzenstein's Lab.
Test: in Katzenstein's Lab, the bot no longer stands at the first room trying to reach "Gate 1 Open" for long.
