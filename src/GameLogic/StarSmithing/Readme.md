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
