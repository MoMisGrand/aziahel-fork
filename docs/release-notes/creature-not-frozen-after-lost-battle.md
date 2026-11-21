Fixed: A dungeon boss could freeze in place after a lost battle

When a battle was lost in the same moment a creature fell, and the wizard was already sent out of the area, the server tripped while taking that creature out of the battle. The creature was then left standing in the room, never fighting again, so the boss of that dungeon copy could not be beaten. The server now skips the wizard who already left and finishes taking the creature out properly.
Where: Any battle, seen with Maito in Maito's Dojo (MooShu, Let me Shek) and with King Uro in the Ahnic Family Tomb.
Test: In Maito's Dojo, lose the Maito battle while landing a hit that knocks out one of the creatures, then come back by Dungeon Recall; no creature should stand frozen on the circle.
