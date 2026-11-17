Fixed: Bots stuck under the Altar of Kings door in the Oasis

When a bot's next stop was on a higher floor than our own floor map knew about, its walk ended right under the stop instead of going up the stairs. In Krokotopia this left Cyndr, Garm and Kurrent standing under the door to the Altar of Kings for over an hour. Now, when the walk would end on the wrong floor, the bot takes the game's own walk map, which has the stairs, from its floor up to the stop. This uses the one shared route finder, so Ultimus, the Quest Team and Bot Players get it too.
Where: Krokotopia, The Oasis, the stairs up to the Pyramid door (Altar of Kings), and any area with a raised door or ledge.
Test: Send a bot on "Digging in the Dirt" to talk to Professor Winthrop; it should walk up the pyramid stairs and go through the door.
