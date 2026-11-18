Fixed: Bots pull the lever between the two obelisks in the Vault of Ice

In the Vault of Ice, the bots opened the Bird and Moon locks but got stuck on the Snake lock. Its Bug obelisk only lights while the room's lever is down, and its Skull obelisk only while the lever is up. Now a bot reads what each puzzle piece waits on: it pulls the lever, lights the Bug obelisk, pulls the lever back, then lights the Skull obelisk, as a player works it out.
Where: Krokotopia, Krokosphinx, The Vault of Ice (the lever room), for Ultimus, the Quest Team and the Calling of 7.
Test: Watch Tulkar or Ra in the Vault of Ice. The log should say they pull "Lever1", click the Bug obelisk, pull "Lever1" again, then click the Skull obelisk. After that the Frost door opens to the Keeper of the Fang.
