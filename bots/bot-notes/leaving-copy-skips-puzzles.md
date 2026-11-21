Fixed: A bot leaving a dungeon to start it over no longer turns back for a puzzle

When a bot decided to leave a dungeon and come back fresh, because the creature for its step was missing or every fix had failed, it still saw the room's unsolved puzzle as work to do first. It then turned between the puzzle and the way out every second, for as long as half an hour in Katzenstein's Lab. A bot on its way out now leaves, since the fresh run will have its own puzzles.
Where: every bot built on Ultimus, in any dungeon with floor puzzles, first seen at the Gate02 plaques in Katzenstein's Lab.
Test: watch a bot in Katzenstein's Lab whose Clanker is missing; it should walk straight out to Scotland Yard and come back in, without turning toward the plaques.
