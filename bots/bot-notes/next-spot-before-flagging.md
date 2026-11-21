Fixed: Bots called a step broken before trying its other spots
When a step has several places to use something, like the three Scout Towers in "You're Fired!", a bot that found one already used flagged the whole step as broken first and only then went on to the next place. Mythis did this at 3:38 AM and finished the quest two minutes later. Now the bot goes on to the other places first, and only a place with nowhere else to try is written up.
Where: MooShu, Tatakai Outpost ("You're Fired!"), and every step with several places to use something.
Test: In the bot log, a step with several spots shows "on to the next spot" without a "BROKEN STEP" line before it.
