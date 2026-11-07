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
- **Debuff fields** and **formulas** (the value of a node by its level, `level` as variable). A learned node is a
  learned skill with its level.
- **Passive effects**: the passives which raise an attribute (Add Max HP 1137, physical 1141, magic 1142, curse 1157,
  both 1146, skill damage 1144) carry a `MasterSkillDefinition` without a root: its value formula is the S21Emu formula
  of the level (one formula up to 30 points, the second above) and its passive power-up adds it, like a passive master
  skill. No root means no master points go into it and the master skill list leaves it out. The stat point, impenetrable
  defense and 4th wing passives, the other node effects and the debuff fields stay data.
- **Node effects** (`EnhanceTreeEffects`): "Add Ext Skill Damage" (1001) adds the value of its formula at its level to
  the damage of the skill which the extension node of its sheet (index 0, e.g. Meteor Strike Extended 1075 for Meteor
  Strike 724) improves, while that extension is learned (`AttackableExtensions.GetDamage`). The node is one skill in
  every sheet of a class, so its level counts for each learned extension. The lookups are indexed per configuration and
  the formula values cached. Left: extra damage rate, damage count, range, splash, speed, targets, the debuff nodes.

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
