Fixed: Picking up a dungeon item no longer reads as everything used up
Right after a bot picked up a crate, the empty spot where it stood counted as "everything here is used up", which could send it out of the dungeon for a fresh copy. Now an item the bot took itself never counts that way, and the bot goes on to look for the next one.
Where: any dungeon with things to collect, first seen with Collect Crates in Katzenstein's Lab.
Test: in Katzenstein's Lab on Collect Crates, the bot takes a crate and goes on to the next, without leaving the dungeon.
