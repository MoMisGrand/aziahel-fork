Changed: The way finder goes floor by floor
The one way finder that Ultimus, the Quest Team, Bot Players and the Quest Tracker line all use now treats a deck and the hall under it as two different places. It starts on the floor the wizard is standing on, ends on the floor the goal is on, and only moves between floors by stairs and ramps it can walk. Before, it could mix up a floor above with the one below and think it had already arrived.
Where: Everywhere the bots walk and the Quest Tracker line is drawn, no setting.
Test: Watch the Quest Tracker line in a place with a deck or balcony, like Krokotopia's Entrance Hall. The line should follow the ramp between the floors instead of stopping right above the goal.
