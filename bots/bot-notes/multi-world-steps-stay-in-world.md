Fixed: Bots no longer bounce between worlds on a quest with a step in each world

On a quest with several "go to" steps in different worlds, such as Wizard Tours, a bot that came through the Spiral Door into one world was set down far from that world's step, so the other world's step looked nearer. It turned around through the Spiral Door again. Ultimus went back and forth between Wolfminster Abbey and The Oasis 43 times in ten minutes. Now a bot keeps the step it picked in the world it is standing in until that step is done.
Where: questing bots on quests with open steps in more than one world.
Test: watch a bot start Wizard Tours; it should finish the step in the world it arrives in before taking the Spiral Door again.
