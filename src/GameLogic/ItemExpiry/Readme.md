# Item expiry

Timed items: an item whose `Item.ExpiresAt` (UTC, migration `AddSeason21Economy`) passed is removed.

- `ItemExpiryPlugIn` (off by default, the world's switch; `ItemExpiryConfiguration.Interval`, a minute) checks the
  players in the world: inventory with the equipped items, vault, both gremory cases.
- `ItemExpiryExtensions.RemoveExpiredItemsAsync` removes them, tells the inventory view and the player
  (`IItemExpiredPlugIn`, `C1 EE 31` for the Babylon client; a message for the others).
- The W32 item format carries the expiry (bytes 28-31, flag bit 7), so the Babylon client shows the time left.
- Items get an expiry from the gremory case (the claim deadline).

Not built: the period extension of Season 21 (`Item/PeriodExtendableItem.txt`), timed products of the cash shop
(their delivery still refuses a duration), the [Speed] and PC room items.
