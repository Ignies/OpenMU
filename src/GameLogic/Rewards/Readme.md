# Rewards

The reward sources of Season 21 which deliver into the gremory case (`../GremoryCase`). `RewardDelivery` creates the item
and puts it into the character's case; when the case isn't available (switch off, full, or a Season 6 client) into the
inventory; with neither, the reward is lost with a message. An unknown or inactive item (e.g. a Muun on a world without
the wide items) is skipped with a warning.

- `LevelRewardsPlugIn` (off by default, `LevelRewardsConfiguration`): a character which reaches a total level (level plus
  master level) of a row gets its item, filtered by base class (empty: every class). It fires on every level-up, so a
  level is rewarded once per climb; a reset that lowers the level would reward it again (S21 has no resets by default).
  Data: `ItemCompensationLevel.xml`, a Muun (16/69 ...) at every 50 total levels from 950 to 1750, for every class.
- `EventContributionPlugIn` (off by default, `EventContributionConfiguration`): kills in a running event give points by
  kind of event (`EventContributionRule`: monster, Sunday monster, player, winner, special monsters); when the event ends,
  each player with points gets the box of their range (`ContributionBox`) and the points are dropped. Data
  (`EventMapPointIntegration.xml`): Mastery Box Minor 100-400, Standard 401-800, Greater 801+ (14/282-284); Blood Castle
  5 a monster, gate 131 100, statue 132 200; Chaos Castle 13, player 50, winner 99; Devil Square 1, rank 1 (the winner)
  200; Illusion Temple player 10, winner 200; Imperial Guardian 45 (Sundays 22), bosses 504-511 100; Doppelganger 2,
  player 10, ice walker 531 100, winner 200.
- Both are configured and switched on by the update "Add the level rewards and event contribution boxes of Season 21".

Not built: Devil Square ranks 2 and 3 (150, 100) and the Illusion Temple statue points (the events have no hook for them),
the Blood Castle class restriction switch of the data.
