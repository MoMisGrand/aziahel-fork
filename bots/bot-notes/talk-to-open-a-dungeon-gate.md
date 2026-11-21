Fixed: Bots stood at Katzenstein's Lab's shut gate instead of talking to Watson

Some dungeon gates open only after a hidden talk quest that the dungeon hands out on the way in, like Watson in Katzenstein's Lab, whose talk opens the gate toward Firegut. Bots whose own quest step was past that gate never talked to Watson, so the gate stayed locked and they waited at it for ten minutes or more. Now, when a shut gate waits on such a talk, the bot goes and talks to that person first (up to three tries), then carries on. This works for every dungeon that opens a gate this way.
Where: Ultimus, the Calling of 7 and the Kitharas Family in dungeons.
Test: Send a bot into Katzenstein's Lab on "The Right Combination" and watch it talk to Watson before the gate.
