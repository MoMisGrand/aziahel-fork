Fixed: A bot trapped in a dungeon copy presses the Commons button to start the copy over.

When a dungeon copy can't give a bot its step any more, the bot walks out and back in for a fresh copy. In Big Ben the way back can be shut: the floors are one-way teleporters up, and a floor's only way on is the door its creature opens. Kitharas Hexation and Sentence stood on Big Ben's 7th floor by a shut Door14 whose Golem Brass Elite never came, and "leaving Big Ben to start it over" walked them back to that door 18 times over 40 minutes. Now, if the walk out has gone on 4 minutes and the bot is still inside, it presses the Commons button once, as a player would, and walks back in for a fresh copy.

Where: Ultimus, TCO7 and Kitharas (no setting).
Test: Look for "presses the Commons button, as a player does, and walks back in for a fresh copy" in the log. The bot should then re-enter Big Ben and finish the floor.
