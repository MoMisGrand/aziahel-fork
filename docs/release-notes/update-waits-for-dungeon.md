Fixed: Updates wait for Ultimus to finish a dungeon before restarting the server
An update set to restart the server when nobody is playing did not count Ultimus, so it restarted while he was in Big Ben, and every restart closed the dungeon and made him do the floors over. It happened six times in one night. Now that restart also waits while Ultimus or a TCO7 member is inside a dungeon, for up to two hours, and the update button says it is waiting for Ultimus to leave a dungeon.
Where: Ahziael, when an update installs by itself while Ultimus is in a dungeon
Test: Let an update come in while Ultimus is in Big Ben and nobody is playing, and check that the update button says it is waiting for Ultimus to leave a dungeon and the server does not restart until he is out.
