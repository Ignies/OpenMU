# Star Smithing

Players with the Star Smith's Hammer change their own items (Hidden-Babylon `docs/star_smithing.md`). This folder is the
shell (PR 3): the hammer, the smith's level and experience, and four actions on an item of the inventory. Recipes,
the bench, effects and auras come in the later PRs.

- `StarSmithingFeaturePlugIn` (feature `star_smithing`, bit 26, off by default; `StarSmithingConfiguration`: the hammer's
  group and number, 2/1000; the durability an action takes, 5; the Zen a recolour costs, 500,000). It sends the smith's
  state, `C1 EE 91 StarSmithState` (hammer in a hand, broken, level, experience, experience to the next level, the
  hammer's durability), on every map entry and when the hammer is put on or taken off.
- Actions, each answered by `C1 EE 97 StarSmithResult` and a new EE 91: recolour (`C1 EE 93`, the 31 look bytes of
  `ItemLookBytes`, then the bag slots of the colour dye and of the Radiant Dye; only the colours are kept, a weapon effect
  comes with recipes; costs Zen and spends dyes, below), name (`C1 EE 94`),
  lore (`C2 EE 95`), lock (`C1 EE 96`). `StarSmithRules.Check` decides in this order: the switch, the hammer in a hand,
  its durability (not for a lock), the place (no trade, open shop, duel or event), the item (an item of the inventory,
  not the hammer), the lock, the Zen of a recolour. A name or lore then goes through the text moderation
  (`TextModerationPlugIn.Check`); an empty one removes it. Each change writes the item's `ItemCustomization` (a new
  record is Common), raises its `Version` and calls `ItemCustomizationCache.Forget`, so everyone who sees the item gets
  it at once (EE 39). A lock is permanent for players (`Locked`, `LockedAt`); only an admin unlocks.
- A done action first shows the smith hammering at a bench to the smith and everyone who sees them,
  `C1 EE 92 StarSmithAnimation` (the smith's id as each receiver knows it, the action, 3 strikes); clients hold the
  smith's result line until the last strike (PR 4).
- The smith's level and experience are on `Character` (`StarSmithLevel`, 0 read as 1; `StarSmithExperience`).
  `StarSmithLevels`: 1 to 100, round(100 * L^1.6) to the next level; experience comes with recipes.
- The hammer item and its place in Hanzo's shop are a data update (`AddStarSmithHammerUpdatePlugIn`).

## Dyes
A recolour spends dyes (Hidden-Babylon `docs/star_smithing.md` section 9):
- `StarSmithingConfiguration.Dyes` (`StarSmithDye`: the item, up to 8 swatches as 0xRRGGBB, the strength cap, and the
  Hue, Keep and Radiant flags). The defaults (`DefaultDyes`) are 14/1000-1014: eight basic dyes (four shades of their
  colour, strength up to 85), five rare ones (three special colours, full strength, the keep mask), the Prismatic Dye
  (the hue shift) and the Radiant Dye (the glow).
- `StarSmithDyes.Check` decides a recolour after `StarSmithRules.Check`: the original colours spend no dye; any other
  look needs a colour dye in the slot named, one of its swatches, at most its strength, the hue shift only with a Prismatic
  Dye, the keep mask only with a rare one, and a glow only with a Radiant Dye beside it, in a swatch of the colour dye.
  `EE 97` says `NoDye` (9) or `WrongDye` (10). A done recolour takes one of each dye it used (`SpendOneAsync`: one off the
  stack, or the item when it was the last).
- `C2 EE 98 StarSmithDyes` goes with EE 91 while the hammer is in a hand, so the client shows only swatches the server
  accepts.
- Items and offers are a data update (`AddStarSmithDyesUpdatePlugIn`, `StarSmithDyeItems`): stacks of 20, the basic dyes
  at Hanzo, the others in one drop group on every map from monster level 60 (one kill in 2,500). The Season 21 shops keep
  them at Hanzo like the hammer.
- The website edits the table (`StarSmithingOperations`, `smithing.dyes` and `smithing.setDyes`, previewed and applied,
  checked by `StarSmithDyes.Validate`: at most 64 dyes, each an item that exists, none twice, 1 to 8 colours, the Radiant
  Dye bare). Players get the new table with their next EE 91.

Tests: `tests/MUnique.OpenMU.Tests/Babylon/StarSmithRulesTest.cs`, `StarSmithLevelsTest.cs`, `StarSmithingViewTest.cs`,
`ItemLookBytesReadTest.cs`, `StarSmithDyesTest.cs`; `tests/MUnique.OpenMU.Persistence.Initialization.Tests/StarSmithDyesDataTest.cs`;
`tests/MUnique.OpenMU.Web.Tests/Site/StarSmithingOperationsTests.cs`.

## Constellation Forge

Every recipe attempt is played as the Forge minigame (Hidden-Babylon `docs/star_smithing.md` section 10). This is its
rule core, deterministic from a seed so the server can deal an attempt again and verify it, its wire, the recipes, the
Guidebook and the experience (below). An attempt lives on the player; the Guidebook is stored on the character.

- Recipes: `StarSmithRecipes.Candidates` lists the recipes a target and its ingredients can make, the most specific
  first (`Match` is its first); `Chance` is the base, +0.15% a smith level (15% at most) and the catalysts, capped by
  the recipe's maximum; `Magnitude` scales a roll by `1 + 0.5 * (L / 100)^0.8`. A recipe has a `Shape`, an `OnFail`
  and a `Risky` flag.
- `StarSmithShape`: the stars it links, in drawing order; `Ordered` false lets it start at either end; `Exact` or
  `Extendable` (each link past it adds `ExtensionPercent`, up to `MaximumExtension` links). `IsPrefix` is the guidance
  test, `Extension` the completion test, `Follows` either.
- `StarSmithSky.Deal(seed, recipes, target, level, ingredients, catalysts, bonus, rules)`, with its own SplitMix64
  dice (`StarSmithDice`), draws in a fixed order: 12 positions in the item's box (0 to 1, kept apart), elements (fire,
  frost, storm, shadow, light) and values (1 to 5), then voids. Each recipe rolls its chance; a failed roll puts a void
  on its shape past the first star, so following that shape cracks. Two exploration voids lie on stars of no shape. A
  Jewel of Guardian (14/31) reveals one void. Hints are shape edges, 1 + level / 20 of them, clarity 0.25 to 1. Links
  join each star to its 3 nearest and include every shape edge. Strikes: 2 + the ingredient count, at most 12.
  `Guidance(path, known)` is the recipes the path is still a prefix of. `StarSmithForgeRules` holds the numbers.
- `StarSmithBeat(interval 800, tolerance 90, round-trip cap 150)`: beats fall every interval after the start; a strike
  within tolerance + round trip (capped) of the nearest beat is clean, within twice that (at most half a beat) shaky,
  else late. Server time only. A second strike on the same beat is late.
- `StarSmithForge.Start(sky, beat, start, roundTrip, level, known)`, then `Strike(star, serverTime)` and `Seal()` or
  `Abandon()`; states drafting, sealed, cracked, abandoned. A strike is refused (and changes nothing) when not drafting, for no such star, no
  strikes left, a used star or one not linked to the last. A void cracks: the recipe the path follows decides by its
  `OnFail`; a destroy needs a risky recipe and no Guardian, else the ingredients are lost. A seal gives the longest
  completed shape's recipe (or the attunement, null, capped at Uncommon), the element mix by value and the dominant
  element, the rarity, the magnitude factor and the Guidebook's first-completion flag.
- Rarity rewards pushing your luck. `Progress` is the struck stars over the completed shape's length in permille; an
  extendable shape counts its extensions up to its cap (a 3-star shape with 3 extensions is 2,000). The score is the
  clean share times the progress, plus 250 * level / 100: Common under 300, Uncommon under 500, Rare under 800, Epic
  under 1,300, Legendary under 2,000, else Mythic. So one clean strike and a seal is at most Uncommon, a full shape all
  clean is Epic, a half-clean one at most Rare, and Mythic needs every extension of a short shape struck clean.

### Positions are a layout hint

The rules play on the graph only: stars, links and shapes. A star's `X` and `Y` (0 to 1 in the item's box) are a hint
of the layout. The client lays the stars out over the item's real 2D silhouette, from the rendered picture's alpha,
and may move a star inside the silhouette as long as the stars keep their relative order (left to right, top to
bottom) and every link stays drawable. No rule depends on positions, let alone pixels; the server never checks them
(`RulesIgnoreStarPositions`).

### Wire

Packets `C1 EE 99` to `C1 EE 9F` (`docs/Packets`, generated from `BabylonPackets.xml`); multi-byte numbers are little
endian. The handlers are no-ops while Star Smithing is off (`StarSmithingFeaturePlugIn.Get` is null). The plug-in's side
is `StarSmithingFeaturePlugIn.Forge.cs`.

The client sends:
- `C1 EE 99 StarSmithForgeStart`, 21 bytes: 4 target slot, 5 ingredient count, 6 catalyst count, 7-16 ten ingredient
  slots, 17-20 four catalyst slots (0xFF unused). Checked in this order: an attempt already running (12), then
  `StarSmithRules.Check` for the new action `Forge` (5): hammer, durability, place, item, lock; then the slots (13: empty,
  named twice, the hammer or the item); then at least one ingredient (14 `NoIngredients`; catalysts never count as one). Ingredients no recipe takes still start, the attunement below; 11 `NoRecipe` is no longer sent. Each ingredient slot
  is one unit of its item (a stack gives one); a Jewel of Guardian (14/31) among the catalysts reveals a void and
  protects the item. On a start the ingredients, the catalysts (one each, like a dye: off the stack, or the item when it
  was the last) and the hammer's durability are spent at once, so leaving or losing the connection never keeps them.
- `C1 EE 9A StarSmithForgeStrike`, 5 bytes: 4 the star's index. The server stamps the time on arrival
  (`Environment.TickCount64`).
- `C1 EE 9B StarSmithForgeSeal`, 4 bytes.
- `C1 EE 9C StarSmithForgeAbandon`, 4 bytes: like a crack off every shape (`StarSmithForge.Abandon`, state
  `Abandoned`): the ingredients are lost, the item stays.

The client receives:
- `C2 EE 9D StarSmithForgeSky`: 5 result (0 started, else the EE 97 codes and 11-13 above; then only the header is
  filled), 6 target slot, 7 strikes, 8 risk (`StarSmithSky.Risk`: the worst fate among the sky's recipes after
  protection, 0 ingredients lost, 1 downgraded, 2 destroyed), 9 star count, 10-11 beat interval ms, 12-13 ms to the
  first beat, 14 entry count, then 5-byte entries (kind, A, B, C, D): the stars in index order (0, index, X, Y as 0 to
  255, 1 when it is the void a Guardian reveals), the links (1, a, b, 0, 0), the hints (2, from, to, clarity 0 to 255, 0).
  At most 255 entries. `StarSmithForgeSkyMessage.From` builds it field by field from the sky.
- `C1 EE 9E StarSmithForgeStrikeResult`, 12 + 2 per guidance recipe: 4 answer (0 linked, 1 cracked, 2 no attempt
  running, 3 no such star, 4 no strikes left, 5 the star is struck, 6 not linked to the last), 5 star, 6 element (0
  fire to 4 light), 7 value, 8 flags (1 void), 9 timing (0 clean, 1 shaky, 2 late), 10 strikes left, 11 guidance count,
  then the recipe numbers (2 bytes each, at most 60). A refused strike shows nothing of the star and no guidance.
- `C1 EE 9F StarSmithForgeResult`, 19 bytes: 4 outcome (0 sealed, 1 cracked, 2 abandoned, 3 timed out, 4 seal or abandon
  refused: no attempt or nothing struck, then nothing else is filled), 5 flags (1 the attunement, 2 the first
  completion), 6-7 recipe (on a crack the one the path was on; 0 the attunement), 8 rarity, 9 extension, 10 dominant
  element (0xFF none), 11 fate (0 ingredients lost, 1 downgraded, 2 destroyed; 0xFF when sealed), 12-16 the element mix
  (fire, frost, storm, shadow, light), 17 clean strikes, 18 strikes. Then EE 91 and, for a changed item, its
  customisation as today (EE 39).

Applying: a seal writes the rarity into the item's `ItemCustomization` (`SaveAsync`: a new record when there is none,
`Version` up, `ItemCustomizationCache.Forget`) and shows EE 92 (action 5, `Forge`) to everyone who sees the smith. A
crack applies the fate: a downgrade clears the customisation (name, lore, look, rarity), a destroy removes and deletes
the item. Then the smith's experience and Guidebook (below). PR 6: a seal also rolls the recipe's outcome and stores its
boosts and the smith's level on the record, and a downgrade clears them ("Outcomes and boosts" below).

Never sent before a star is struck: its element, its value and whether it is a void (except the revealed one), nor
which recipes the sky holds beyond the smith's known ones. `StarSmithSky.Stars` holds all of them, so the packet is
built field by field, never by copying a star (`TheSkyHidesUnstruckStarsAsync` compares the bytes of skies that differ
only there).

Every Star Smithing packet writer clears its buffer before it writes: the connection's output memory is pooled, and a
field left unwritten (like the spare last byte of `C1 EE 97 StarSmithResult`) would otherwise carry old bytes.

Decided for the wire step:
- Attempt state: `StarSmithForgeAttempt` on `Player.StarSmithForge`: the seed (a cryptographic random 64-bit number,
  never sent), every input of the deal, the forge, the target's slot and persistent id, and each accepted strike with
  its server time. `Replay()` deals the sky again and plays the strikes and the ending again to verify the attempt. One
  attempt per player; it lives in memory only.
- Round trip: a client claim is never trusted. Since PR 6 the server measures it with its own beat ping (EE F3/F4,
  `StarSmithRoundTrip`, the lowest of the last 3 samples, 0 with none) and a start uses it, capped by `RoundTripCapMs`.
  The beat settings are on `StarSmithingConfiguration` (800, 90, cap 150).
- Timeout: an attempt with no strike for `ForgeIdleTimeoutSeconds` (60) ends as an abandon (outcome 3), the safe choice:
  an idle seal could hand out a result nobody played for, an idle crack could destroy an item.
- A start needs at least one ingredient (14 `NoIngredients`, appended so the other codes keep their numbers; jewels are catalysts, never ingredients). With ingredients and no fitting recipe the attempt starts anyway: a sky with no recipe shape (only the exploration voids), and a seal attunes ("Attunement" below).

### Recipes

`StarSmithingConfiguration.Recipes` defaults to `DefaultRecipes()`: the design's five weapon shapes (the demo
Guidebook of Hidden-Babylon `src/modules/star_smithing/forge.ts`) and, since PR 6, two for armour, two for wings, one
for pendants and one for rings. Each has a `Name`, a hint for its sealed row, the smith `Experience` it gives and an
outcome table (boosts by tier, "Outcomes and boosts" below):

| # | Name | Item | Stars | Match | On fail | Exp | Hint | Boosts |
|---|---|---|---|---|---|---|---|---|
| 1 | The Serpent | weapon | 0, 1, 2, 3, 4, 5, 6 | exact | downgraded | 120 | Seven scales, from the point down to the hand. | damage, crit, excellent, speed |
| 2 | Twin Fangs | weapon | 9, 6, 10 | extendable | lost | 40 | Two fangs bite across the guard. | crit, damage, excellent |
| 3 | Ember Ring | weapon | 6, 7, 8 | extendable | lost | 40 | A ring of embers wraps the grip. | speed, damage, mana |
| 4 | Storm Spine | weapon | 6, 4, 2, 0 | exact | destroyed (risky) | 60 | Lightning climbs the blade in leaps. | damage, speed, crit |
| 5 | Wyrm Coil | weapon | 1, 11, 3, 5 | exact | lost | 60 | Four stars knot themselves around the blade. | excellent, damage, crit |
| 6 | Aegis Weave | armour | 3, 4, 8, 7 | exact | lost | 50 | Four plates lock over the heart. | defense, HP, absorb |
| 7 | Bulwark Knot | armour | 2, 9, 6 | extendable | lost | 40 | A knot of three holds where the blows land. | absorb, defense, HP |
| 8 | Featherfall | wings | 1, 10, 4, 7 | exact | lost | 60 | Four feathers drift and never touch the ground. | HP, absorb, damage |
| 9 | Tempest Pinions | wings | 11, 3, 9, 5, 8 | exact | lost | 70 | ingredient: a Season 6 skill orb (12/7) | speed, damage, mana |
| 10 | Heart of Embers | pendant | 6, 2, 10 | extendable | lost | 40 | none | mana, damage, excellent |
| 11 | Circle of Tides | ring | 0, 4, 8 | extendable | lost | 40 | Three tides meet and never end. | HP, mana, crit |

All have a 60% base chance (95% at most) and take the same family: any mix of the Season 6 skill orbs (12/7-48) and
scrolls (15/0-14), up to ten of each. One family puts every shape of an item kind in one sky, so exploring off the
known shapes finds the hidden ones; the target rule (`StarSmithTargetKind`: weapon, armour piece, wings, pendant, ring,
a minimum level, excellent or not) keeps the kinds apart. A new database gets them with the plug-in's default
configuration; an existing one through `AddStarSmithRecipesUpdatePlugIn` (PR 5: fills an empty list once) and
`AddStarSmithItemRecipesUpdatePlugIn` (PR 6: adds every default number the list lacks and gives a stored default recipe
without outcomes its default table; an operator's recipe on a number is never overwritten).

A recipe's own `Catalysts` add their chance when they are among the attempt's catalysts; the balance's chance bonus
(`StarSmithForgeRules.LevelChance`, set from `Balance.Chance` when the configuration is checked) replaces the fixed
+0.15% a level.

### Outcomes and boosts

- `StarSmithOutcomes.Roll(recipe, rarity, factor, seed, balance)`: one outcome by weight among those whose tier the
  forge's rarity reaches (none: the lowest tier), its own dice from the attempt's seed (salted), each boost rolled in its
  range, times the factor (`1 + extension * ExtensionPercent / 100` times `Balance.LevelFactor(level)`), clamped to the
  PvE per-item cap (a stat without a PvP value: the lower cap), and only the tier's `BoostSlots`.
- The record: `ItemCustomization.Boosts` ("stat:value" x100, `StarSmithBoosts`) and `SmithLevel`, migration
  `AddStarSmithBoosts`.
- In play: `StarSmithBoostBook` (item id to boosts, filled when a player first enters a map with Star Smithing on and on
  every seal or downgrade, which also rebuilds a worn item's power-ups) and `StarSmithItemPowerUpFactory`, the decorator
  `GameContext` puts over `ItemPowerUpFactory` (no boost while the feature is off). Each item's value goes into a pool
  attribute per stat and context; one capped element per pool feeds the real attribute, so the total cap holds whatever
  item came first and is recounted when one comes off. `StarSmithBalance.Ceiling` stops any cap. PvP damage and absorb
  are two new attributes (`Stats.StarSmithDamageIncreasePvp`, `StarSmithDamageReceiveDecrementPvp`) applied in the PvP
  branch of `AttackableExtensions`.
- The tooltip gets them through EE 36 (`ItemCustomizationDetail`, 451 bytes); EE 34 sets flag 32 when an item has any.

### Attunement

Experimenting is never a dead end (design section 3). A seal that completes no shape, and every seal of an attempt whose
ingredients fit no recipe (its sky then has no shape, no recipe void and no hint), is the attunement: no recipe, at
most Uncommon, `AttunementExperience` (10), nothing in the Guidebook, and one small boost from the ingredients' family
(`StarSmithOutcomes.AttunementStat`: the group most ingredients belong to; skill orbs damage 0.5-1%, scrolls max HP
30-60, anything else defense 0.5-1%), rolled by `StarSmithOutcomes.Attunement` and scaled by the smith level and capped
like any outcome. A void still cracks it: the ingredients are lost, the item stays.

### Configuration and the bridge

`StarSmithConfigValidator.Validate` is the one validator (contract above); `Sanitize` runs when the plug-in gets a
configuration and drops only what would break the game (a recipe with a bad number, chance, catalyst, fate, shape or
outcome; a bad balance falls back to the default). `StarSmithConfigJson` reads and writes the bridge's camelCase
shape and names each field it cannot read. `StarSmithingOperations.ConfigAsync` / `SetConfigAsync` are the site
operations `smithing.config` and `smithing.setConfig`; a refusal is an `AdminOperationException` with `Fields`, which
the site bridge returns as `fields`.

### Guidebook

Every recipe is hidden until the smith makes it once. The Guidebook is one column on the character,
`Character.StarSmithGuidebook` (text, migration `AddStarSmithGuidebook`): "number:made:seen" entries joined by ';',
sorted by number (`StarSmithGuidebook.Parse`, `Seen` and `Record`); "seen" is the bit mask of the outcomes the smith
rolled and is left out while 0, so a PR 5 "number:made" pair still reads. Its keys are the known set a start feeds the
forge, so guidance shows only known recipes and a first completion is a recipe not in it. A seal of a recipe records it
(new at 1, else one more) and the outcome it rolled.

`C2 EE 90 StarSmithGuidebook` goes out with EE 91 while the hammer is in a hand, so also after every ended attempt. PR 6
made it v2 (contract above): 75-byte entries, a found row with its shape length followed by its ingredient lines and
its boost lines (per stat the range of the outcomes seen), a sealed row with only its hint kind and hint (a riddle's
text or an ingredient's item). The found rows come first, then a sealed row for every recipe not found, each by
number. Nothing names an unfound recipe. A stored recipe the world no longer has is left out. The writer stops before a
row that would pass the C2 limit.

### Experience

An ended attempt gives the smith experience (design section 3, step 6; `StarSmithLevels.Add` raises the level): a seal
the recipe's `Experience`, doubled on the first completion, or 10 for the attunement; a crack 30% of the recipe the
path was on; an abandon or a timeout nothing. EE 91 shows the new level and experience. The level curve is the
balance's (`StarSmithLevels.ToNext(level, Balance.LevelCurve)`).

Tests: `tests/MUnique.OpenMU.Tests/Babylon/StarSmithRecipesTest.cs`, `StarSmithForgeTest.cs`, `StarSmithForgeWireTest.cs`,
`StarSmithGuidebookTest.cs`, `StarSmithOutcomesTest.cs`, `StarSmithBoostPowerUpTest.cs`, `StarSmithConfigValidatorTest.cs`,
`StarSmithRoundTripTest.cs`; `tests/MUnique.OpenMU.Persistence.Initialization.Tests/StarSmithRecipesDataTest.cs`;
`tests/MUnique.OpenMU.Web.Tests/Site/StarSmithingOperationsTests.cs`.

## PR 6 contract

The contract of PR 6 (Hidden-Babylon `docs/star_smithing.md` section 11), written before the code so the client and the
site can build against it. Multi-byte numbers are little endian unless named big endian. A boost value on the wire is
always the stat's value times 100, as a signed 32-bit integer (6.25% is 625, +300 HP is 30000).

### Boost stats

| Byte | JSON name | Unit | PvE attribute | PvP attribute |
|---|---|---|---|---|
| 0 | `damage` | % | `FinalDamageIncreasePvm` | `StarSmithDamageIncreasePvp` (new) |
| 1 | `defense` | % | `DefensePvm` (multiplied) | `DefensePvp` (multiplied) |
| 2 | `maxHealth` | flat | `MaximumHealth` | the same |
| 3 | `maxMana` | flat | `MaximumMana` | the same |
| 4 | `attackSpeed` | flat | `AttackSpeed` | the same |
| 5 | `criticalRate` | % | `CriticalDamageChance` | the same |
| 6 | `excellentRate` | % | `ExcellentDamageChance` | the same |
| 7 | `damageAbsorbed` | % | `FinalDamageReceiveDecrementPvm` | `StarSmithDamageReceiveDecrementPvp` (new) |

A stat that cannot tell a player from a monster (2 to 6) is capped by the lower of its PvE and PvP caps, so PvP never
sees more than its table.

### Outcomes

- A recipe has 1 to 16 outcomes `{weight, tier, boosts}`; an outcome has 0 to 3 boosts `{stat, min, max}`.
- A seal rolls one outcome by weight among those whose tier is at most the forge's rarity (none: the lowest tier
  outcome). The roll uses the attempt's seed, so `Replay()` gives the same result.
- Each boost of the outcome is rolled in `[min, max]`, multiplied by the seal's `MagnitudeFactor` (extension and smith
  level: `1 + factor * (L / 100)^exponent`, balance `scaling`), then clamped to the PvE per-item cap. Only the first
  `boostSlots` of the forge's rarity tier are kept.
- The item's record keeps the forge's rarity (as in PR 5), the boosts, and the smith level at forging. A new seal
  replaces the boosts. A downgrade clears them. The attunement (no recipe) gives no boost.
- The Guidebook records which outcomes a smith has seen: `number:made:seen`, `seen` a bit mask of outcome indices
  (a PR 5 pair `number:made` reads as seen 0).

### Boosts in play

A decorator over `IItemPowerUpFactory` adds a worn item's boosts. Each item's value is clamped to its per-item cap (PvE
and PvP separately) and goes into a per-player pool attribute per stat and context; one capped element per pool feeds
the real attribute, so the total across all worn items never passes the total cap, whichever item came first. Caps are
clamped again to the code ceilings, whatever the configuration says. Two new attributes and one line in the PvP damage
path carry the PvP damage and absorb.

### Item record and EE 36

`ItemCustomization` gains `SmithLevel` (short, 0 none) and `Boosts` (text, `stat:value` pairs joined by `;`, value x100,
at most 3), migration `AddStarSmithBoosts`.

`C2 EE 34 ItemCustomizations`: flag 32 = boosts waiting; the client asks `EE 35` for them as for the lore (flag 4).

`C2 EE 36 ItemCustomizationDetail` grows from 433 to 451 bytes:

| Index | Size | Field |
|---|---|---|
| 0-12 | | as before (header, storage, objectId big endian, slot, version) |
| 13 | 420 | lore, as before |
| 433 | 2 | smith level at forging (0 not forged) |
| 435 | 1 | boost count (0 to 3) |
| 436 | 3 x 5 | boosts: stat (1 byte, table above), value (int32, x100); unused ones zero |

The value is the stored one (PvE per-item capped); the client shows it in the rarity colour under the name and lore.

### EE 90 v2

`C2 EE 90 StarSmithGuidebook`: 5 the recipe count (for "? of N found"), 6-7 the entry count (u16), then 75-byte entries.
Each entry starts with its kind; a found row is followed by its lines (its ingredient lines, then its boost lines).

| Kind 0 found / 1 sealed | Size | Field |
|---|---|---|
| 0 | 1 | kind |
| 1 | 2 | recipe number (0 sealed) |
| 3 | 2 | made count (0 sealed) |
| 5 | 1 | shape length in stars (0 sealed) |
| 6 | 1 | hint kind: 0 none, 1 riddle, 2 ingredient (found rows send 0) |
| 7 | 1 | hint item group (ingredient hint; else 0) |
| 8 | 2 | hint item number (ingredient hint; else 0) |
| 10 | 1 | lines that follow (found: ingredient lines + boost lines; sealed 0) |
| 11 | 64 | text, UTF-8 zero padded: a found recipe's name, a riddle's text, else empty |

| Kind 2 ingredient line | Size | Field |
|---|---|---|
| 0 | 1 | kind 2 |
| 1 | 1 | count in this line, 1 to 12 |
| 2 | 12 x 5 | ingredients: group (1), number (2), minimum (1), maximum (1); the rest zero |

| Kind 3 boost line | Size | Field |
|---|---|---|
| 0 | 1 | kind 3 |
| 1 | 1 | count in this line, 1 to 8 |
| 2 | 8 x 9 | boosts seen: stat (1), min (int32, x100), max (int32, x100): per stat the range over the outcomes the smith has seen, unscaled; the rest zero |

Found rows come first by number, each followed by its lines; then a sealed row for each recipe not found, by number. A
sealed row carries only its hint kind and the hint (text or item): no number, name, shape length, ingredients or
boosts. The ingredient hint names one of the recipe's ingredients; the client writes "a recipe uses <item name>".

### Beat ping

| Packet | Direction | Layout |
|---|---|---|
| `C1 EE F3 ForkPing` | server to client | 8 bytes: 4-7 token (u32) |
| `C1 EE F4 ForkPong` | client to server | 8 bytes: 4-7 the token of the ping it answers |

The client answers a ping at once and sends nothing else. The server sends one with every EE 91 while Star Smithing is
on, at most one outstanding and at most one per 2 seconds; a pong with another token or after 5 seconds is ignored. The
round trip is the server's own clock from send to answer; the smith's round trip is the lowest of the last 3 samples
(0 with none), and a forge start uses it (`StarSmithBeat.Window`, capped by `RoundTripCapMs`). The client never states
a time.

### Configuration JSON

`StarSmithingConfiguration` gains `Balance`, and each recipe `HintKind`, `HintGroup`, `HintNumber`, `Catalysts` and
`Outcomes`. The bridge speaks camelCase, enums as camelCase strings:

```json
{
  "recipes": [{
    "number": 6, "name": "Aegis Weave", "experience": 50,
    "targets": ["armor"], "minimumItemLevel": 0, "excellent": null,
    "ingredients": [{ "group": 12, "number": 7, "minimum": 0, "maximum": 10 }],
    "baseChance": 6000, "maximumChance": 9500,
    "catalysts": [{ "group": 14, "number": 13, "chance": 500 }],
    "onFail": "ingredientsLost", "risky": false,
    "shape": { "stars": [3, 4, 8, 7], "ordered": true, "match": "exact", "extensionPercent": 10, "maximumExtension": 3 },
    "hint": { "kind": "riddle", "text": "Four plates lock over the heart.", "group": 0, "number": 0 },
    "outcomes": [{ "weight": 60, "tier": "common", "boosts": [{ "stat": "defense", "min": 1, "max": 3 }] }]
  }],
  "balance": {
    "tiers": [{ "name": "Common", "color": "#ffffff", "effectSlots": 0, "boostSlots": 1 }],
    "levelCurve": { "base": 100, "exponent": 1.6 },
    "scaling": { "factor": 0.5, "exponent": 0.8 },
    "chance": { "perLevel": 15, "maximum": 1500 },
    "caps": {
      "pve": { "damage": { "perItem": 6, "total": 15 } },
      "pvp": { "damage": { "perItem": 3, "total": 8 } }
    }
  }
}
```

- `targets`: any of `weapon`, `armor`, `wings`, `pendant`, `ring`. `excellent`: true, false or null (either).
- `onFail`: `ingredientsLost`, `itemDowngraded`, `itemDestroyed`. `match`: `exact`, `extendable`.
- `hint.kind`: `none`, `riddle` (needs `text`), `ingredient` (needs `group` and `number` of one of the ingredients).
- `tier`: `common`, `uncommon`, `rare`, `epic`, `legendary`, `mythic`. Chances in hundredths of a percent.
- Boost `min`/`max` and the caps in the stat's unit (table above), decimals allowed, two places kept.
- `caps` names every stat in both tables. Defaults (PvE per item / total, PvP per item / total, code ceiling):
  damage 6/15, 3/8, 30; defense 6/15, 3/8, 30; maxHealth 300/800, 300/800, 3000; maxMana 300/800, 300/800, 3000;
  attackSpeed 5/12, 5/12, 30; criticalRate 3/8, 2/5, 20; excellentRate 3/8, 2/5, 20; damageAbsorbed 4/10, 2/6, 25.
- `tiers`: exactly 6, Common to Mythic; defaults white 0/1, green 0/2, blue 1/2, purple 1/3, orange 2/3, red 2/3.

Validation (`StarSmithConfigValidator`, run at load and on every bridge write; at load a bad recipe is dropped and
logged): at most 64 recipes, numbers 1 to 32767 and unique; name 1 to 64 UTF-8 bytes; targets not empty; minimum item
level 0 to 15; 1 to 40 ingredients, each an existing item, none twice, minimum at most maximum at most 10; base chance 0
to 10,000, maximum chance from the base to 10,000; at most 4 catalysts, each an existing item, chance 0 to 10,000;
`itemDestroyed` only on a risky recipe; shape 2 to 12 distinct stars, each below the forge's star count, extension 0 to
100 %, at most 6 extension links; 1 to 16 outcomes, weight 1 to 10,000, at most 3 boosts, no stat twice, 0 < min <= max
<= the stat's ceiling; experience 0 to 100,000; hint text at most 64 UTF-8 bytes. Balance: 6 tiers, name 1 to 16
characters, colour `#rrggbb`, effect slots 0 to 2, boost slots 0 to 3; level curve base 1 to 100,000, exponent 1 to 3;
scaling factor 0 to 1, exponent 0.1 to 2; chance per level 0 to 100, maximum 0 to 3,000; each cap 0 <= per item <=
total <= ceiling.

### Bridge

The design's `GET`/`POST admin/star-smithing/config` are site operations like `smithing.dyes`/`smithing.setDyes`
(fork #32), called through the site's `POST admin/cfg/read` and `POST admin/cfg/write` with `{op, args, role}`:

- `smithing.config` (read, role Viewer; site flag `smithing.view`), no arguments. Result:
  `{ "config": {recipes, balance}, "ceilings": {stat: number}, "stats": [...], "items": [{group, number, name}] }`,
  `items` naming every ingredient, catalyst and hint item the config uses.
- `smithing.setConfig` (write, role Operator; site flag `smithing.edit`), arguments `config` (the JSON above as a
  string; a missing `recipes` or `balance` keeps the current one), `apply`, `confirmationToken`. A preview answers
  `{ "change": {kind: "config", recipesBefore, recipesAfter, balanceChanged}, "confirmationToken": ... }` as `setDyes`
  does; the apply writes the plugin configuration, every other setting kept, and the game reads it at once.
- A refusal is HTTP 400 `{ "ok": false, "error": "invalid", "message": "3 fields are wrong.", "fields": [{ "field":
  "recipes[2].outcomes[0].boosts[1].max", "message": "At most 30 (the code ceiling)." }] }`. `field` is the JSON path
  in the camelCase shape above; the site marks those inputs.

### Default recipes

The five weapon recipes get outcome tables. New, all with the skill orb and scroll family, a 60% base chance (95% at
most) and hints: armour 6 Aegis Weave, 7 Bulwark Knot; wings 8 Featherfall, 9 Tempest Pinions; pendant 10 Heart of
Embers; ring 11 Circle of Tides. `AddStarSmithItemRecipesUpdatePlugIn` adds every default number a database lacks and
gives a stored default recipe without outcomes its default table; it never changes anything else.

## PR 7a contract

The contract of PR 7a (Hidden-Babylon `docs/star_smithing.md` sections 12 to 14 and 16, catalogue
`docs/star_constellations.md`), written before the code so the client and the site can build against it. It replaces
the sky of 12 dealt stars, links, hints and voids with free nodes, shape matching, fault lines and layers. Multi-byte
numbers are little endian unless named big endian.

### The box

Every point is in the item's picture box: x from 0 (left) to 65535 (right), y from 0 (top) to 65535 (bottom), as an
unsigned 16-bit number ("box units"). The server reads a point as `(x / 65535, y / 65535)` and measures in that unit
square, so the client draws the picture in a square box (letterboxed when the picture is not square); otherwise
angles are not true.

### A constellation's shape

A constellation (a recipe with a geometry) is drawn as a chain of **placements**, one node per beat:

| Field | Meaning |
|---|---|
| `nodes` (n) | the corners of the shape, 3 to 12 |
| `closed` | the last corner joins the first: the figure needs one more placement, back on its first node, so it has n + 1 placements |
| `turns` | the turn at each inner corner, signed degrees, left positive: n - 2 for an open shape (at nodes 1 to n - 2), n - 1 for a closed one (at nodes 1 to n - 1) |
| `lengths` | each segment's length over the first: n - 1 for an open shape, n for a closed one; the first is always 1 |
| `extension` | optional: one more placement at its own turn (from the last segment) and length (over the first); needed for Chroma |

- A **turn** at a node is the change of heading from the segment into it to the segment out of it, in degrees in
  (-180, 180]. Left is counterclockwise as seen on the screen and positive: with the heading vectors `a` and `b` in box
  coordinates (y down), `turn = -atan2(a.x * b.y - a.y * b.x, a.x * b.x + a.y * b.y)` in degrees. So heading right and
  then up the screen is +90 (90L).
- A **length ratio** is a segment's length over the figure's first segment.
- Rotation and size are free; mirroring is not (a mirrored figure has every turn's sign flipped). The drawing order is
  part of the shape: a figure is matched from its first node, forwards only. A closed shape starts at its own first
  node (no cyclic start); the default closed shapes are regular, so any corner works for them, but only drawn
  counterclockwise (their turns are left turns).
- The extension of a closed shape starts at the closing placement (the first node), its turn measured from the closing
  segment.

### Placing a node

`C1 EE 9A` carries one point. The server checks, in this order (a refused node changes nothing and spends no strike):

1. an attempt is running (else answer 2);
2. the figure is not full: after a closing placement only one more node (the extension) is taken (else 5);
3. a strike is left (else 3);
4. **closing:** the figure has at least 3 placements, is not closed, and the point is within `closeRadius` (0.04) of
   the figure's first node: the node is a closing placement, and its point becomes exactly the first node's;
5. otherwise **too near:** a point within `nodeGap` (0.03) of any node of the attempt (the figure and the banked
   figures) is refused (4);
6. the node is accepted: a strike is spent, the server stamps its clock and judges the timing on the beat as before
   (`StarSmithBeat`: clean, shaky, late; a second node on one beat is late);
7. **fault:** a point within `faultRadius` (0.025) of a fault segment cracks the attempt (answer 1);
8. the figure is matched (below), a first match is found (discovery), and the answer goes out.

A figure is the nodes since the start or the last stash. Every placement costs a strike, the closing one too: Fire is
4 placements, its Chroma 5. Strikes: 2 + the ingredient count, at most 12.

### Accuracy, score and tier

For a figure measured against a shape (P base placements: n open, n + 1 closed):

- Placements 0 and 1 have no geometry (they set the position, heading and scale). From placement 2 on, placement k
  measures the turn at node k - 1 and the length ratio of segment k (the closing placement: the turn at the last
  corner and the closing segment).
- Per measured placement, the angle error `ea = |wrap(turn - target)|` in degrees and the length error
  `el = |ratio - target| / target`, each on the curve `f(e, perfect, miss)`: 1000 when `e <= perfect`, 0 when
  `e >= miss`, else `round(1000 * (1 + cos(pi * (e - perfect) / (miss - perfect))) / 2)` (midpoint away from zero).
  Angle: perfect 2 degrees, miss 15. Length: perfect 0.02, miss 0.20. The placement's **accuracy** is
  `g = min(f(ea, 2, 15), f(el, 0.02, 0.20))`.
- A placement's **timing weight** `w`: clean 1000, shaky 500, late 0.
- The figure **matches** a shape when it has exactly P placements (or P + 1 with the shape's extension), a closed
  shape's last base placement is a closing placement, and every measured placement, the extension included, has
  `g > 0`.
- **Score** `S = (sum of g over the m measured base placements) * (sum of w over all P base placements) /
  (m * P * 1000)`, in 64-bit integers, rounded down (0 to 1000). The extension counts in neither sum.
- **Tier** (section 13's ladder; the item rarity of the same number):

  | Tier | Byte | Rarity | Score |
  |---|---|---|---|
  | Faint | 0 | Common | under 400 |
  | Hue | 1 | Uncommon | 400 to 599 |
  | Lustre | 2 | Rare | 600 to 799 |
  | Venom | 3 | Epic | 800 to 899 |
  | Virulent | 4 | Legendary | 900 and more |
  | Chroma | 5 | Mythic | Virulent, and the extension placed with `g = 1000` and clean |

  The smith's level never changes the tier: the same drawing on the same beat gives the same tier. A drawing with an
  extension that is not perfect and clean keeps its base tier (at most Virulent).
- Two shapes can only match one figure when, at every measured placement, their targets lie within twice the miss of
  each other. The validator keeps them apart: two shapes (or extended shapes) with the same placement count, whose
  target kinds share a kind, must differ at some placement by at least 30 degrees or by a length factor of at least
  1.5. Should a figure still match two, the higher score wins, then the lower number.
- A figure is a **prefix** of a longer shape when it has at least 3 placements, fewer than the shape's P, and every
  measured placement so far has `g > 0` against that shape.

### Fault lines

Each attempt deals `faultCount` (3) short segments from its seed (`StarSmithDice` on the attempt's seed, nothing drawn
before them): per fault, in this order, `x1 = m + u * (1 - 2m)`, `y1 = m + u * (1 - 2m)` (`m` = `faultMargin`, 0.1),
the angle `a = u * pi`, the length `L = min + u * (max - min)` (0.06 to 0.12); `x2 = clamp(x1 + L cos a, 0, 1)`,
`y2 = clamp(y1 + L sin a, 0, 1)`; each `u` is the next `NextDouble()`, and the ends are kept in box units
(`round(v * 65535)`). With a Jewel of Guardian (14/31) among the catalysts one fault is then revealed:
`Next(faultCount)`. Only the revealed fault is ever sent. The Guardian also turns a destroy into ingredients lost, as
before.

### Stash, seal and slots

- **Slots** by the smith's level: 1 (levels 1 to 20), 2 (21 to 49), 3 (50 to 79), 4 (80 to 100). An item holds at most
  4 layers.
- **Stash** (`EE 9B`, action 1) banks the figure: refused when no attempt runs (2), the figure matches nothing (6), its
  hull overlaps a used hull (7), or no slot is free (8). Otherwise the figure is banked with its match, tier, score and
  hull, and the next node starts a new figure.
- **Used hulls:** the item's layers and the banked figures. A hull that overlaps exactly one item layer, of the same
  constellation, and nothing else, is a **reseal** of that layer and needs no slot; any other overlap refuses.
- **Free slots:** the level's slots minus the item's layers (never below 0); each banked figure that is not a reseal
  takes one.
- **Seal** (`EE 9B`, action 0; a 4-byte PR 5/6 `EE 9B` reads as a seal) first banks the figure being drawn when it
  would be accepted as a stash; a figure that would be refused is dropped. Then every banked figure becomes a layer,
  in the order banked: a new one is appended; a reseal raises its layer's tier and takes the new hull when the new tier
  is higher, and otherwise keeps the layer as it is (`kept`). With nothing banked and at least one node placed, the
  seal is the attunement (below). With no node placed it is refused (outcome 4).
- **Crack** (a fault): every banked figure is lost. The fate is the onFail of the constellation the figure matches or
  is a prefix of (the longest, then the lowest number), else of the first banked figure, else ingredients lost; a
  destroy needs a risky recipe and no Guardian. A downgrade clears the record (name, lore, look, rarity, boosts, smith
  level) and its layers.
- **Abandon** and the idle timeout: as before; the ingredients and the banked figures are lost, the item stays.
- **After a seal** the item's rarity is the highest of the seal's own rarity (the best sealed tier, or the
  attunement's) and every layer's tier. A sealed constellation with an outcome table (recipes 1 to 11) rolls its
  outcome as in PR 6, with its tier as the rarity, and replaces the item's boosts; with several, the highest tier rolls
  (the first banked on a tie). Looks constellations leave the boosts as they are.
- **Attunement:** no layer; the boost from the ingredients' family as in #35, at most Uncommon: Hue when the mean
  timing weight of all placements is 400 or more, else Faint.
- **Experience** of a seal: each sealed figure its recipe's experience, doubled when the smith had never sealed it
  before (made 0); the attunement 10. A crack: 30% of the fate's recipe. Abandon and timeout: nothing.

### Discovery

The first time a figure matches a constellation the smith has not found, it is found: the Guidebook gets it with made 0
(`number:0`; a found row is any entry, made 0 included), and the node's answer sets flag 8 with the constellation's
number and look, for the "Constellation of {Element} found." popup. A find is kept even when the attempt later
cracks. Nothing names an unfound constellation: the guide lists known ones only, and a match is a find.

### The hull and the layer record

- The **hull** of a figure is the convex hull of its distinct placements (the extension included), in box units:
  Andrew's monotone chain over the points sorted by x then y, collinear points dropped, in order around the hull,
  starting at the point with the least x (least y on a tie). More than 8 points: drop the point whose triangle with its
  two neighbours has the least area (the first in order on a tie), until 8 remain.
- **Overlap:** two hulls overlap when no edge normal of either separates them (separating axes, integer arithmetic);
  hulls that only touch do not overlap. A two-point hull is a segment.
- `ItemCustomization.Layers` (text, migration `AddStarSmithLayers`): up to 4 entries joined by `;`, in the order
  sealed, each `number:tier:look:x1,y1,x2,y2,...` (decimal; the hull's 3 to 8 points in box units). The look is stored,
  so a layer keeps its look when the recipe changes or goes. It travels with the item like the rest of the record.

### Looks

A recipe's `look` (byte): 0 none, then the catalogue's numbers: 1 Fire, 2 Poison, 3 Frost, 4 Storm, 5 Shadow,
6 Light, 7 Blood, 8 Earth, 9 Water, 10 Wind, 11 Arcane, 12 Nature, 13 Gold, 14 Silver, 15 Obsidian, 16 Jade,
17 Crimson Steel, 18 Starlight, 19 Prism, 20 Chroma, 21 Pearl, 22 Bronze. The client draws a layer by its look and
tier: the colour family on the hull's part, from Venom the effect, at Chroma the chroma chrome. Look 0 (the boost
recipes 1 to 11) has no colour of its own.

### Default constellations

Recipe numbers 101 to 122 are the catalogue's 1 to 22 plus 100. Every default extension is one more node turning 90
degrees against the shape's last turn, at half the first length. All take the skill orb and scroll family like the
other recipes, fit every item kind, lose the ingredients on a fail, have no hint and no outcome table, and give 10
experience per base placement.

| # | Look | Shape (nodes, turns, lengths) | Extension | Exp |
|---|---|---|---|---|
| 101 | 1 Fire | 3 closed · 120L 120L · 1 : 1 : 1 | 90R 0.5 | 40 |
| 102 | 2 Poison | 4 · 60L 60R · 1 : 1 : 1 | 90L 0.5 | 40 |
| 103 | 3 Frost | 5 · 90L 90L 90L · 1 : 0.5 : 1 : 0.5 | 90R 0.5 | 50 |
| 104 | 4 Storm | 4 · 150L 150R · 1 : 1 : 1 | 90L 0.5 | 40 |
| 105 | 5 Shadow | 4 · 90R 30L · 1 : 2 : 1.5 | 90R 0.5 | 40 |
| 106 | 6 Light | 6 closed · 60L 60L 60L 60L 60L · 1 : 1 : 1 : 1 : 1 : 1 | 90R 0.5 | 70 |
| 107 | 7 Blood | 4 · 120L 150L · 1 : 0.5 : 2 | 90R 0.5 | 40 |
| 108 | 8 Earth | 3 · 120R · 1 : 2 | 90L 0.5 | 30 |
| 109 | 9 Water | 3 · 60R · 1 : 0.5 | 90L 0.5 | 30 |
| 110 | 10 Wind | 3 · 60L · 1 : 2 | 90R 0.5 | 30 |
| 111 | 11 Arcane | 4 · 90L 150R · 1 : 1.5 : 1 | 90L 0.5 | 40 |
| 112 | 12 Nature | 3 · 120L · 1 : 0.5 | 90R 0.5 | 30 |
| 113 | 13 Gold | 4 closed · 90L 90L 90L · 1 : 1 : 1 : 1 | 90R 0.5 | 50 |
| 114 | 14 Silver | 4 · 120R 150R · 1 : 1.5 : 1.5 | 90L 0.5 | 40 |
| 115 | 15 Obsidian | 4 · 120L 120L · 1 : 1.5 : 0.5 | 90R 0.5 | 40 |
| 116 | 16 Jade | 3 · 90R · 1 : 2 | 90L 0.5 | 30 |
| 117 | 17 Crimson Steel | 3 · 120L · 1 : 1.5 | 90R 0.5 | 30 |
| 118 | 18 Starlight | 3 · 30R · 1 : 1.5 | 90L 0.5 | 30 |
| 119 | 19 Prism | 4 · 60R 90L · 1 : 1.5 : 1 | 90R 0.5 | 40 |
| 120 | 20 Chroma | 4 · 90R 150R · 1 : 1.5 : 1.5 | 90L 0.5 | 40 |
| 121 | 21 Pearl | 3 · 90R · 1 : 1 | 90L 0.5 | 30 |
| 122 | 22 Bronze | 3 · 90L · 1 : 0.5 | 90R 0.5 | 30 |

Frost's fifth node lands on its first: it is placed as a closing placement and still matches the open shape (an open
shape does not care whether its last placement closes).

The boost recipes 1 to 11 keep their numbers, names, outcomes and fates and get shapes of their own, apart from the
whole catalogue (all 121 shapes and the extensions), so 7b adds its constellations without a clash:

| # | Name | Shape | Extension |
|---|---|---|---|
| 1 | The Serpent | 6 · 120L 120R 60R 30L · 1 : 1 : 1.5 : 0.5 : 2 | 90R 0.5 |
| 2 | Twin Fangs | 3 · 150L · 1 : 2 | 90R 0.5 |
| 3 | Ember Ring | 3 · 150L · 1 : 0.5 | 90R 0.5 |
| 4 | Storm Spine | 4 · 120R 150L · 1 : 2 : 1 | 90R 0.5 |
| 5 | Wyrm Coil | 4 · 120R 30L · 1 : 1.5 : 1.5 | 90R 0.5 |
| 6 | Aegis Weave | 4 · 60L 150L · 1 : 1 : 0.5 | 90R 0.5 |
| 7 | Bulwark Knot | 3 · 150R · 1 : 0.5 | 90L 0.5 |
| 8 | Featherfall | 4 · 60L 60L · 1 : 0.5 : 0.5 | 90R 0.5 |
| 9 | Tempest Pinions | 5 · 120R 60L 30R · 1 : 2 : 2 : 1.5 | 90L 0.5 |
| 10 | Heart of Embers | 3 · 60L · 1 : 0.5 | 90R 0.5 |
| 11 | Circle of Tides | 3 · 120R · 1 : 0.5 | 90L 0.5 |

`AddStarSmithConstellationsUpdatePlugIn` adds every default number a world lacks and gives a stored default recipe 1
to 11 without a geometry its default one; an operator's own recipe is never changed.

### Wire

Same sub codes as PR 5/6 (the crafting range 90-9F is full), new layouts. A layer on the wire is 37 bytes:

| Index | Size | Field |
|---|---|---|
| 0 | 2 | constellation number |
| 2 | 1 | tier, 0 to 5 |
| 3 | 1 | look |
| 4 | 1 | hull point count, 3 to 8 |
| 5 | 32 | 8 points, x then y, u16 each, in box units; unused zero |

The client sends:

- `C1 EE 99 StarSmithForgeStart`, 21 bytes: unchanged.
- `C1 EE 9A StarSmithForgeNode`, 8 bytes: 4-5 x, 6-7 y (box units). A 5-byte PR 5/6 strike is too short and ignored.
- `C1 EE 9B StarSmithForgeSeal`, 5 bytes: 4 action, 0 seal, 1 stash.
- `C1 EE 9C StarSmithForgeAbandon`, 4 bytes: unchanged.

The client receives:

- `C1 EE 9D StarSmithForgeAttempt`, 179 bytes (was the C2 sky):

  | Index | Size | Field |
  |---|---|---|
  | 4 | 1 | result: 0 started, else the EE 97 codes, 12 attempt running, 13 wrong slot, 14 no ingredients (then only 4-5 are filled) |
  | 5 | 1 | target slot |
  | 6 | 1 | strikes |
  | 7 | 1 | risk: 0 ingredients lost, 1 downgraded, 2 destroyed (the worst fate among the attempt's constellations) |
  | 8 | 1 | free slots on this item |
  | 9 | 1 | the smith's slots by level, 1 to 4 |
  | 10 | 2 | beat interval, ms |
  | 12 | 2 | ms to the first beat |
  | 14 | 1 | flags: 1 a fault is revealed |
  | 15 | 8 | the revealed fault: x1, y1, x2, y2 (box units); zero when none |
  | 23 | 2 | fault radius (box units) |
  | 25 | 2 | close radius (box units) |
  | 27 | 2 | node gap (box units) |
  | 29 | 1 | fault count |
  | 30 | 1 | the item's layer count, 0 to 4 |
  | 31 | 148 | the item's layers, 4 x 37; unused zero |

- `C1 EE 9E StarSmithForgeNodeResult`, 107 bytes, the answer to a node or a stash:

  | Index | Size | Field |
  |---|---|---|
  | 4 | 1 | kind: 0 node, 1 stash |
  | 5 | 1 | answer. Node: 0 placed, 1 cracked, 2 no attempt, 3 no strikes left, 4 too near a node, 5 figure full. Stash: 0 banked, 2 no attempt, 6 no match, 7 overlap, 8 no free slot |
  | 6 | 1 | timing of the node: 0 clean, 1 shaky, 2 late; 0xFF on a refusal and on a stash |
  | 7 | 1 | strikes left |
  | 8 | 1 | flags: 1 the node closed the figure, 2 it hit a fault, 4 the figure's hull overlaps a used hull, 8 a first find, 16 the match counts its extension, 32 the match is a reseal of the item's layer |
  | 9 | 1 | the figure's placements (0 after a stash) |
  | 10 | 1 | banked figures |
  | 11 | 2 | match: the constellation number (0 none); on a stash, the banked one |
  | 13 | 1 | the match's look |
  | 14 | 1 | the match's tier (0xFF none) |
  | 15 | 2 | the match's score, 0 to 1000 |
  | 17 | 2 | the node's measured turn, signed tenths of a degree, left positive (0 for placements 0 and 1) |
  | 19 | 2 | the node's measured length ratio, permille of the first segment (0 for placement 0) |
  | 21 | 1 | guide count, 0 to 8 |
  | 22 | 16 | guide: known constellations the figure is a prefix of, 8 x u16, by number; unused zero |
  | 38 | 36 | per placement of the figure, 12 x 3: timing (1), accuracy (u16, `g` against the match, else the first guide; 1000 for placements 0 and 1; 0xFFFF with neither) |
  | 74 | 1 | hull point count (the figure's; on a stash the banked one's) |
  | 75 | 32 | hull points, 8 x (x, y) u16 |

  A refused node or stash fills only 4-7 and 9-10. The found popup is flag 8: the number at 11, the look at 13.

- `C1 EE 9F StarSmithForgeResult`, 41 bytes:

  | Index | Size | Field |
  |---|---|---|
  | 4 | 1 | outcome: 0 sealed, 1 cracked, 2 abandoned, 3 timed out, 4 refused (nothing else filled) |
  | 5 | 1 | flags: 1 the attunement, 2 a first seal doubled experience |
  | 6 | 1 | the item's rarity after it |
  | 7 | 1 | fate: 0 ingredients lost, 1 downgraded, 2 destroyed; 0xFF sealed |
  | 8 | 1 | clean placements |
  | 9 | 1 | placements |
  | 10 | 4 | experience earned (u32) |
  | 14 | 2 | the fate's constellation (cracked; 0 none) |
  | 16 | 1 | figures: the sealed ones, or on a crack or abandon the banked ones lost |
  | 17 | 24 | sealed figures, 4 x 6: number (u16), tier, look, result (0 added, 1 raised, 2 kept), layer index on the item |

  Then EE 91, EE 90 and the item's EE 39 as before.

- `C2 EE 36 ItemCustomizationDetail` grows from 451 to 600 bytes: 451 the layer count (0 to 4), 452 the layers
  (4 x 37). `C2 EE 34` (and EE 39) set flag 64 when an item has layers; a viewer asks EE 35 for them as for the lore,
  so worn layers are drawn for everyone.
- `C2 EE 90 StarSmithGuidebook` v3: a found row's byte 5 is the shape's base placement count (P), and its lines start
  with a shape line (counted in byte 10), 75 bytes:

  | Index | Size | Field |
  |---|---|---|
  | 0 | 1 | kind 4 |
  | 1 | 1 | nodes (n) |
  | 2 | 1 | flags: 1 closed, 2 has an extension |
  | 3 | 1 | look |
  | 4 | 2 | zero |
  | 6 | 22 | turns, 11 x i16, tenths of a degree, left positive; unused zero |
  | 28 | 24 | lengths, 12 x u16, permille of the first; unused zero |
  | 52 | 2 | the extension's turn (i16 tenths) |
  | 54 | 2 | the extension's length (u16 permille) |
  | 56 | 19 | zero |

  A found constellation never sealed (made 0) is a found row with made 0. Sealed rows are unchanged; the default
  constellations have no hint (kind 0).

**PR 5/6 clients:** keeping them would mean running both Forges side by side; that is not cheap, so they are not
kept. A PR 5/6 client still recolours, names, inscribes, locks, reads the Guidebook's rows and the boosts (EE 36 grows
at its end), but cannot play the Forge: its strikes are ignored and it misreads EE 9D, 9E and 9F. Client and fork ship
together.

### Configuration JSON

A recipe's `shape` (stars) becomes `geometry`, and it gains `look`:

```json
{
  "number": 101, "name": "Fire", "look": "fire",
  "geometry": { "nodes": 3, "closed": true, "turns": [120, 120], "lengths": [1, 1, 1],
                "extension": { "turn": -90, "length": 0.5 } },
  "outcomes": []
}
```

- `look`: `none` or the camelCase names above (`crimsonSteel`). `turns` in degrees, left positive, one decimal kept;
  `lengths` two decimals kept; `extension` may be null.
- Validation: nodes 3 to 12 open, 3 to 11 closed; the turn and length counts above; each turn 10 to 170 degrees
  either way; the first length 1, each length 0.25 to 4; the extension's turn and length the same; P plus the
  extension at most 12; shapes kept apart as said above (field `recipes[i].geometry`, naming the other recipe); 0 to 16
  outcomes, at least one outcome or a look. A recipe without a geometry is dropped at load (logged) and refused by
  the bridge.
- `StarSmithingConfiguration.Forge` (not in the bridge): `FaultCount` 3 (0 to 8), `FaultMargin` 0.1,
  `FaultMinLength` 0.06, `FaultMaxLength` 0.12, `FaultRadius` 0.025, `CloseRadius` 0.04, `NodeGap` 0.03,
  `BaseStrikes` 2, `MaxStrikes` 12, the Guardian 14/31. The PR 5 sky settings (star count, exploration voids,
  neighbours, spacing) are gone.
- A recipe's chance and catalysts no longer roll anything in the Forge (the faults are the risk); they stay in the
  configuration for the editor and a later use.
