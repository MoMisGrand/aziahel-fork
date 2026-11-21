Fixed: TCO7 bots call a teammate for help even when a bots update comes between their losses

A TCO7 bot that loses the same quest's fight four times in two hours asks a free teammate to come and help. Every bots update logged the bots out and their loss count started over, so with updates every half hour the fourth loss never came: Kurrent lost to Maito five times without calling anyone. The count is now kept in each bot's own folder and survives a bots update.

Where: TCO7 and Kitharas Family help calls.
Test: Watch a TCO7 bot that keeps losing one boss across a bots update: after the fourth loss in two hours it should ask a teammate for help.
