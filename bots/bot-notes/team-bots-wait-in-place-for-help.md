Fixed: A team bot that calls for help now waits where it is for the helper
When a TCO7 bot keeps losing a fight, it asks a free teammate for help and waits for them before going back in. Its stuck check took that wait for being stuck, so it stepped aside and tried other ways out, and Kurrent wandered through Ravenwood and The Commons while Mythis chased after him. Now waiting for a called helper counts like the other group waits, so the caller stands still until the helper arrives.
Where: TCO7 and every bot built on Ultimus, after a help call on a fight they keep losing.
Test: Watch for "I keep losing a fight on" in the log, then "waiting for ... to reach me": the caller should stay put, with no "has stood still" lines, until the helper joins.
