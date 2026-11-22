Fixed: Bots finish the step in the world they arrive in on quests with a step in each world

The earlier fix for Wizard Tours did not hold. A bot remembered the step it was heading for by the Spiral Door it was walking to, and once it came through, that memory no longer matched. It went back through the Spiral Door again. Now the bot remembers the step itself, and any open step in the world it is standing in comes before steps in other worlds.
Where: questing bots on quests with open steps in more than one world, such as Wizard Tours.
Test: watch a bot on Wizard Tours; after it comes through the Spiral Door into a world, it should walk to that world's step instead of turning back.
