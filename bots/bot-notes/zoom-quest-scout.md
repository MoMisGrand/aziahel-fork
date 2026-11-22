New: Zoom, the Story quest scout

Zoom is a new server-played wizard built like True Form: top level, Storm, the best top-level gear and every spell. He plays the main Story quests with Ultimus's questing brain, but he teleports straight to each quest step and one-shots whatever he fights, so he reaches broken quests before Ultimus and the teams do. A step that does not finish in 10 minutes is written in logs/zoom/broken-quests.txt (and as an issue), then finished by command so he keeps scouting. Every main quest he gets through is listed in logs/zoom/scouted.txt. None of his fights, walks or times go into what the other bots learn from.
Where: Zoom logs in on his own once this update is in. Type .zoom in game to see where he is and what quest he is on, and .zoom broken for the newest broken steps he found.
Settings (Imlight.ini, [Zoom]): Enabled (on), OneShot (on: he may use the dev one-shot spells), StepMinutes (10), PushPast (on), PartyMode (off), Port (12391).
Test: type .zoom a few minutes after the update; he should be in the game and moving through Wizard City's main quests by teleport.
