Fixed: A dungeon creature stuck standing after a lost battle

When every wizard in a dungeon battle logged out at once (for example during a bots update), the creatures won and were sent back to their spots. One of them could hit an error while leaving the battle and stay standing where it was. It never fought again and never came back properly, so any door that opens when it is beaten stayed shut. Now a creature that can't leave its battle cleanly is removed anyway, so its spawn point brings it back ready to fight.

Where: dungeons, for example Shrieker in Big Ben, whose defeat opens the door on the way to Meowiarty.
Test: Start a fight in a dungeon room and log out during it. Log back in, walk into the room's creatures again, and win. The room's door should open.
