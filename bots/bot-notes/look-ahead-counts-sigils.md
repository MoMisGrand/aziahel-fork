Changed: The bots' look-ahead list is honest now
The list the bots keep of problems in quests they have not reached yet flagged 649 of 1628 quests as likely stuck. Most of those were false: every dungeon entered by a sigil read as having no way in, and many areas read as missing though their game files are there. Sigils now count as a way in, and an area counts as read when its game file is there, so the list shows only real problems ahead.
Where: Ahziael, the bots' readiness list (readiness.txt in the Ultimus logs)
Test: after the bots update, open readiness.txt and see far fewer "likely stuck" quests, with Zarathax's Tower and Castle Strigoi no longer flagged
