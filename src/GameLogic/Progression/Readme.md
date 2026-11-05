# Progression

How a world with the Season 21 maps progresses. One setting, `ProgressionConfiguration.Mode`, of the
`ProgressionFeaturePlugIn` (off by default, editable in the admin panel):

- **Original**: the Season 21 numbers as the data has them (total levels up to 1750, master level 1350).
- **Normalized**: Season 21 requirements mapped into level 400 plus master level (cap 200); monsters re-derived on
  the Season 6 curve.
- **Stars**: like Normalized, but above the first star band a requirement asks for stars (resets) instead of master
  levels; master levels are off and stars give the master tree points.

The Season 21 rows stay untouched; every normalized number is computed from the settings.

## The level layer (`ProgressionMapping`)

- `T(R) = A1000 * (R / 1000) ^ k`, with `k = ln(A400 / A1000) / ln(0.4)`; the anchors default to `T(400) = 190` and
  `T(1000) = 400`.
- Normalized: level `min(cap, T)`, master level `T - cap`, rounded to 5.
- Stars: `S = floor((R - 400) / 300)` from `R = 700`, level `T - worth(S)`; so `R = 1000` asks for 2 stars and level 200.
- Warp zen `max(2000, 1000 + 50 * T)` in steps of 500, fitted to the Season 6 warp list.
- Item drops roll with the monster level capped at 160: the highest drop level is 150 and the drop generator keeps
  items within 12 levels, so a monster above 161 would find no item.

## The monsters (`MonsterNormalizer`, `Season6MonsterCurve`)

The anchor level of a set is `M = 7.602 * T ^ 0.5026`, the median monster level of a Season 6 hunting map against its
entry level. Each monster column is the Season 6 curve at `M` times the monster's value over the median of its set's
spawned normal monsters, clamped by kind (normal 0.5-2x health, elite 1.5-3x, boss 20x). Experience and money follow
from the level through the game's own formulas.

## Stars (`StarRules`, `StarResetDefaults`)

A star is a reset of the reset feature (`Stats.Resets`). Its costs are the reset configuration's
(`StarResetDefaults` seeds them: 10 stars, level 400, 5,000,000 zen times the star, Jewels of Life from star 6).
Its rewards are the progression configuration's: stat points of 100 levels of the class per star (50 after star 5),
20 master tree points, 1% damage and defense against monsters per star up to 10%, and the power level
`E = level + 100 per star (50 after star 5)` for item and event requirements. Each star takes 6% experience, down to 40%.

A reset which resets the stats keeps the level-up points of the finished quests (`ResetPointsExtensions`).

## Not wired yet

The configuration and the rules are in place; applying them to the running world (level caps, warp list and gates,
monster attributes, entry checks by stars, star rewards in the reset, the drop cap, events, items, experience) follows
in later changes.
