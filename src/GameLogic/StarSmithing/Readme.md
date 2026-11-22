# Star Smithing

Players with the Star Smith's Hammer change their own items (Hidden-Babylon `docs/star_smithing.md`). This folder is the
shell (PR 3): the hammer, the smith's level and experience, and four actions on an item of the inventory. Recipes,
the bench, effects and auras come in the later PRs.

- `StarSmithingFeaturePlugIn` (feature `star_smithing`, bit 26, off by default; `StarSmithingConfiguration`: the hammer's
  group and number, 2/1000; the durability an action takes, 5; the Zen a recolour costs, 500,000). It sends the smith's
  state, `C1 EE 91 StarSmithState` (hammer in a hand, broken, level, experience, experience to the next level, the
  hammer's durability), on every map entry and when the hammer is put on or taken off.
- Actions, each answered by `C1 EE 97 StarSmithResult` and a new EE 91: recolour (`C1 EE 93`, the 31 look bytes of
  `ItemLookBytes`; only the colours are kept, a weapon effect comes with recipes; costs Zen), name (`C1 EE 94`),
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

Tests: `tests/MUnique.OpenMU.Tests/Babylon/StarSmithRulesTest.cs`, `StarSmithLevelsTest.cs`, `StarSmithingViewTest.cs`,
`ItemLookBytesReadTest.cs`.

## Constellation Forge

Every recipe attempt is played as the Forge minigame (Hidden-Babylon `docs/star_smithing.md` section 10). This is its
pure rule core: deterministic from a seed, so the server can deal an attempt again and verify it. No packets, plug-in
wiring or database yet.

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
- `StarSmithForge.Start(sky, beat, start, roundTrip, level, known)`, then `Strike(star, serverTime)` and `Seal()`;
  states drafting, sealed, cracked. A strike is refused (and changes nothing) when not drafting, for no such star, no
  strikes left, a used star or one not linked to the last. A void cracks: the recipe the path follows decides by its
  `OnFail`; a destroy needs a risky recipe and no Guardian, else the ingredients are lost. A seal gives the longest
  completed shape's recipe (or the attunement, null, capped at Uncommon), the element mix by value and the dominant
  element, the rarity (clean share in permille + 250 * level / 100: Common under 300, Uncommon 500, Rare 700, Epic
  900, Legendary 1,100, else Mythic), the magnitude factor and the Guidebook's first-completion flag.

Tests: `tests/MUnique.OpenMU.Tests/Babylon/StarSmithRecipesTest.cs`, `StarSmithForgeTest.cs`.
