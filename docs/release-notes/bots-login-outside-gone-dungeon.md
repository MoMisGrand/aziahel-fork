Fixed: Ultimus logging back into the middle of a dungeon after a restart

After a server restart, Ultimus came back on the floor where he logged out, in a brand new copy of Big Ben. The quest had gone back to room 1, so he was on the wrong floor and walked out through the exit teleporters as if they were the way in. Now a bot whose dungeon copy is gone starts outside the dungeon door, just like a player does, and walks in by the real entrance. If the copy is still open, such as after a short drop, he goes straight back into it and keeps the floors already done.

Where: Ultimus, the Quest Team and the TCO7 bots, when they log in.
Test: Restart the server while Ultimus is inside Big Ben, then check that he logs in outside Big Ben and starts again from the first floor.
