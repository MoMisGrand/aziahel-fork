Fixed: Bots finish "collect" steps after picking something up on the way
When a bot picked up one of the things a step wants while walking to the quest arrow, it wrote the arrow's own spot off as done, even though the thing there was still waiting. In Katzenstein's Lab, Cyndr and Kurrent took one crate on the way and then never went to the other two, so "Collect Crates" stayed at 1 of 3 for half an hour. Now a bot marks the spot where it actually picked something up. If every spot looks done but the step still wants more, it checks all the spots again.
Where: Every step where you collect or use several things, first seen in Katzenstein's Lab (quest If You Build It..., Collect Crates).
Test: Send a bot to "Collect Crates" in Katzenstein's Lab and watch it pick up all three crates.
