Fixed: The fresh copy for used-up dungeon things never started
Bots meant to leave a dungeon copy whose things to use were all used up, but it never happened. Getting free from a stuck spot counted as progress and kept restarting the 20-minute wait. The empty spot was also only noted after 45 seconds there, and stuck fixes moved the bot off it sooner than that. Now only real progress restarts the wait, and nothing to click at the spot is noted at once.
Where: any dungeon with things to collect or use, first seen with Collect Crates in Katzenstein's Lab.
Test: in Katzenstein's Lab with the crates used up, the bot walks out and back in for a fresh copy within about 20 minutes.
