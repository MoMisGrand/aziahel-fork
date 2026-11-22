Fixed: Bots stop turning back and forth between a dungeon's own quest and the way on

Two Kitharas bots in Shirataki Temple turned between the River Temple quest's Shoji and the way out to their story step twice a second for over three hours, never reaching either. Now, when a dungeon's own quest is taken up and dropped eight times in two minutes with no progress, the bot sets that quest aside for ten minutes and goes on with its story step. The quest is picked up again later.
Where: questing bots inside dungeons whose own quest keeps coming and going.
Test: watch Lumeria or Crimes of Kitharas in Shirataki Temple; within a minute or two they should say they leave River Temple alone for 10 minutes and walk on.
