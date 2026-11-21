Fixed: Two gates opened by the same plaques count as one puzzle for the bots
In Katzenstein's Lab, Gate02 and its twin open from the same four plaques. One bot was setting the plaques for one gate while another set them for the twin, so they still undid each other. Now a puzzle is known by its pieces, so only one bot sets those plaques whichever gate it is after.
Where: Katzenstein's Lab (Marleybone), and any two gates sharing puzzle pieces, no setting
Test: Send bots into the same Katzenstein's Lab copy and check that only one steps on the plaques while the others wait.
