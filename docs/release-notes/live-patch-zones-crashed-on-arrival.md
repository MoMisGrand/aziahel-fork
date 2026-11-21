Fixed: Doors and teleporters stopped working after the last live update
When v0.10.941 went into the running server as a live update, the areas that were already open could not read a new list that update added, and they failed every time someone arrived in them. For about 15 minutes, doors, teleporters and dungeon entrances often did not take anyone on, mostly the bots. This update makes those areas skip the new check instead of failing, so everything works again without a restart.
Where: Every area that was open when v0.10.941 was installed with Live patches on.
Test: After this update installs, walk through a few doors and a dungeon entrance; each should take you on right away, and the server log should show no "HoldsEventPeopleSpawners" errors.
