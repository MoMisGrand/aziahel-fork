Fixed: Bots leave an empty pick-up spot even when creatures walk by it
The earlier fix let bots move on from an empty pick-up spot, but in the Hall of Champions they first went after the Hall Servants walking near the empty gem spot, then came back to the same spot, over and over. Now a spot that has had nothing to click for 20 seconds is left for the step's next spot before the bot thinks about the creatures near it. A step with only one thing to use, like the Frozen Forge, still has its guards fought first.
Where: any step where you collect or click several things, first seen in Krokotopia's Hall of Champions (quest Get Some Bling)
Test: watch a Kitharas bot on Get Some Bling move on from an empty gem spot instead of fighting the Hall Servants beside it.
