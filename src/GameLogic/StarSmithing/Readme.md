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
<<<<<<< HEAD
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

### Wire contract (next step)

The client sends:
- start: the target slot, the ingredient and catalyst slots (the server checks them again, deals the sky and starts
  the beat on its own clock);
- strike: the star's index only; the server stamps the time on arrival;
- seal;
- abandon (proposed: like a crack off every shape, the ingredients are lost and the item stays; the forge has no
  `Abandon` yet).

The client receives:
- on start: per star its index and layout hint (X, Y) and, for the one void a Jewel of Guardian reveals, that it is a
  void; the links; the hints (pair and clarity); the strikes allowed; the beat interval and the time to the first beat;
  the risk (the worst onFail among the sky's recipes, after protection);
- on each strike: the answer (linked, cracked or why refused), the struck star's element, value and void flag, the
  timing (clean, shaky, late), the strikes left and the guidance (the known recipe numbers the path is still a prefix
  of);
- on seal or crack: the result (recipe number or attunement, element mix, rarity, extension, first completion, the
  item's fate on a crack), then the item's new customisation as today (EE 39).

Never sent before a star is struck: its element, its value and whether it is a void (except the revealed one), nor
which recipes the sky holds beyond the smith's known ones. `StarSmithSky.Stars` holds all of them, so the packet must
be built field by field, never by copying a star.

Open for the wire step: where the round trip comes from (the server's own ping of the connection, not a client
claim), the attempt's timeout (an idle attempt seals or cracks), and how the seed is chosen and stored with the
attempt's strikes so it can be replayed and verified.

Tests: `tests/MUnique.OpenMU.Tests/Babylon/StarSmithRecipesTest.cs`, `StarSmithForgeTest.cs`.
=======
`ItemLookBytesReadTest.cs`, `StarSmithDyesTest.cs`; `tests/MUnique.OpenMU.Persistence.Initialization.Tests/StarSmithDyesDataTest.cs`;
`tests/MUnique.OpenMU.Web.Tests/Site/StarSmithingOperationsTests.cs`.
>>>>>>> origin/master
