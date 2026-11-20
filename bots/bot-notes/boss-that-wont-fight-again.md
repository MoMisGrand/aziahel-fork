Fixed: Bots waited by a dungeon boss that would not fight
After a lost boss fight and a trip back in by Dungeon Recall, the boss in that copy could stand there and never start a battle again. A bot that reached the boss as the room's fight did not count that for its quest step, and resting beside it never counted either, so it waited instead of leaving for a fresh copy. Now both count, and the bot walks out and back in for a fresh copy within a few minutes, as a player does.
Where: any dungeon boss, first seen with Bellows in Counterweight West on Weights and Measures.
Test: in Counterweight West, lose to Bellows once and come back by Dungeon Recall; if Bellows will not fight, the bot leaves and comes back to a fresh copy.
