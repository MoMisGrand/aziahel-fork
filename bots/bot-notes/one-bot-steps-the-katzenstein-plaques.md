Fixed: Bots sharing Katzenstein's Lab no longer undo each other's plaques
Gate02 in Katzenstein's Lab opens only when the Sun, Bird, Tree and Bug plaques are stepped on in that order, and several bots in the same copy were each stepping on them at once, so a plaque turned twice and the order broke again and again. Now the first bot to work the plaques does them, and the others stand clear of the plaques and wait until the gate opens. The same goes for every puzzle that must be done in order.
Where: Katzenstein's Lab (Marleybone), and any dungeon puzzle done in order, no setting
Test: Send two or more bots into the same Katzenstein's Lab copy and check that one steps on the plaques while the others wait, and that Gate02 opens.
