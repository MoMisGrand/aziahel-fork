Fixed: A bot stopped by the watchdog no longer shows as paused by hand
When Ultimus, a Calling of 7 member or the Quest Team went two hours without moving their quest forward, the watchdog stopped them, but their status said "paused (Ahziael's Pause button)" as if someone had pressed Pause. Their status now says they were stopped by the watchdog and need Resume in Ahziael, so it is clear nobody paused them by hand.
Where: the status of Ultimus, each Calling of 7 member and the Quest Team in Ahziael.
Test: when a bot is stopped by the watchdog, check that its status says "stopped by the watchdog" instead of the Pause button.
