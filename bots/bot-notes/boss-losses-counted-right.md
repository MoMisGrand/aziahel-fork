Fixed: Lost boss fights are counted, and a long boss fight is not an off-track alarm

When Ultimus lost a long boss fight, the bot could lose track of which step the fight was for, so every loss was counted as the first one and his plan for "I lost this fight twice, so I change something" never started. Losses are now counted against the fight he went in for, by quest and step. A fight for his story step also no longer counts as time without story progress, so a fifteen-minute boss fight no longer sets off the off-track alarm or the step's time limit the moment it ends.

Where: every bot built on Ultimus (Ultimus, TCO7, the Kitharas family), any boss they lose to more than once.
Test: let Ultimus lose to Zarathax twice and check the log says "lost the battle (2 time(s) at this step)" followed by "after losing 2 times he tries".
