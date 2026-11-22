Fixed: Ultimus still threw away his heal to draw a treasure card when the rest of his hand was all hits.

The earlier fix kept heals out of the cards that are always safe to drop. But when every other card in his hand was a hit, the fallback still picked the heal, because it put any card that is not a hit first. Ultimus threw away Satyr this way against the Sea Lord. Now the heal goes to the back of that list, and if only heals are left to drop, he simply doesn't draw a treasure card that round.

Where: Ultimus and the TCO7 in hard fights.
Test: Watch Ultimus fight the Sea Lord. His notes "discards ... to make room for a treasure card" should never name Satyr or another heal.
