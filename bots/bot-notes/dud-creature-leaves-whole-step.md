Fixed: Still turning back and forth at a dungeon quest creature that will not fight
In a dungeon, when a bot had set aside a creature that would not start a fight, its quest step pointed at the creature's spawn spot or another copy of it on the next look, so the bot turned back to it every half second. Lumeria and Crimes still did this with Shoji in Shirataki Temple after the last fix. Now setting aside the creature sets aside its whole quest step for 10 minutes too, and the copy can be started over fresh.
Where: MooShu, Shirataki Temple (River Temple, Defeat and Collect Shoji), and any dungeon quest creature that will not fight.
Test: Watch Lumeria or Crimes in Shirataki Temple: they should stop turning between Shoji and the way out.
