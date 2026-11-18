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
