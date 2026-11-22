Fixed: Turning back and forth when a dungeon quest's creature is not there
In a dungeon, a bot walked to the spot of its dungeon quest's fight, found nothing there, turned to the way out, and one step later turned back to the fight, every half second. Lumeria and Crimes did this with Shoji in Shirataki Temple for over an hour. Now, reaching an empty fight spot leaves that step alone for 10 minutes, so the bot goes on, and the copy can be started over fresh.
Where: MooShu, Shirataki Temple (River Temple, Defeat and Collect Shoji), and any dungeon quest whose creature is missing.
Test: Watch a bot reach a dungeon quest's fight spot with no creature: it should say it leaves the step alone and go on instead of turning back.
