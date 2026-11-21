Fixed: Potion trip lost after a bots update
A bot that was standing at the Wizard City potion shop when the bots updated came back and recalled straight to the spot it had marked, with no potions bought, then turned around for the shop again. The trip to the shop is now remembered over a restart, so the bot buys its potions first and recalls back after.
Where: any world without its own potion shop, on the trip to The Commons in Wizard City
Test: let a bot head to The Commons for potions, update the bots while it is there, and watch it buy before it recalls back
