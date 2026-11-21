Fixed: Bots no longer turn back and forth for wisps after a fight

After winning a fight while not at full health, a bot walks to the middle of the battle circle where the wisps fall. Once there, stepping off toward its next step made it think it had not reached the middle yet, so it turned back every second until the wisps were gone. Reaching the middle once now counts, and the bot carries straight on.
Where: every bot built on Ultimus, right after a won fight.
Test: watch a bot win a fight at under full health; it should walk through the circle's middle once and then head to its next step without turning back.
