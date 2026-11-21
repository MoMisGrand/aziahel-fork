Fixed: Bots stop turning back and forth at Katzenstein's Lab's plaques

In Katzenstein's Lab two gates stand side by side and open from the same four plaques, the first time before Clanker and the second time after him. The bots counted the second gate, still locked, as a door in front of the plaques whenever they stood a little east, so they dropped the plaque and headed for Clanker, then two steps later picked the plaque again, every second for hours. A gate that opens from the same plaques can never block the way to them, so the bots now keep going to the next plaque until the first gate opens.
Where: Marleybone, Katzenstein's Lab, "Weird Science" (Kitharas Family, Ultimus, the Calling of 7, Quest Team).
Test: Send a bot into Katzenstein's Lab after Firegut; it walks plaque to plaque in order (Sun, Bird, Tree, Bug) without turning back, and Gate02 opens.
