Fixed: A bot kept walking to creatures behind a shut gate while it waited
When a bot puts a stuck step aside, it fights a nearby creature in the meantime. Garm kept picking the Street Sweepers behind The Ironworks' second gate, which was still shut, and stopped at the gate about 60 times in an hour. Creatures the bot can only reach through a shut door are now skipped for these in-between fights.
Where: Marleybone, The Ironworks (and any dungeon with shut gates).
Test: Watch Garm in The Ironworks: while a step waits, he should fight a creature on his side of the gate or rest, not walk to Gate2 over and over.
