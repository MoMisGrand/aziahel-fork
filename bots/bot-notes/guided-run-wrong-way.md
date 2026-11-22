Fixed: Bots no longer follow a player's guided run the wrong way out of a tower
When a player guides a bot through a dungeon, the bot remembers the route and reuses it. In Big Ben, Rock remembered the route Walter Blanco took up the tower. When Rock later tried to leave from floor 11, that route kept sending him back up from floor 10 to floor 11, 20 times in 35 minutes. Now a bot only follows a guided route in the direction it was walked. If its step lies behind it on that route, the bot finds its own way instead.
Where: Any dungeon a player has guided a bot through. No setting.
Test: Guide a bot up a few floors of Big Ben, then let it leave the tower on its own. It should go down floor by floor to the exit without being sent back up.
