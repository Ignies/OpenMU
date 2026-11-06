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
Season 21". The state lives in memory: a restart spawns the bosses again.

Not built: the special skills of the bosses (Nix's barrier stones and knowledge absorb, Ferea's whirlwind and land
mines, the god's flame and darkness), Medusa's random choice of four grounds (ground 3 is used), Priest James and
the Spirit Nixie's Shard (the data has no such item; the statue lets the party in).
