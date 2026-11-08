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
  defense and 4th wing passives stay data.
- **Node effects** (`EnhanceTreeEffects`): "Add Ext Skill Damage" (1001) adds the value of its formula at its level to
  the damage of the skill which the extension node of its sheet (index 0, e.g. Meteor Strike Extended 1075 for Meteor
  Strike 724) improves, while that extension is learned (`AttackableExtensions.GetDamage`). The node is one skill in
  every sheet of a class, so its level counts for each learned extension. The lookups are indexed per configuration and
  the formula values cached. The same rule gives the other options of a sheet:
  - Damage count 1002-1004 (formulas 15, 30, 45 / level, learned at 15): 1, 2 or 3 hits more of the skill
    (`AreaSkillAttackAction`, `TargetedSkillDefaultPlugin`).
  - Splash 1010 (10 / level at 10): one hit on each monster within a tile of the target; Splash Count 1154 one hit more,
    Splash Radio 1153 one tile more (muonlinefanz.com tree tool: "one DMG Counter to monsters within 1 tile").
  - Range 1011, radius 1009, targets 1012 (formulas 10 / level and alike, learned at 10; tree tool "ATK Range
    Increase", "+1 AOE Range", "+1 Max Targets"): `GetReach`. The range adds to the distance a skill reaches its target
    (`TargetedSkillDefaultPlugin`, the extra target of `AreaSkillTargetSelector`) and to an area around the caster; the
    radius to the effect range, the target area, the area around the caster and the implicit targets of a targeted
    skill; the targets to an area skill with a limit (`AreaSkillSettings.MaximumNumberOfHitsPerAttack`), an unlimited one
    hits every target already. `SkillHitValidator` and `DeferredHitScheduler` check the same widened area.
  - Weapon damage 1020 (physical, level * 0.75), 1021 (wizardry) and 1156 (curse, level): while the skill is used, the
    weapon's part of its damage type rises by the percentage (tree tool "Increases the equipped weapon's WIZ Raise by a
    percentage when using the skill"): the physical damage of the weapon (`MinimumPhysBaseDmgByWeapon`), the staff rise
    or the book rise (`GetWeaponRise`, `AttackableExtensions.GetBaseDmg`).
  - Extra damage rate 1007 (formulas 50-53, tree tool "+1.12 % Mastery DMG Chance" at 1): the chance in percent of a
    Mastery DMG hit of the skill. No source has the amount of the Mastery DMG, so it is the setting
    `MasteryDamagePercent` (percent of the hit, 0 by default: the node does nothing until an operator sets it).
  - Left as data: speed 1008 (client animation), cooldown 1019, the explosions 1125-1135 and 1196-1201.

## Debuffs (`EnhanceTreeDebuffs`)

The debuff nodes of a sheet (colors 1 and 2) place Poisoning, Chilling and Bleeding through a circle (S21Emu
`EnhanceSkillTreeMagicField.xml`; meanings from the muonlinefanz.com tree tool and Webzen guide 112). Called after each
skill hit of a player (`Player.AttackByAsync`, `AttackableNpcBase.AttackByAsync`), with the nodes of the used skill's
sheet (extension learned, like the node effects):

- Rate nodes (1024-1026, 1046-1048, %): the chance to cast the circle of the kind at the target. It lasts 10 seconds,
  then waits for its cooldown (20 seconds from the cast), `MagicFields`.
- While it lasts, a skill hit on a target inside it (`DebuffCircleRange` tiles plus the range nodes 1172/1174/1176)
  places the basic debuff, at most on `DebuffCircleTargets` plus the target nodes (1166/1170/1168, 1190-1192) targets,
  with the success rate of the debuff plus the chance nodes (1178-1189) of `DebuffRateScale` (per mille: 200 = 20 %).
- With the upgrade node (1040-1042), a hit on a debuffed target inside it upgrades it (Toxic, Frostbite, Bleeding
  Severe) with the improved success rate, chance and target nodes.
- Effects (`Debuffs`, MagicFieldBuffInfo; effect numbers 273-279, `EnhanceTreeDebuffEffect`; a Season 6 client sees
  none of them): seconds plus the duration nodes (1032-1034).
  - Poisoning: 2000 + 0..1000 + the value node (1028) each seconds / count (1.8 s); Toxic twice that plus the improved
    value nodes (1036, 1058).
  - Chilling: defense - (106 + value node 1029); Frostbite also attack and magic speed - (20 + improved nodes 1037/1059) %.
  - Bleeding: each later skill hit on the target (any player's) bleeds 20 % of it plus the value node (1030), 10 times;
    Bleeding Severe twice that plus the improved value nodes (1038, 1060).
- No source, so settings: the circle's base targets and range (1 each) and the rate scale. Left as data: the explosions
  (1125-1135, 1196-1201), the movement slow of Chilling and the shock/stun line (no S21Emu field).

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
