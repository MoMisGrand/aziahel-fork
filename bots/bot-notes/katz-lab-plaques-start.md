Fixed: Bots now actually start the plaque puzzle in Katzenstein's Lab.
The last bots update taught them the plaque order, but they still skipped the puzzle. The game only switches on the check for the last plaque after the earlier plaques are clicked, so the bots read the whole puzzle as switched off. Now the puzzle counts as off only when every one of its checks is off, so Tulkar, Cyndr and Garm walk to the plaques instead of waiting at the shut gate.
Where: Katzenstein's Lab in Marleybone.
Test: Watch a bot on "If You Build It..." click Sun, Bird, Tree and Bug, then walk through the opened gate to Grunk.
