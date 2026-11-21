Fixed: Bot updates no longer wait for a server restart because of the Pet Hatching Kiosk
The last bot update could not be used until the server restarted, because the kiosk bot's pet listing needed a piece of the server that only a full restart brings. The kiosk bot now does that part itself, the same way, so the waiting bot fixes (training inside the school houses, waiting in place for a called teammate, and logging in again after a stuck load) can come in at once.
Where: All bots, Ahziael > Bots page, no setting
Test: Look at the Bots page after the next bot update and check it says the new bots are in, without a restart.
