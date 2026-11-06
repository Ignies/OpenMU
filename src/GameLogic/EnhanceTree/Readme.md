# Skill enhancement tree

The tree of the fourth and fifth evolution (Season 21), fork feature id `enhance_tree`. `EnhanceTreeFeaturePlugIn` holds
it as its configuration (off by default); the update "Add the skills of Season 21" fills it from the S21Emu data and
turns it on.

## Data (`EnhanceTreeConfiguration`)

- **Nodes**: per base class, sheets (`Group`, one per main skill) of nodes. A node teaches a skill; its first step
  costs `MinimumLevel` points (the node is active from there), each further step one, up to `MaximumLevel`. It may
  require skills with points (`-1`: the skill is learned or available, e.g. the main skill of its book).
- **Passives**: unlocked in up to three stages by the points spent in the class's tree; each stage has its point
  cost per level and its highest level.
- **Debuff fields** and **formulas** (the value of a node by its level, `level` as variable): data only, the server
  doesn't simulate the tree's effects yet. A learned node is a learned skill with its level, so its passive or
  extended skill is there for the effect code when it comes.

## Points (`EnhanceTreePoints`)

Season 21 gives one point per level above the total level 800 (level plus master level), up to 950
(`PointsStartAt`, `MaximumPoints`). The progression mode reads the level as that total, like a map requirement:

| Mode | Total level read as |
|---|---|
| Original (or progression off) | level + master level |
| Normalized | R(level + master level): the master levels feed the tree |
| Stars | R(power level E = level + the stars' worth): each star feeds the tree |

R is the inverse of the progression's level mapping (`ProgressionMapping.GetSeason21Requirement`). At the fourth
evolution's own gate (T(800), about 335) a character has a few points; 10 stars at level 400 reach the 950.
The free points are computed from the level and the learned tree skills, nothing is stored.

## Actions and views

`AddEnhanceTreePointAction` adds a point (result `EnhanceTreePointResult`); `IEnhanceTreeViewPlugIn` shows the tree and
the result. The Babylon client gets them as `EE 40`-`42` (docs/babylon.md); a Season 6 client has no view of it.
