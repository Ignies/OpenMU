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

Not built: the elite debuff on hit (S21Emu `EliteMonster.txt`: bleeding or poison plus attack, attack speed or
defense down per element), the weekly drop caps, the Elite Zone Flag NPCs (838, 866, 925: no client model), the
weekly rotation of Forgotten Raklion.
