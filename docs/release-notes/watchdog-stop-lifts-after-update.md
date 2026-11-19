Fixed: An update lifts the watchdog stop again
When the watchdog stops a bot, the next update is meant to start it again, because the update may hold the fix. Since the bots began loading their own updates, every build marked its stops the same way, so no update ever counted as new and Ultimus and three of the Calling of 7 stayed stopped after v0.10.922. Each build now has its own mark, and stops written with the old mark are lifted by the next update.
Where: Ultimus, each Calling of 7 member and the Quest Team after the watchdog stops them
Test: install this update without pressing Resume and check that Ultimus, Tulkar, Kurrent and Mythis start playing again by themselves.
