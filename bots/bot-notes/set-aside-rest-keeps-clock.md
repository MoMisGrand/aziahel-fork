Fixed: Bots resting from a stuck step never finished resting
When a bot sets a step aside and has nothing else to do, it rests for up to 5 minutes and then tries the step again. The step's two moves took turns every half second, and each turn started the rest over, so the bots wrote "sets it aside for real" 7 times a minute and never tried again. The rest now keeps its clock for the whole step.
Where: Krokotopia, Ahnic Family Tomb ("De-Cipher the Ahnics"), and any step a bot sets aside.
Test: In the bot log, a "sets aside for real" line comes once, and about 5 minutes later "I rested from" follows.
