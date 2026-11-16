Fixed: Bots never pick broken cards
Ultimus, the Quest Team and the Bot Players now leave out every card that Ahziael's Spells tab marks BROKEN, using the same check. This applies to their decks and to the treasure cards Ultimus buys. The first time a bot would have used a broken card, the server log reports it once as a CARD PROBLEM with the reason.
Where: the bots' decks, and the server log.
Test: Let Ultimus build his deck and fight, then search the server log for "CARD PROBLEM" to see any broken card he skipped.
