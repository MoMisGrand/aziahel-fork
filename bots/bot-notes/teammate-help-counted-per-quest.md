Fixed: A bot losing again and again now calls a teammate in time
The call for a teammate's help counted losses only at the exact same spot of a fight, and that spot changed between tries, so Idunn never reached the four losses that start the call. Losses are now counted for the whole quest over the last two hours.
Where: Any fight a TCO7 bot keeps losing, such as Bellows in Counterweight West
Test: After four lost fights on one quest, look in the log for "Can you come and help?".
