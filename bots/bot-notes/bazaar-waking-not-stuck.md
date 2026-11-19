Fixed: Bots gave up on the Bazaar while it was still opening
When a bot walked into the Bazaar to sell or buy, the Bazaar takes about half a minute to wake up. The bot stood at the counter that whole time, was counted as stuck just before the Bazaar opened, and then skipped the Bazaar for 3 hours. Waiting at the counter while the Bazaar wakes up no longer counts as being stuck, so the sale goes through once it opens.
Where: Wizard City, Olde Town, the Bazaar.
Test: Let Ultimus or a TCO7 bot fill its backpack in Wizard City and watch the log: after "the Bazaar is open" it should say "At the Bazaar I sell", not "SIDE TRIP DROPPED".
