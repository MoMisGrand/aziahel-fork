New: Ignore bots for restarts
Update restarts no longer wait for the server's own bots. Before, a restart waited up to 2 hours for Ultimus or a TCO7 member to leave a dungeon, and a countdown restart waited for bots' battles to end. Now only people playing and their battles hold a restart; the bots log back in by themselves afterwards. The switch is on by default and can be turned off to wait for the bots again.
Where: Ahziael tab, Updates card, "Ignore bots for restarts".
Test: With only bots online (one in a dungeon), let an update install; the server should restart right away instead of showing "Waiting for Ultimus to leave a dungeon".
