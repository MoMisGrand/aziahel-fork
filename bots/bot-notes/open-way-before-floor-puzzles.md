Fixed: Bots no longer solve another quest's gate puzzle when the way to their own step is already open.

In Katzenstein's Lab, Libra had just talked to Watson on the open west side and needed Firegut's teleporter, further west with nothing shut on the way. The floor rules still sent him east to step on the plaques for Gate02 (2), which belong to the next quest. From there the only way back went through Gate02, which stays shut until Firegut is beaten, so he stood at it for 11 minutes. Umbella, Hydra and Consumer got stuck at the same spot earlier today. Now, when the walk to the exit or door his step needs is on his own floor and passes no shut door, he goes there first, and the floor's puzzles wait.

Where: Ultimus, the Calling of 7 and the Kitharas Family in dungeons (Katzenstein's Lab most of all)
Test: Send a bot into Katzenstein's Lab on "The Right Combination". After Watson it should walk west to Firegut, with no "This floor first" line about the plaques and no stop at Gate02.
