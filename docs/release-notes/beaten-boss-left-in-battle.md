Fixed: A beaten boss stuck "in a battle" after everyone left

When a creature was beaten in the same round that every wizard left the battle (for example when they were logged out while the last hit played), the battle ended before the creature's defeat played out. The creature then stayed marked as fighting and kept its spot, so it never came back for anyone's quest. Now such a creature is taken away when the battle ends, and one left over from before is taken away the next time a quest asks for it, so it comes back to be fought.
Where: Every street creature, for example the Earth Elemental in Dragonspyre, Plaza of Conquests (No Weaklings Allowed).
Test: Summon the Earth Elemental for No Weaklings Allowed, bring it to almost no health, and log out as your last hit plays. Log back in and go back: the Earth Elemental is there again.
