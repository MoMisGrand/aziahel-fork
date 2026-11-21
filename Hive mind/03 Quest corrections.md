# Quest corrections

Every correction the bots were given for a quest, a dungeon or a puzzle. Ultimus, the seven TCO7 wizards and the Quest Team all share one brain for this, so a correction learned by one of them is known by all of them at once. A new correction is added here the same day it goes out in a bots update.

## Krokotopia, Grand Arena
- Stonechin: Kitharas Umbella found no Krag Stonechin in his Grand Arena copy at 23:40 (2026-11-20), most likely a copy where a family member had already beaten him; after trying every move he leaves and comes back in for a fresh copy (23:45), as a player would. Watching that it clears. (2026-11-20 11:45 PM Bot Update)
- Stonechin: Kitharas Wraith walked to Kitharas Rock's fight with Krag Stonechin (494, -328) and was not let in; he stood there 4 minutes, flagged stuck 6 times, then lost to Krag Stonechin himself. Fix (bots): after 30 seconds at a group mate's battle without being let in, wait beside it until it ends. Bot fault, not quest. (2026-11-20 11:00 PM Bot Update)
- Gladiator Games (Talk to Arena Master): the way is the teleporter at (-1715, 3325), which lands at (-5209, 22207) beside the stairs up to the Arena Master. Solo Kitharas (Hart, Wraith, Moon) did it in 40 seconds; the ones busy forming or leaving a group (Jackle, Shade, Consumer, Lumeria) took 3 to 4 minutes on the same walk. Quest is fine; group shuffling is the slow part. (2026-11-20 10:45 PM Bot Update)
- Taking it to the Tuts: 9 Kitharas finished in 50 seconds to 8 minutes; the slow ones were in fights with the tomb's creatures, not stuck. (2026-11-20 10:45 PM Bot Update)
- Stonechin copy with no Krag Stonechin (Kitharas Umbella, 11:45 PM on 20 November): walking out and back in for a fresh copy worked, and he finished Stonechin at 11:58 PM (16 minutes in all).

## Dragonspyre, The Tower Archives
- At 23:42, right after the bots update logged him back in at (7753, 13565), he marked his spot and left for treasure cards at the Bazaar; this trip marks and Recalls back to the spot, so the climb is kept (unlike the 23:14 selling trip, now fixed). (2026-11-20 11:45 PM Bot Update)
- At 23:14 his backpack filled and he left the Tower Archives for the Wizard City Bazaar to sell one piece (about 1,700 gold over a quick sell, with 59,160 gold held), then came back in at the bottom (-31770, -14011) at 23:23, losing 70 minutes of climbing. Fix (bots): from another world, a Bazaar sale must bring at least a tenth of the gold he holds over a quick sell. Bot fault, not quest. (2026-11-20 11:30 PM Bot Update)
- Portal Combat: Ultimus has been in the Tower Archives since 22:11 (2026-11-20) following the game's walk map toward the raised teleporter at (9177, -7685); the way winds far round (he passed (12650, 11220) and (25407, 27105)) and creatures catch him about every 5 minutes. He wins them, but each two-enemy fight ran up to 13 rounds because he cast 1-pip hits (about 121) every round while Locust Swarm and Gearhead Destroyer waited for pips. Fix (bots): with only a small hit castable and a hit 2.5 times bigger waiting, he builds pips first. Bot fault, not quest. (2026-11-20 11:15 PM Bot Update)
- Treasure card trip with Mark worked (21 November): Ultimus marked his spot at 11:42 PM, bought Ice Hound x2 and Elemental Shield at the Wizard City Bazaar at 11:49 PM, and Recall put him back on his marked spot in the Tower Archives at 11:49 PM. He then won his Portal Combat fights, saving pips for the bigger hit when his small hit was not worth it.
- Ultimus (21 November, 11:49 PM to 12:31 AM): after recalling back to (7753,13565) he walked north-east to (20000-26600, 28000) instead of south-west toward the ToWizT pad (9177,-7685); the game's walk map goes south-west from there (about 48,000 long). The "game's own paths" walk never took over for him. Being looked into.
- Ultimus reached the ToWizT pad at (9177,-7685) at 12:48 AM (21 November), seven minutes after the bots update logged him back in, after hours of wrong-way walking. The pad did not fire because of the server outage that started at 12:31 AM, not because of the quest; once the server works again it should take him up. (2026-11-21 1:15 AM Bot Update)

## Dragonspyre, The Basilica
- Portal Combat: Ultimus sat resting at the Basilica's arch to The Atheneum for hours (the arch offered no way through). After the arch fix reached the server at 22:08 (2026-11-20) he walked straight through at 22:09 and took the teleporter at (9177, -7685) toward the Tower Archives. Success. (2026-11-20 10:15 PM Bot Update)
- After the arch, Ultimus is crossing The Atheneum toward the Tower Archives but is caught by side fights he tries to walk round (22:12, 22:18, 22:24, 22:29), each a long one of 5 minutes or more against Loathsome Creepers. He wins them; slow, not stuck. Watching. (2026-11-20 10:30 PM Bot Update)

## Wizard City, The Commons potion shop (every bot's potion trip)
- The shop's "Fill All" is refused unless the gold covers every missing potion. Kurrent (4 missing, 390 each, 1115 gold) asked for Fill All every 12 seconds at the counter from 21:53 to 21:58 and was flagged stuck 37 times; Tulkar the same at 02:15. Fix (bots): with too little gold for all, buy single potions, as many as the gold pays for. Bot fault, not quest. (2026-11-20 10:00 PM Bot Update)
- Still stuck at 22:16 (flagged 41 times) because the fix only reaches the server with the 10:15 PM bots update; watch that it clears. (2026-11-20 10:15 PM Bot Update)
- The bots update at 22:28 logged Kurrent back in at the shop counter, and he recalled straight to Regent's Square with 0 of 4 potions (the trip was not remembered over the restart, only the mark), then set off for the shop again. Fix (bots): the potion trip is kept over a restart, so he buys first and recalls after. Bot fault, not quest. (2026-11-20 10:30 PM Bot Update)
- Worked: Kurrent bought single potions at 22:37 (two, 0 to 2 of 4) and one more at 22:44 when he had the gold. The Fill One fix is confirmed on the live server. (2026-11-20 10:45 PM Bot Update)

## Krokotopia, The Vault of Ice
- Worked: AlchemiZist pulled Brazier3 from Ice to Storm at 22:50 and finished Breaking the Vault at 22:57 (11 minutes), after sitting at the shut Frost door for over an hour earlier. (2026-11-20 11:00 PM Bot Update)
- Breaking the Vault / Find the Fang: the Frost door (DynaTrigger_KT_Door_Frost) opens when the MoonLock braziers read Fire, Ice, Storm. Brazier3 needs two pulls (Fire to Ice, then Ice to Storm), and after the first pull it offers nothing until the wizard walks out of reach and back in. Kitharas Lamia got it at 20:48 by walking off and back; AlchemiZist stood on it at 21:34, gave up, and sat at the shut Frost door for 10 minutes. Fix (bots): after pulling a piece that still needs another pull, step about 360 away and come back. Bot fault, not quest. (2026-11-20 9:45 PM Bot Update)
- Brazier3 second pull still failed for Kitharas Moon (00:00) and Libra (00:16) on 21 November: the bot tracked the piece by the setting it showed (Fire), which changes to Ice after the first pull, so it forgot it had pulled it, stood on it with nothing offered, gave up and waited at the shut Frost door (Moon, Was Silenced and Rock stuck at (2899,3260) for "Defeat and Collect Keeper of the Fang"). Fixed 21 November, 12:15 AM round: the piece is known by name whatever it shows. Kitharas Jackle and Consumer still finished Breaking the Vault (00:02, 23:54).
- Brazier3 by name worked (bots-v93, built 12:24 AM on 21 November): Libra, Jackle, Andromeda, Hart, Crimes Against Kitharas and Wraith all finished Breaking the Vault between 12:23 and 12:28 AM, after Moon, Was Silenced and Rock had been stuck at the shut Frost door.

## Marleybone, Counterweight West (and every dungeon fight)
- More Metal Madness: Kurrent (level 26) lost to Clockwork Warrior and Clockwork Wizard over and over, going in at 28% to 64% health; each loss puts him out of the dungeon and the dungeon starts over. Across all bots on 2026-11-20, 97 of 291 losses were put down to going in hurt (38 under half health), and 118 times a bot in a dungeon went on at low health rather than walk out. Bots keep their potions for the boss (Niduus's rule), so this is his call: asked whether a potion may be used before any dungeon fight when under half health. (2026-11-20 11:00 PM Bot Update)
- Kurrent, Weights and Measures (21 November 00:05): stood on a Clockwork Warrior of the room circle "Activator Volume (2)" for 20 seconds with no battle; lever MB_CW_Lever 03 offered nothing; then stuck at (967,3144) on "Defeat 1 creatures (where they spawn)". Clockwork Warrior has beaten him 45 times of 127.

## Marleybone, Big Ben (every dungeon with battle circles)
- The Last Meow: after losing a room's fight (Mythis to Filcher on room 8, Idunn to Gearhead on room 9) and coming back by Dungeon Recall, the room's creatures stand there but walking into them starts no battle; the bots marked them unfightable and sat at the shut door (Door16). Fix (bots): a battle circle whose fight was started but not won, with its door still shut and no creature that will fight, is stepped on again (up to 2 times per visit). Bot fault, not quest. (2026-11-20 9:45 PM Bot Update)
- Stealthy Stuff shows as FINISHED when a bot leaves Big Ben after a lost fight, then starts again on Dungeon Recall (Idunn 00:06, 21 November): not a real finish.
- The Last Meow finished by TCO7 Tulkar (12:22 AM, 40 minutes) and Mythis (12:28 AM, 46 minutes) as a group on 21 November; Tulkar went straight on into MooShu (Bad News..., Welcome to MooShu?, Mind Games, Do You Mind?).

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
- Get Some Bling worked after that fix: from 22:10 to 22:15 (2026-11-20) 16 of the Kitharas finished it in 86 seconds to 6 minutes each, after 17 of them had sat there for up to 7 hours. Some still flag "nothing at the quest arrow's spot" for a moment, then find a gem at the next spot. Success, nothing more to change. (2026-11-20 10:15 PM Bot Update)

## Krokotopia, Emperor's Retreat
- The pad to the second tower that the area's files give sits inside a wall. The bots use the real pad, which is a different one nearby, and prefer a way out they can actually reach.

## MooShu, trips away from the story
- A trip that takes a bot into an area and then leaves it walking straight back out, twice within a quarter of an hour, is set aside for an hour so the bot gets on with the story. First seen with Ultimus going between Hametsu Village and Jade Palace.

## MooShu, Crimson Fields and Emperor's Retreat
- Garm at Rikugun Mikawa (Warlord Katsumori, A Soldier's Duty) and Kitharas Hydra at Shalek the Wise (Overpower the Overseer) were told "no options" for a few minutes, then the talk went through each time (23:04, 23:30, 23:39). Nothing to fix yet; flagged if it lasts. (2026-11-20 11:45 PM Bot Update)

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
- Confirmed 21 November 1:30 AM: the server brought Travis Pawman out on floor 5 for a bot whose earlier step happened in another copy (v0.10.944).
- Stealthy Stuff, second "Talk to Travis Pawman" (floor 5, near (-4294,22361)): he is brought there only by the event "MovePawman" that talking to him in the first room sends, in that copy only. In a new copy (relog, restart, fresh copy) floor 5 stays empty for good. Garm was stuck there 92 times on 20 November and Idunn again on 21 November at 11:53 PM. Server fix (21 November, 12:00 AM round): a person whose spawner starts off, held by one spawner only, comes out for a wizard whose open talk step needs them, in every dungeon.

## Not quest faults (server outage)

- From 12:31 AM on 21 November every door, teleporter and dungeon sigil failed for everyone ("the exit never takes" lines, sigil waits timing out), until Niduus stopped the server at 1:12 AM; it came back at 1:24 AM on v0.10.944 with no more door or teleporter failures. It was a server fault from the v0.10.941 live install (v0.10.942 could not install while it ran). Bots should not learn "this way goes nowhere" from any exit that failed in that window, and quest blames from it (Tomb of Storms in The Oasis, The Tower Archives from The Atheneum, and others) are not quest faults.

## Not quest faults (trainer hall door)
- The Key to the Temple (Krokotopia) was flagged by Kitharas Hydra, but the quest was fine. The trainer hall door spot in the Commons sits on the bots' path to the Shopping District, and each bot that walked over it was sent toward a house its game client cannot load, then stood still for 90 seconds ("The house could not be prepared"). This happened 43 times on 21 November up to 2 AM. Fixed 21 November 2 AM: the door lets bots walk past. (2026-11-21 2:00 AM Bot Update)
- Stealthy Stuff (Big Ben floor 5): Idunn waited at the shut Door43 in an old copy until 2:04 AM, then left and started a fresh copy, and at 2:12 AM the server brought Travis Pawman out in it, as the 1:30 fix meant to. She is climbing back up. (2026-11-21 2:15 AM Bot Update)

## Not quest faults (gear)
- Ultimus kept swapping Adept Sorcerer's Robe and Clanker's Noteworthy Robe (and his hat) back and forth every few minutes. Every bot shared one record of the schools it hits with and is hit by, so each other bot's fights moved his choice. Fixed 21 November 2:15 AM: each bot keeps its own. (2026-11-21 2:15 AM Bot Update)
