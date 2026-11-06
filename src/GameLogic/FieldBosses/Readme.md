# Field bosses

The field bosses of Season 21 on the shipped maps: Medusa (Swamp of Calmness), Lord Silvester (Urk Mountain), Core
Magriffy (Nars), Lord of Ferea (Ferea), Nix (Nixie Lake) and the God of Darkness (Swamp of Darkness).

`FieldBossesFeaturePlugIn` (off by default, the world's switch), configured by `FieldBossConfiguration`:

- **Respawn**: since S19 Part 2-3 one rule for all, 18 hours plus a random 0-6 hours after the kill
  (`RespawnMinimum`, `RespawnRandom`; S21Emu `BossFightSystem.xml`). At server start they spawn at once (`SpawnOnStart`).
- **Stages**: a boss is a list of stages; when a stage monster dies, the next spawns (Ferea: crystal orb, then the lord;
  God of Darkness: swamp monster with its pets, water monster, god). A stage has summons (Medusa's 7 Sapi Queens) and
  health phases (Ferea's knights at 90/70/50/30 %).
- **Arena** (optional): the gates into it and the entry NPC (Nix: the Spirit of Nixie, the party within 10 tiles comes
  along) are closed while the boss is away; it takes `MaximumPlayers` (God of Darkness 10) and can close once the fight
  started (`ClosesWhenEngaged`). The battle time (`BattleMinutes`) starts with the first hit; when it runs out, the
  battle is lost, the monsters leave and the boss returns after `FailedRespawn`. After a kill the arena's players are
  moved to the map's spawn gate after `ExitDelay` (5 minutes).
- **Announcements**: a golden notice to the whole server for a spawn, a kill (with the killer) and a lost battle, which
  the Season 6 client shows; the Babylon client also gets `C1 EE A1 FieldBossState` (`docs/babylon.md`).
- **Game master**: `/fieldboss <name>` spawns a boss now, `/fieldboss` lists them with their state.

The bosses' numbers are normalized by the progression with their map (`MonsterNormalizer` bosses, 20x a normal);
Medusa stands on a Season 6 map and keeps its numbers. The data comes with the update "Add the field bosses of
Season 21".

- **Special skills** (`FieldBossStage.Abilities`), used during a fight: area strikes (a share of the stage monster's
  damage to every player within a distance: Ferea's whirlwind and land mine, the water monster's wave and water
  course, the god's flame and darkness, S21Emu `Event/WorldBoss`) and summons up to a number alive (the swamp
  monster's 8 pets every 20 seconds).
- **Grounds** (`Grounds`): a boss spawns on one at random, its stages and summons move along (Medusa's four grounds of
  `BossFightSystem.xml`).
- **Entry item** (`EntryItemGroup`, `EntryItemNumber`): the entry NPC takes one from each player it lets in (Nix: the
  Spirit Nixie's Shard 14/390, from the S21 client's item table); `EntryItemNpcNumber` hands it out for
  `EntryItemPrice` zen (Priest James 725 in Elbeland and Loren Market, the data's spawn points).
- **Restart**: the state file (`StateFile`) keeps the timers; a killed or waiting boss keeps its respawn time, a living
  one spawns again.

Not built: Nix's barrier stones and knowledge absorb, the roar of the Lord of Ferea, the water source healing (the
data names them without numbers which say what they do).
