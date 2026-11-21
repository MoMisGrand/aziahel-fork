Fixed: Bots step on Katzenstein's Lab's plaques for a gate that cannot open yet

Each new copy of a dungeon runs its quests again, so a bot that already did "If You Build It..." on an earlier run gets it again inside a fresh Katzenstein's Lab. Until it is done in that copy, the plaques do nothing and Gate02 stays shut, but the bots still counted the quest as done from the earlier run. AlchemiZist stepped on the plaques in the right order over and over and the gate never opened. Bots now treat a quest they hold again in this copy as not done, so they do that quest first and then set the plaques.

Where: Marleybone, Katzenstein's Lab, Gate02 plaques, and every dungeon puzzle that waits on an earlier quest of the same dungeon (Ultimus, TCO7, the Kitharas)
Test: Send a bot that is past "If You Build It..." into a fresh Katzenstein's Lab: it should do the crates first, and the plaques should then open Gate02.
