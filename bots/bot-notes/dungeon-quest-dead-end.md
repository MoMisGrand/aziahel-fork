Fixed: Bots no longer walk back and forth between two dungeon rooms when the dungeon's own quest leads nowhere
In Shirataki Temple, the side quest "Summoning the Spirit" sent Tulkar out of the Plague Oni's palace to the cliffside, but there he could not find what that quest needed, so his story step took him back into the palace. He went in and out 43 times in 20 minutes. Now, when the dungeon's own quest leads to a room where its step has nothing to do, the bot remembers that way as a dead end for 20 minutes and goes on with its story step instead.
Where: Any dungeon with its own quest. No setting.
Test: Take "Release" in MooShu and enter Shirataki Temple. After talking to Din Ho, the bot goes into the palace and fights the Plague Oni instead of going back out to the cliffside.
