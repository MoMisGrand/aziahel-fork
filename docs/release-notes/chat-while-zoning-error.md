Fixed: A server error when someone chats while changing areas

When a player or bot sent a chat line in the moment they were changing areas, the server logged an error because their wizard wasn't in the new area yet. This happened twice tonight while the TCO7 bots walked into The Oasis. Now that line is quietly dropped instead.

Where: Chat, while moving between areas.
Test: Check the server log after the TCO7 change areas a few times; there should be no ChatService errors.
