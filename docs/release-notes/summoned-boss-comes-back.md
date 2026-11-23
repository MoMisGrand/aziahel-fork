Fixed: A summoned boss that never came back

When a wizard summoned a quest boss on a street and the battle ended without them (for example when they were logged out in the last round), the boss could stay "taken" for good, so it never showed up again for their quest. The Earth Elemental in Plaza of Conquests stayed away for over an hour this way. The server now frees the boss's spot first when the quest asks for it again, and writes in the log what still holds the spot if it is really taken.
Where: Dragonspyre, Plaza of Conquests (No Weaklings Allowed), and every other quest boss an earlier step summons on a street.
Test: Use the Summoning Crystal for No Weaklings Allowed, log out during the fight, log back in and go back to the spot: the Earth Elemental is there again.
