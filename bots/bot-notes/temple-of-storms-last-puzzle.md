Fixed: The bots stood still on "Get Smart" in the Temple of Storms
After the two door puzzles, the quest "Get Smart" waits for a third puzzle: the Sun, Tree, Snake and Bug pieces, lit in that order. It opens no door, so the bots never saw it and stood at the quest marker for about two hours. Now a bot whose quest step waits on a puzzle like this goes and lights its pieces in the right order, as a player does.
Where: The Temple of Storms in Krokotopia, on the quest "Get Smart", and any dungeon puzzle a quest step waits on.
Test: Send a bot through the Temple of Storms and check it lights the Sun, Tree, Snake and Bug pieces and then talks to the Temple Gatekeeper again.
