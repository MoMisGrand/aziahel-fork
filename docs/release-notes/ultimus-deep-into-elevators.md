Fixed: Ultimus stopping short of the elevator teleporters in Big Ben

Big Ben's teleporters up sit in a small elevator just past each floor's door. Our map draws the way into that elevator as a wall, so Ultimus could stop just short of it and never get taken up. Now, when he stops short of a teleporter in a dungeon, he walks straight in, all the way to the back of the elevator, then tries its left and right sides until it takes him. If nothing works after eight tries, he says so in his log and tries his other fixes.

Where: Big Ben's elevators on every floor, and any dungeon teleporter he stops short of.
Test: Let Ultimus climb Big Ben, and check that he walks all the way into each elevator and goes up without standing at the door.
