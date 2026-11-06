# Rewards

The reward sources of Season 21 which deliver into the gremory case (`../GremoryCase`). `RewardDelivery` creates the item
and puts it into the character's case; when the case isn't available (switch off, full, or a Season 6 client) into the
inventory; with neither, the reward is lost with a message. An unknown or inactive item (e.g. a Muun on a world without
the wide items) is skipped with a warning.

- `LevelRewardsPlugIn` (off by default, `LevelRewardsConfiguration`): a character gets each reward whose Season 21 total
  level it meets and which it didn't get yet, filtered by base class (empty: every class). It checks on every level-up,
  master level-up and map entry, so a level passed before the reward existed is given on the next check.
  - The threshold goes through the progression mode like the evolution levels (`Classes/EvolutionRules.IsMet(int, ...)`,
    `Player.MeetsSeason21Level`): original the total level, normalized level plus master level against the mapped level,
    stars the power level of level and stars. So 950-1750 is reachable under the 400 cap.
  - Each reward has an `Id` (1-64); `Character.ClaimedLevelRewards` holds one bit per claimed id, so a reset (a star) and
    the climb back never give it again. An id is a contract with the characters: never reuse it. A failed delivery
    isn't recorded and is tried again.
  - Data: `ItemCompensationLevel.xml`, a Muun (16/69 ...) at every 50 total levels from 950 to 1750, ids 1-17.
- `EventContributionPlugIn` (off by default, `EventContributionConfiguration`): kills and objectives in a running event give
  points by kind of event (`EventContributionRule`: monster, Sunday monster, player, winner, special monsters, rank points,
  relic claim and relic score); when the event ends, each player with points gets the box of their range
  (`ContributionBox`) and the points are dropped. Data (`EventMapPointIntegration.xml`): Mastery Box Minor 100-400,
  Standard 401-800, Greater 801+ (14/282-284); Blood Castle 5 a monster, gate 131 100, statue 132 200; Chaos Castle 13,
  player 50, winner 99; Devil Square 1, ranks 200/150/100; Illusion Temple player 10, relic taken 50 (the data's statue
  kill), relic delivered 30 (statue defeat), winner 200; Imperial Guardian 45 (Sundays 22), bosses 504-511 100;
  Doppelganger 2, player 10, ice walker 531 100, winner 200.
- Objectives come through the plugin point `IMiniGameObjectiveReachedPlugIn` (`MiniGameContext.NotifyObjectiveReachedAsync`):
  devil square sends every finisher's rank, illusion temple the taken and the delivered relic.
- Both are configured and switched on by the update "Add the level rewards and event contribution boxes of Season 21"
  (version 2 adds the reward ids, the rank and the relic points to a version 1 configuration).

Not built: the Blood Castle class restriction switch of the data.
