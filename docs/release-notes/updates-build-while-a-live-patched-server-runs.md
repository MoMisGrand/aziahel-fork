Fixed: Updates stopped with "access denied" after a live install
After an update went into the running server live, the next update could not be built: it stopped with "Access to the path 'Imlight.CoreLib.pdb' is denied", because the running server kept a file of the app's build folder open. The server now reads that file from its own folder instead, and while an older server still holds the build folder, the update is built in a second folder so it goes ahead anyway.
Where: Ahziael, updates while the server runs with Live patches on.
Test: With the server running and Live patches on, let two updates come in one after the other without restarting; both should build and go in, and setup-log.txt should not end with "access denied".
