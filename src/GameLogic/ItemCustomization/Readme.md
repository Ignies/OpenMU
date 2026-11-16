# Item customisation

The look, name, lore and rarity of single items, shown to the Babylon client. It's the first part of Star Smithing
(Hidden-Babylon `docs/star_smithing.md`): the website gives customised items today (deliveries, gacha prizes, the
admin's Generate item); the Star Smith makes them later.

## The record

`ItemCustomization` (DataModel, its own table, migration `AddItemCustomization`), keyed by the item's persistent id, so
an item keeps it wherever it moves and loses it with the item (cascade delete):

| Field | |
|---|---|
| `Name` | at most 24 characters, or null for the item's own name |
| `Lore` | at most 140 characters on at most 3 lines, or null |
| `Rarity` | 0 Common, 1 Uncommon, 2 Rare, 3 Epic, 4 Legendary, 5 Mythic |
| `Look` | the site's item look JSON (`mode`, `color`, `strength`, `hue`, `keep_from`, `keep_range`, `glow`, `fx`, `fx_colors`, `fx_tune`), or null |
| `Version` | goes up on every change; the client keeps the lore by it |
| `CreatedBy`, `UpdatedAt` | who made it (an account, a delivery) and when |

Writers today are outside the game server: the Hidden plugin glue writes the record when a web delivery makes an item,
and copied the site's older `web_item_tint` rows once. Readers: `IPlayerContext.GetItemCustomizationsAsync`.

## The channel

`ItemCustomizationFeaturePlugIn` (feature `item_custom`, wire bit 25, off by default; a world turns it on with the
"Item looks and names" system). Packets (`docs/Packets/`), all fixed size:

- `EE 33 ItemCustomizationRequest`: up to 32 places `(storage, objectId)`: 0 worn by a player in view, 1 the own
  inventory, 2 the own vault, 3 the trading partner's offer, 4 the open personal shop of a player in view, 5 the own
  Gremory case. A place the player may not see gives nothing (`ItemCustomizationCollector`).
- `EE 34 ItemCustomizations`: the customised items of the asked places, 123 bytes each: the place, the slot, the
  version, flags (1 look, 2 name, 4 lore waiting, 8 aura, 16 particles), the rarity, the look (31 bytes, `ItemLookBytes`,
  byte for byte the body of the Hidden plugin's `E7 09` tint entry), the name (72 bytes UTF-8), an aura id and a
  particle layer (both zero until Star Smithing adds them).
- `EE 35 ItemCustomizationDetailRequest` / `EE 36 ItemCustomizationDetail`: the lore of one item (420 bytes UTF-8),
  asked when a tooltip opens.

One request is out at a time on the client, so an answer covers exactly the request it answers. Requests are answered
four a second per player; the rest are dropped. Records are cached five minutes, a missing one thirty seconds (a
delivery writes the record right after it makes the item).

Names and lore are cut to their fields on a character, never inside one (`ItemCustomizationText`).
