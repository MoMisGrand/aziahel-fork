Fixed: Dungeon step counts start over after leaving and coming back
Steps that count things in a dungeon, like collecting 3 crates, went back to 0 when you left the dungeon and came back into the same copy. That happened after a lost battle, a relog or a Dungeon Recall, while the crates you already used stayed used, so the step could not be finished in that copy. Now the copy remembers how far you got and gives your count back when you return. A fresh copy still starts at 0.
Where: Every dungeon with a counted step, such as Katzenstein's Lab in Marleybone (If You Build It...)
Test: In Katzenstein's Lab, collect 1 or 2 crates, log out and back in (or leave and use Dungeon Recall), and check the quest still shows the same count.
