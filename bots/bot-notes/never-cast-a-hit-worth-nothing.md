Fixed: Bots cast a hit that could do no damage instead of guarding or waiting

When a bot planned its next two rounds, it could pick a hit worth nothing against the enemy, such as Ice Hound against the Sea Lord, and spend its turn and pips on it while the enemy kept hitting. A plan worth nothing is now dropped, so the bot heals, shields or saves its pips instead. This applies to Ultimus, the Calling of 7, the Kitharas Family and the Quest Team.
Where: Every bot battle.
Test: Watch Ultimus fight the Sea Lord and check he never casts a hit his round notes value at about 0.
