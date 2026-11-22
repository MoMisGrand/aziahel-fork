# Side dungeon: Commons button after 10 minutes of walking out

**What went wrong (2026-11-22):** Kitharas Rock finished Marleybone's story inside Big Ben at about midnight. His next step is in Wizard City, so Big Ben became a side dungeon and he set off "to Regent's Square (leaving Big Ben)". The map showed no way out on foot. The run Walter Blanco once guided him through took the floor teleporter at (-32064, 12003) back up to the top floor, and the server moved him back down again. He kept doing this for three and a half hours (00:00 to 03:31). Moon (from 02:15) and Hart (from 03:19) got caught in the same loop.

**Why:** `LeaveSideDungeon` only pressed the Commons button when `Travel` found no walk at all. A walk that never gets him out still counts as a walk, so the button was never pressed.

**Fix:** `UltimusDirector.Bounds.cs` now keeps `_sideSince`, which records when he started leaving this side dungeon. If he is still inside after 10 minutes with no real story progress, `SideByButton` presses the Commons button. That takes him to this world's hub, the same way a player gets out. He presses it at most once every 5 minutes.

Unlike the potion trip, his spot is not marked first, because Recall would only take him back into the dungeon he is leaving. The server's Commons button (`MSG_GOHOME`, ZoneService) works inside dungeons.

**Watch:** look for "has been walking out of Big Ben (a side dungeon) for N minutes and is still in it: presses the Commons button". Rock should then show up in Regent's Square and head for Wizard City.
