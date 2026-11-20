Fixed: Bots find a boss that waits behind a dungeon door
When a fight step's boss is not in the room a bot stands in, and a door or portal of that dungeon leads to a room whose own files hold that boss, the bot now goes through that door instead of searching the wrong room. This is how the bots reach Kagemoosha through the Tree of Life's spirit portal, and it works the same in any dungeon built like this.
Where: Any dungeon whose boss waits in another room behind a door or portal, first seen in MooShu's Tree of Life (quest Tree of Life, Defeat Kagemoosha).
Test: Send a bot through the Tree of Life quest after using the tree and watch it walk to the spirit portal, enter it and fight Kagemoosha.
