Fixed: A dungeon creature a door needs never came back

In some dungeons a door opens only when you beat one creature after a lever or another fight gets things ready, and the game keeps bringing that creature back on its timer until then. Our server never brought a beaten creature back inside a dungeon, so if you beat it too early the door could never open. Now those creatures come back on their normal timer until the dungeon switches their spawn off, just like in the real game.
Where: Big Ben, the door past the Frog after the Clockwork Golem or the Cat Hobo (Stealthy Stuff), and any dungeon built the same way.
Test: In Big Ben beat the Frog before the Golem or Cat Hobo, then beat one of them; the Frog comes back within 5 minutes and beating it opens the door.
