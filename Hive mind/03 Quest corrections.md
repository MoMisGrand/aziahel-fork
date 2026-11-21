# Quest corrections

Every correction the bots were given for a quest, a dungeon or a puzzle. Ultimus, the seven TCO7 wizards and the Quest Team all share one brain for this, so a correction learned by one of them is known by all of them at once. A new correction is added here the same day it goes out in a bots update.

## Krokotopia, The Vault of Ice
- Breaking the Vault / Find the Fang: the Frost door (DynaTrigger_KT_Door_Frost) opens when the MoonLock braziers read Fire, Ice, Storm. Brazier3 needs two pulls (Fire to Ice, then Ice to Storm), and after the first pull it offers nothing until the wizard walks out of reach and back in. Kitharas Lamia got it at 20:48 by walking off and back; AlchemiZist stood on it at 21:34, gave up, and sat at the shut Frost door for 10 minutes. Fix (bots): after pulling a piece that still needs another pull, step about 360 away and come back. Bot fault, not quest. (2026-11-20 9:45 PM Bot Update)

## Marleybone, Big Ben (every dungeon with battle circles)
- The Last Meow: after losing a room's fight (Mythis to Filcher on room 8, Idunn to Gearhead on room 9) and coming back by Dungeon Recall, the room's creatures stand there but walking into them starts no battle; the bots marked them unfightable and sat at the shut door (Door16). Fix (bots): a battle circle whose fight was started but not won, with its door still shut and no creature that will fight, is stepped on again (up to 2 times per visit). Bot fault, not quest. (2026-11-20 9:45 PM Bot Update)

## Wizard City, Ravenwood (tutorial copy)
- WizardCity/WC_Ravenwood_Lite (the Ravenwood a new wizard sees) has no exits in its game file (only its look). A bot that follows a teammate in there finds "no way from Ravenwood" to anything. Way out: the Commons button, then on from The Commons (2026-11-21). Kitharas Sentence stood there 15 hours on Sinister Skeleton before this.

## Marleybone, Katzenstein's Lab
- If You Build It...: the gate past the first lab room opens only after the Sun, Bird, Tree and Bug plaques are clicked in that order. A plaque clicked too early is clicked back first and the order starts over.
- The game only switches on the check for the last plaque after the earlier ones are clicked, so the puzzle counts as not started until all of them are off.
- The plaques stand right beside the shut gate. A walk to a gate's own plaque never stops at that gate.
- The second gate works the same way, with the Tree, Sun, Bird and Bug plaques in that order.
- Weird Science: Firegut and Clanker are in the lab's first wing, nowhere near the plaques. A far puzzle only counts when its gate stands toward the bot's step, so these bots go straight for the fights.
- The plaques hang on the wall and are clicked from the floor below them, not walked onto (the game's own plaque data, read November 20, 2026). Earlier notes here said to walk onto them; that was wrong, and walking at them only runs into the wall.
- Correction November 20, 2026: the plaques could only be clicked while the dungeon's quest "Locked Door" asked for them. Ra and Mythis got through because they had that quest open. Bots on Weird Science or Gate is Enough without it found plaques that offered nothing. Fixed in the server along with the Counterweight East levers: the plaques can now be clicked any time, in the Sun, Bird, Tree, Bug order.

## Marleybone, The Ironworks
- Purloin the Plans: Baxter only comes out once a wizard walks past the first gate. A bot that logs in beside his spot walks over the first gate first.
- The second gate opens only after the Bird, Moon, Tree and Bug plates are stepped on in that order. The third gate works the same way.
- The factory door only opens once Brady is beaten and his password is won. Without the password the bots leave the door and the cat's peephole alone and go for Brady.
- Internal Investigation: Pops O'Leary walks a path. The bots walk to where he really is, not where his walk began.
- Pops never starts a fight when you walk into him. He only joins a fight started with the O'Leary Napper beside him, so the bots start that fight.
- Confirmed November 19, 2026: Kurrent and Mythis beat Pops, Garm and Ra got out of The Ironworks.

## Marleybone, Katzenstein's Lab
- If You Build It..., the plaque gate (Gate02): the plaques only work after Weird Science is done (beat Firegut and Clanker in the first wing). Clicked before that, all four stay set and the gate never opens (Cyndr and Kurrent, 20 November, 05:53 to past 07:00). Do Weird Science first, then click Sun, Bird, Tree, Bug. If all four are set and the gate is still shut, click Sun back to reset them and start again.
- If You Build It..., Collect Crates (0 of 3): the three crates are at (-17044, 52394) on the middle floor, (-15577, 55521) on the top floor by Grunk's lab, and (-13898, 49858). The quest is fine. Bots took the middle one on the way to the top one and then skipped the top one (20 November); fixed in the bots.

## Marleybone, Counterweight East
- Metal Madness and Stolen Away are done in one run: pull the four levers floor by floor, then go straight up to Sprockets. A bot that leaves in between finds the levers no longer work for it in a fresh copy.
- A floor creature that won't fight is never a reason to leave the dungeon. Only the step's own creature is.

## Marleybone, Gibson's Factory and every dungeon
- Creatures inside a dungeon never move apart, so a bot fighting alone goes straight into the fight instead of waiting for a lone creature.

## Every dungeon, a boss or creature that won't fight
- If the creature a step needs inside a dungeon has not been there to fight for 20 minutes, and there has been no story progress, the bot walks out the way it came in and comes back to a fresh copy, as a player would. Never mid fight, and never while grouped with a player.
- If the step's own boss stands there and starts no battle for 20 seconds, for example after a Dungeon Recall or after a teammate already beat it, the bot walks out for a fresh copy at once. At most twice an hour per dungeon, then back to the 20 minute wait.
- Confirmed November 19, 2026: Tulkar beat Jacques the Scratcher in Knight's Tower after a fresh copy.

## Every dungeon, side fights
- A bot never picks an in-between fight with a creature that stands behind a shut door.

## Krokotopia, The Oasis and the Tomb of Storms
- The three storm obelisks that open the way into the Well of Spirits stand on Storm Island in The Oasis, not in the Tomb of Storms. The bots light them in The Oasis on the way, pressing each one from its foot, and a bot already inside with obelisks left walks back out to them first.
- A bot following its group stays in the dungeon with its group mate when a shared step leads outside, and lets the mate lead.

## Krokotopia, The Temple of Storms
- Test of Strength: lighting some moons puts other moons out. The bots light first the pieces that put others out, then the rest, so every moon stays lit and the suns go dark.
- Get Smart: after the two door puzzles the step waits on a third puzzle that opens no door. The bots light the Sun, Tree, Snake and Bug pieces in that order.

## Krokotopia, Hall of Champions
- Get Some Bling: the gemstones are spread over many spots in the hall, and not every spot has one at a time. A bot that finds nothing to click at a gem spot for 20 seconds goes on to the next spot instead of waiting there.
- Get Some Bling gems: a gem picked up was deleted for the whole area and only came back when the area emptied; with 15 bots in the Hall no gem was left from 20:45 to 21:30 (2026-11-20) and every bot flagged the step broken. Fixed 2026-11-21: collected items now disappear only for the wizard who picked them up (every collect step, every world).

## Krokotopia, Emperor's Retreat
- The pad to the second tower that the area's files give sits inside a wall. The bots use the real pad, which is a different one nearby, and prefer a way out they can actually reach.

## MooShu, trips away from the story
- A trip that takes a bot into an area and then leaves it walking straight back out, twice within a quarter of an hour, is set aside for an hour so the bot gets on with the story. First seen with Ultimus going between Hametsu Village and Jade Palace.

## MooShu, Jade Palace
- An Imperial Cure, Defeat Jade Oni: the quest is fine. The throne room sits at the top of a long stair, and bots stayed on a hidden floor under the stair, so the Oni never started the fight with Ultimus (20 November, 05:08 onward; Mindy beat it normally). Fixed in the bots: they now climb stairs and ramps like a player. Walk up the long stair to the throne and the Oni comes out to fight.
- An Imperial Cure, fighting the Jade Oni: once the stairs were fixed Ultimus reached it at once but lost twice solo at level 35 (18 and 14 minutes). Each time his 20-card deck ran out around round 20 with the Oni at about 1,600 to 2,100 of 6,000 health, and it healed while he passed. Bring a bigger deck with more hits and heals; the bots now pack more cards for a long boss fight they have lost before.

## MooShu, Tree of Life
- Left Behind: the Go to Tree of Life step finishes inside the big circle round the dead tree itself, in the middle of the dungeon. The game files name that circle with a copy number, so the server did not count it and the step never finished (Ultimus stood at the tree for an hour). The small spot of the same name behind the tree is only the hidden oyster. Fixed in the server; walk up to the tree and the step finishes.
- Left Behind and Question of Faith's tower waypoint confirmed working on 20 November after the fix (Ultimus finished both alone).
- Question of Faith, Defeat Nomoonaga: Nomoonaga is not in the tree area at all. From the tree take teleporter A by the tree's trunk, walk east to the tower and walk into the spot just before the tower teleporter; it takes you to the Spirit World room (same name, Tree of Life). Nomoonaga waits there and only comes out when you step on the battle circle in the middle of that room. Bots used to walk back out of that room as if it were the wrong area (Ultimus looped for 20 minutes); now they stay and step on the circle. Leave by the spot behind where you came in afterwards to go back to the tree for Yishin Chen.
- Tree of Life (quest): after Question of Faith, talk to Yishin Chen and use the Tree of Life; that turns on the spirit portal in front of the tree. Press X at the portal to enter; it takes you to the Spirit World room, where you step on the circle in the middle to fight Kagemoosha. Before 20 November the portal offered no Enter at all (the server did not know where it went); fixed in the server.

## Marleybone, Counterweight East
- Stolen Away: the levers that open the gates up to Sprockets could only be pulled during Metal Madness, so after that quest every new copy stayed shut and the bots waited for a Sprockets that never came (Garm and Tulkar, 20 November). Fixed in the server: the levers can now be pulled any time. Pull the first lever by the entrance, beat each floor's guard to free the next lever, and climb to Sprockets at the top.

## Marleybone, Counterweight West
- Weights and Measures, Defeat Bellows: after losing to Bellows and coming back by Dungeon Recall, the Bellows in that copy can stand there and never start a battle again (Idunn, 2026-11-20, 40 minutes). Walking out and back in gives a fresh copy where Bellows fights. Bots now count a boss that won't fight as the step's own, even when they reached it as the room's fight, and walk out for a fresh copy.

## Marleybone, Katzenstein's Lab
- Katzenstein's Lab, Gate 1: the spot named "Gate 1 Open" (2498,3764) lies right at the gate, a floor below the entrance room. It is not a plate to step on from the entrance side; walk on with the room instead (Cyndr stood by it 20 minutes, twice, 20 November).
- If You Build It, Collect Crates: the three crates (by (-15577,55521), (-13898,49858) and (-17044,52394)) can be used up in a copy while the quest still wants all three (a restart starts the dungeon's quest over). Nothing is offered at the crate spots then. Walk out and back in for a fresh copy with new crates.

## Dragonspyre, The Basilica
- The arch to The Atheneum (LibraryPort1) did nothing in v0.10.936: the arch-menu fix kept the arch's own use, whose teleport names no destination, instead of its door. Fixed 2026-11-21: the arch's own use is kept only when its teleport knows where it goes. Same for the other cathedral arches (AcadPort1, NecroPort1).

## Dragonspyre, The Tower Archives
- Portal Combat, Use Blue Crystal: the crystal is on top of the Blue Tower. From the low floor, walk up the long ramp east of the Wizard Tower teleporter (it starts by (8400,-10300) and runs south past (10800,-15000)) to the Blue Tower's teleporter at (9890,-18707). It lands on the tower at (11016,-16656); walk up the tower to the crystal by (12239,-16404). Bots used to read the ramp as a wall and never found the way.
- Portal Combat, after the crystals: click the Crystal Vortex at (12767,-11249) (it turns on once the crystals are used). It lands on the high platform at (16316,-14341), where the Tower 11 sigil and the next step are. It is the only way onto that platform; before the fix of 2026-11-20 the vortex went nowhere for everyone.
- Portal Combat, the way there: the Tower Archives are very long from the door to the far teleporter. Bots keep going to the teleporter they chose and only give up on it once they have stood by it for three minutes.
- Portal Combat, walking to the ToWizT teleporter at (9177,-7685): our ground map shows its floor cut off from the entrance, so the guessed way went far north. The right way is the game's own path: down past (1680,-7431) and back up onto the pad (about 69,000 long). Bots now take it (2026-11-21).

## Marleybone, Scotland Yard
- After Weird Science, Watson offers "Fetch Bones!" right at the hand-in. That offer is the only hint; the game does not mark Fetch Bones! as story. A bot that misses it must go back to Watson and take it, never go back to Wizard City for other quests (Cyndr, 20 November).
- "Fetch Bones!" needs "It's Alive!", the second quest inside Katzenstein's Lab (after "If You Build It..."), not "Weird Science". A team that leaves the Lab with only Weird Science done has no quest offered anywhere; go back into the Lab, finish both, then see Watson (Cyndr and Kurrent, 20 November).

## Marleybone, Big Ben
- Door43 (by the fourth room's battle circle, around (-9650,21000)) opens only when a level 29 Frog is beaten after the door's trigger was switched on by an earlier floor door. If the Frog is never out in that copy, stepping on the room's circle starts nothing and the door stays shut for good. Walk out and back in for a fresh copy (Garm and Ra, 20 November).
- Server fix (next full release): leaving a dungeon copy and coming back (a lost battle, a relog, Dungeon Recall, a bots update) no longer puts counted steps back to 0. Before it, Katzenstein's Lab crates went back to 0 of 3 while the used crates stayed used, which forced a fresh copy (20 November).
