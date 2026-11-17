Fixed: Ultimus fighting a room whose door is already open

On Big Ben's eighteenth floor, the door to the next floor opens as soon as the seventeenth floor's fight is won. A player walks straight through floor 18 without a fight, but Ultimus would still step on its battle circle and start a fight he didn't need. Now he skips a room's battle circle when the only reason for that fight is a door that is already open. Quest fights still come first as before.

Where: Big Ben floor 18, and any dungeon floor whose door is open before its fight.
Test: Let Ultimus climb Big Ben alone, and check that he walks straight across floor 18 to its teleporter without starting a fight.
