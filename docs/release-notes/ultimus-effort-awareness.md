Fixed: Ultimus no longer flees dungeon battles and keeps track of how his efforts went
When Ultimus goes down in a dungeon or instance, he no longer flees after four rounds, because fleeing would lose the whole run. He stays down and waits for a revive or the end of the fight. He now keeps a daily record of the good and the bad: fights won and how fast, fights lost, flees, dungeons lost and steps he could not get past. That record goes into his "today I learned" review and his stuck thinking, so he weighs the cost of each choice.
Where: Ultimus in battle, logs/ultimus/effort.txt, and learned-today.txt.
Test: Let Ultimus fight for a while, then open logs/ultimus/effort.txt and check that it lists both GOOD and BAD lines.
