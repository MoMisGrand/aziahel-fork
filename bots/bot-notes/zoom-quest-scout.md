New: Zoom, the Story quest scout

Zoom is a new server-played wizard built like True Form: top level, Storm, the best top-level gear and every spell. He plays the main Story quests with Ultimus's questing brain and everything the bots and the Hive mind know, but he teleports straight to each quest step and one-shots whatever he fights, so he reaches broken quests before Ultimus and the teams do. He shows in Friendly Players like every bot. A step that does not finish in 10 minutes is marked down as a broken step, and no next main quest 10 minutes after one he finished is marked down as a broken quest, both in his own mind file (logs/zoom/zoom-mind.json, readable copy zoom-mind.txt) and as an issue. He never finishes anything by command: he keeps trying until the quest is fixed. None of his fights, walks or times go into what the other bots learn from.
Where: Zoom logs in on his own once this update is in.
Settings (Imlight.ini, [Zoom]): Enabled (on), StepMinutes (10), PartyMode (off), Port (12391).
Test: open Friendly Players a few minutes after the update; Zoom should be listed and moving through Wizard City's main quests.

## How he fights (Niduus, 2026-11-22)
"give Zoom all vanilla spells for storm and enchants. His main tactic should either be temp + Epic to clear or Glowbugs + Epic to clear."
No dev spells. Each round his hand is dealt: Epic x2, Tempest, Glowbug Squall, then his biggest real hits (ZoomBattle). He casts Tempest when he can pay for it, else Glowbug Squall, with Epic put on first; only when neither can be cast, the biggest hit he can pay for. The old [Zoom] OneShot setting does nothing now.
