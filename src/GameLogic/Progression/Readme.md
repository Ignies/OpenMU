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

## Master tree points (`MasterTreeRules`)

Season 21 gives master tree points up to master level 400; the later levels (a total level above 800) feed the skill
enhancement tree (`EnhanceTreePoints`). `MasterTreePointsCap` (400, 0 = no cap) stops the points of the master levels
in every mode, and the stars' points at the same number, so a master level 1350 character fills about a third of the
Season 6 tree instead of all of it. From the total level 800 a monster gives master experience only from level 160
(`EnhancedMinimumMonsterLevel`, S21Emu `MinMasterExperienceExtMonsterLevel`; `MasterExperienceRules`, also used by the
bots and the offline helper).

## In the running world

`ProgressionFeaturePlugIn` applies the mode without changing a stored row, so switching back to Original restores
every number:

- **Season 21 maps** are the maps with a total-level requirement (`Stats.TotalLevel`). `ProgressionWorld` computes
  their requirement (level, master level, stars) and warp zen; the map check (`TryGetRequirementError`), the warp list
  and the gates ask it instead of the stored total level.
- **Monsters** which spawn only on Season 21 maps are normalized per set (maps sharing monsters, or with the same
  safezone map and requirement), at the median requirement of the set's maps. Spawned monsters get multipliers on
  their attributes, shared per definition, so a mode change reaches the living ones too.
- **Classic maps**: their warps and gates take the Season 21 levels of `Season21ClassicWarpLevels` and
  `Season21ClassicGateLevels` (S21Emu `Move.txt`, `Gate.xml`; the progression update fills them) in the original mode;
  the normalized modes keep the Season 6 levels unless `ClassicMapEntryLevels` asks for the Season 21 ones or their
  normalized level T(R) (`ProgressionMoveExtensions`). A warp or gate the tables don't hold keeps its stored level.
- **Capped worlds**: a world never compares a raw Season 21 total with caps below it. Without a mapping mode (Original,
  or the feature off) a total above level cap plus master level cap is the caps (`GetReachableTotalLevel`: the map
  check, the evolution trials, `Season21LevelCheck`), and a warp or gate level is at most the level cap. A refused map
  entry names what is missing and how much ("Missing requirement to enter the map: Resets 4").
- **Drops** roll with the normalized level, capped (`DefaultDropGenerator`).
- **Level caps** are set on the running game configuration (400 and master 200 or 0); Original puts the stored ones
  back. The experience tables are built for the stored caps, so the progression only lowers a cap. The stored caps are shared per game configuration, so
  several game servers on one configuration keep the first stored ones; the last one turned off puts them back.
- **Attack speed**: `AttackSpeedLimit` replaces the maximum of the attack and magic speed the same way (empty keeps
  the database's 200, 0 is none as S21Emu `DisableAttackSpeedLimit = 1`). The extended stats packet still names 200
  as the maximum when there is none.
- **Events**: a Season 6 grade checks level plus master level (Normalized) or the power level E (Stars), capped at 400;
  a Season 21 grade (above the level cap) checks the Season 21 total the key stands for (the inverse of T). In Stars a
  master class grade needs a star.
- **Stars**: a reset needs the third class; it gives the class's points per level times 100 (50 after star 5), the
  quest points, and 20 master tree points. Each star takes 6% experience (down to 40%).
- A plugin without a configuration does nothing: a database gets the row (off, stars) at initialization or startup,
  and the update adds it when it's missing. `AddProgressionUpdatePlugIn` turns it on in the stars
  mode and the reset feature with the star costs, keeping operator values.

- **Star power**: 1% more damage against monsters and 1% less damage from them per star, up to 10%
  (`Stats.FinalDamageIncreasePvm`, `Stats.FinalDamageReceiveDecrementPvm`); with `StarPowerInPvp` it applies to all
  damage instead. **Items** check the power level E (capped at 400) against their level requirement in Stars.
- **Turning the plugin off** puts the stored caps back at once and takes the monster and star numbers off
  (`IDeactivationAwareFeaturePlugIn`).

## Level penalties (`LevelPenaltyPlugIn`, `LevelPenalties`)

S21Emu `DamagePenalty.lua` and `ExperienciePenalty.lua`: a character below the monster's level + 600 deals 10 % less
damage per level to it and takes 10 % more from it, at most 7 levels (70 %). Outside the Labyrinth of Dimensions (map
115) only a fourth or fifth evolution has it (`CheckEnhanceLevel`); in the labyrinth every evolution from total level
400. Monsters of the Kardamahal Underground Temple (map 143) give 10 % less experience per level, at most 70 %.
The five floors of the Deep Dungeon (maps 116-120, `PenaltyFreeMaps`) have no damage penalty: their renewal removed
the hunting penalty (Webzen Korea patch notes, muhelper.com), which the Lua does not know.
`MapPenaltyLevels` (map, total level) gives a map the fixed hunting penalty level of the Webzen guides (entry + 50,
e.g. Tormenta 141: 1450, Kardamahal 143: 1550) in place of the monster level + 600; empty by default, as in the Lua.

- The threshold and the 400 are Season 21 total levels and map through the mode with `EvolutionRules.GetLevelsMissing`,
  the helper of the evolution rules: the levels below count in the mode's unit (total, level + master level, power
  level). A normalized level 400 character is above the mapped threshold of a level 150 monster, not 350 short.
- The monster level is the stored one (`MonsterDefinition`), the Season 21 level the threshold is written for.
- Damage: after the PvM final factors in `AttackableExtensions.CalculateDamageAsync`, rounded like the Lua's floor.
  Experience: an `IExperienceCalculationPlugIn` factor, also for master experience and party members.
- Left open by the Lua and therefore settings: the evolution of `CheckEnhanceLevel`, the maps, and switches for the
  three parts. Off by default (`IDisabledByDefault`), fork feature `level_penalties`.
