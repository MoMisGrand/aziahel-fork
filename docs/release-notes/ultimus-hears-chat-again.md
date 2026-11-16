Fixed: Ultimus hears what players say to him again
Every line a player said to Ultimus crashed before he read it, so he answered nothing, including "teleport to me", "dismissed" and "leave group". It came from a quick check of which quest the speaker is on, which could not read one part of a player's quest list. That check now reads it safely, and if it ever fails again he still hears and answers the player.
Where: Chat with Ultimus, in a group or nearby
Test: Group with Ultimus, say "teleport to me" and then "leave group", and check that he answers both and leaves the group.
