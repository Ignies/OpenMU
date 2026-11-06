# MU Helper Plus

The MU Helper Plus of Season 21. The helper itself runs in the client (OpenMU's `MuHelper` bills it); the plus mode adds
moves into hunting zones: S21Emu `Character/MuHelperPlus.xml` has 31 hunting grounds from Debenter on, each with the warp
of `Move.txt` it belongs to, and 330 zones (a map, an element and the point where the hunt starts).

- `HelperPlusPlugIn` (off by default, the world's switch) sends the rules on every map entry and on a switch of the mode,
  keeps the plus mode per player (runtime only) and moves a character into a zone: the warp of the zone's ground gives
  the level requirement and the zen, as a move by the move list does; the character lands on the zone's hunting point.
- Rules from the guides: the plus mode needs total level 400 (`PlusMinimumTotalLevel`) and a character alone or the party
  leader (`PartyLeaderOnly`).
- No source: the ranges sent to the client (`HuntRange`, `PickupRange`, `PlusHuntRange`, `PlusPickupRange`, 8 tiles like
  the classic helper window) and the cost of the plus mode (`PlusCostPercent`, 100: the guides name no other cost).
  `MuHelperZenCostCalculator` applies the percent while the plus mode is on.
- The zones of Aquilas Temple (ground 30, warp 75) stay unreachable until the map is there: the maps update leaves it out.
- The Babylon client gets `C1 EE E0` and `C1 EE E2` and sends `C1 EE E1` and `C1 EE E3` (`docs/babylon.md`).
- The data comes with the update "Add the MU Helper Plus of Season 21" (`Persistence/Initialization/VersionSeasonSix/Season21`).
