Fixed: Bots went back and forth between two areas many times before a creature that had beaten them.

Before a creature that beat them twice, the bots want full health, so they left to find health wisps. In the next area the wisps were gone, and because their next move there was only a walk back, they forgot why they came and went back, only to leave again. Now the heal trip is kept until they are healed or ten minutes pass, so they try the next wisp area or a potion shop instead of bouncing.

Where: every bot, mostly in Marleybone (Knight's Court and Scotland Yard Roof) and Dragonspyre (The Atheneum and Plaza of Conquests).
Test: Let a bot lose twice to the same creature, then watch it leave to heal. It should not go in and out of the same two areas over and over.
