# Season pass

The season pass (MU Pass) of Season 21, from S21Emu `Event/BattlePass/BattlePass.xml`. During a season of the schedule,
missions give pass experience; 20 levels (100 to 6000 experience) each give the rewards of the free pass, and of the gold
or premium pass when the character owns it. Buff rewards last until the season ends, item rewards go to the gremory case.

- `SeasonPassPlugIn` (off by default, the world's switch) keeps the pass per character (`CharacterSeasonPass` in
  `StateFile`, `state/season_passes.json`); a new season starts it again (no experience, passes, claims or buffs).
- Schedule: the data lists the twelve months of 2025 (the 1st 00:00 to the 30th 23:59, February the 28th). With
  `RepeatEveryYear` (design, on) the dates repeat every year; the 31st of a month runs no season, as in the data.
- Missions: 25 daily missions (they start again at 00:00 UTC, Webzen's guide) and one special mission (design: it counts
  for the whole season), each with its experience, count and completions. The data only numbers them and the client's
  mission texts aren't in any reference, so each `Kind` starts as `None` and the operator picks what it counts:
  `MonsterKills`, `Experience` (by kills, normal and master) or `PlaySeconds` (time in the world, added on a map change).
- Tickets: the Exp Pass Ticket (20/122) gives 300 pass experience, the Gold Pass Ticket (20/123) the gold pass, the
  Premium Pass Ticket (20/124) the premium pass on a character with the gold pass. The data has the premium pass off.
- Buffs: a claimed buff raises its attribute by the value of the highest level at which it was claimed. Only "Increase
  Max HP" (effect 411) names its attribute (maximum health); the others (base experience, soul crystal chance, Tempest Muun
  multiplier) have no attribute of this server, so their `AttributeId` is empty until an operator or a later change sets one.
- The Babylon client asks with `C1 EE D7` and claims with `C1 EE D5`; it gets `C2 EE D2` and `C1 EE D6` (`docs/babylon.md`).
- The data comes with the update "Add the season pass of Season 21" (`Persistence/Initialization/VersionSeasonSix/Season21`).
