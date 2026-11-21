Fixed: A bots update no longer cuts off a long boss fight near its end

A bots update waits for the bots' battles to end, but only up to 15 minutes, and with many bots one is nearly always fighting. Ultimus's 18-minute fight against Earth Elemental was cut off in round 32 with the boss almost beaten, and it counted as a loss. Now, when the 15 minutes run out, battles that Ultimus, the TCO7 or the Quest Team are already in get up to 20 more minutes to finish. Battles that start after that point don't hold the update back.

Where: Bots updates on the server (Ahziael, Bots).
Test: Send a bots update while Ultimus is in a long boss fight. The update should wait until his fight ends, then swap the bots.
