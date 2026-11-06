# Elite zones

The elite zones of Season 21 (S15 Part 2 on): every map from Scorched Canyon on (and Atlans Abyss 3) has five
elemental areas, Fire, Water, Earth, Wind and Darkness, at the entrances Webzen publishes; the map's elites spawn
inside them instead of over the whole map.

- `EliteZonesFeaturePlugIn` (off by default, the world's switch) holds the zones (`EliteZoneConfiguration`).
- Entry rule: a walk into a zone is refused (`IWalkRequestValidationPlugIn`) when the character misses the map's
  Season 21 entry level plus `LevelAboveMap`, checked like the map itself (`Progression/Season21LevelCheck`, so the
  normalized and stars modes map it). Webzen's guides name no rule of their own, so the default is 0: a zone asks what
  its map asks. Walks which start inside a zone are never refused.
- The Babylon client gets the zones of the map on entering it (`C2 EE A0`, `docs/babylon.md`).
- The data (zones, elites, Twisted Karutan's terrain) comes with the update "Add the elite zones of Season 21"
  (`Persistence/Initialization/VersionSeasonSix/Season21`, see its readme).

- **Debuffs** (`Debuffs`, `IAttackableGotHitPlugIn`): an elite (its name says "Elite") standing in a zone puts the
  debuffs of the zone's element on the character it hits (S21Emu `EliteMonster.txt`, rate 100): bleeding, a share of
  the hit a second up to a maximum; poisoning, 2-3 % of the maximum health a second; attack (Weakness), attack speed
  (points) or defense (Innovation) down. The effects show with the magic effects explosion, poisoned, weakness, iced
  and innovation. Webzen's guide gives darkness bleeding and defense down; the data says attack speed down, which is kept.
- **Weekly drop limits** (`DropLimits`, `IMonsterItemDropPlugIn`): per character and item, from S21Emu
  `ItemDropLimit.xml` (Garuda's Flame, Sealed Armor Fragment 5, the guardian fragments, Golden Crest, Jewel of
  Harmony 10), for every monster drop like the data's drop type 10; the week starts on `WeekStartsOn` (Tuesday, the
  data's day 2). The counts live in the state file (`StateFile`, `GameLogic/WorldStateFile`), so a restart keeps them.
- The Elite Zone Flags (838, 866, 925) stand at the entrances (the update places them; the S21 client draws
  `obj_flag.bmd`).

Not built: the weekly rotation of Forgotten Raklion (the map isn't shipped).
