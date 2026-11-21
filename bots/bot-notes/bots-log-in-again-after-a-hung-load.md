Fixed: A bot whose loading screen hangs now logs in again
Sometimes the server gives up on a bot that is entering a new area, but nothing told the bot, so it waited forever. TCO7 Ra sat "waiting to be in the game" in Crimson Fields for 40 minutes after one. Now a bot that is not in after a minute and a half logs in again, the way a player restarts the game after a stuck loading screen.
Where: Ultimus, TCO7, the Kitharas and the Quest Team, whenever they change areas or log in.
Test: Watch the log for "never got into ... it logs in again": the bot should be back in the game within a minute, with no long "waiting to be in the game" stretch.
