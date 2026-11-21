Fixed: Bots stop swapping the same piece of gear back and forth
Each bot picks its gear partly by the schools its own spells hit with and the schools of the creatures it is about to fight. That information was shared by all the bots at once, so whenever another bot looked at a different creature, Ultimus changed his mind about his robe and hat and swapped them again a minute later. Now each bot keeps its own, so its gear only changes when its own fights change.
Where: Every bot's gear choices, seen most with Ultimus in the Tower Archives.
Test: Read Ultimus's thoughts for a while and the robe and hat should no longer flip between the same two pieces every few minutes.
