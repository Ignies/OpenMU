# Wide items

Fork feature `wide_items`. `WideItemRules` decides which items only the wide item format (W32) of the Babylon client
carries, and whether a player's client can see an item:

- `IsWideOnly(ItemDefinition)`: item group above 15, number above 511, or the option definition "Wide Item Format"
  (option type `Season21ItemOptionTypes.WideFormat`), which the update "Activate the items of the wide item format"
  gives the waiting items of the groups 0-15.
- `IsWideOnly(Item)`: the definition, or a mastery, mastery bonus or wing grade option, or a socket option above
  sphere level 5.
- `player.CanSee(item)`: the item serializer of the connection (`IItemFormatViewPlugIn.CarriesWideItems`) carries
  wide items, or the item isn't wide-only. A player without a client view (a bot) sees every item.

Who asks: the item views of the game server (lists, drops, trade, shops, appearance), the trade move, the merchant and
player shop purchase, the pick-up, the drop generator (per killer) and the admin "give item". The places and what a
Season 6 client gets instead are listed in `docs/babylon.md`, "Wide items".

## Item rules of Season 21 (`Season21ItemsFeaturePlugIn`)

Fork feature `s21_mixes`. Its configuration holds the grade rates of the wings (`WingGrades` rolls the grade options on
drops and on the fourth wings combination) and the mastery item mixes (`MasteryMixRecipe`), which
`PlayerActions/Craftings/MasteryItemMixCrafting` finds by the items in the chaos machine. `FourthWingCrafting` gives the
wings of the player's class. Data, numbers and sources: `docs/babylon.md`, "Options (W32 only)" and "Mixes".

## Jewel of Harmony on ancient items

With these item rules on, the Jewel of Harmony takes ancient (set) items too (`HarmonyOnAncientItems`, on: S21Emu
`SetItemAcceptHarmonySwitch` 1, Webzen since Season 16 Part 1-3); without them the Season 6 rule refuses them
(`HarmonyJewelConsumeHandlerPlugIn`).
