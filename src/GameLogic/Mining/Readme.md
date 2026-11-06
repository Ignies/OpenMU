# Mining

The Pandora mining of Season 21: mining spots stand in the safe zones of Lost Tower, Tarkan, Aida, Swamp of Calmness,
Raklion and Kubera Mine. A character holding the Pandora Pick (0/41, sold by Bolo in Karutan) mines a spot stage by stage
for Jewels of Bless; after each stage it may stop and collect, and a failed stage loses what the spot gave so far.

- `MiningFeaturePlugIn` (off by default, the world's switch) holds the spots (`MiningConfiguration`). Each spot kind is an
  NPC (605 small, 606 medium, 607 large) with five stages from S21Emu `Mining.txt`: success rate 4750/10000 per stage,
  the jewels when stopping after it (small 1/3/7/16/33, medium 2/6/15/34/67, large 4/12/29/65/125), and a jackpot after
  the last stage (2.5 % of 100, 1.9 % of 400, 1.8 % of 1000). After the last stage the jewels are collected by themselves.
- The column `Value` (2, 4, 8 by spot size) is read as the durability the pickaxe loses for an attempt; the data doesn't
  name it, the guides say the pickaxe wears and is repaired. A worn pickaxe (durability below the cost) can't mine.
- Design (no source): the reach (`MaximumDistance`, 3 tiles like an NPC talk), one miner per spot (`OneMinerPerSpot`),
  and the delivery: the jewels go into the inventory, else the gremory case (`Rewards/RewardDelivery`), packed into
  Packed Jewels of Bless of 30, 20 and 10 (`Bundles`) so a large find takes few slots.
- Talking to a spot opens the window of the Babylon client (`C1 EE A3`, result 0); `C1 EE A2` mines a stage or collects
  (`docs/babylon.md`). A Season 6 client has no mining window: talking to a spot tells it so.
- The data comes with the update "Add the mining of Season 21" (`Persistence/Initialization/VersionSeasonSix/Season21`).
  It also sells Bolo's pickaxe at its full durability: the shop rows of the data give it the durability 1, a count there.
