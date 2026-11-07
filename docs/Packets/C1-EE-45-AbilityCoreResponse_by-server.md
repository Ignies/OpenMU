# C1 EE 45 - AbilityCoreResponse (by server)

## Is sent when

After an AbilityCoreRequest.

## Causes the following actions on the client side

The client shows the result; on success the card (or the stones of a reduction) at its new place.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   40   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x45  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result; 0 done, 1 off, 2 not a fifth evolution, 3 no item, 4 not a card (or no level up / reduction for it), 5 slot locked, 6 wrong slot (kind, row color or class), 7 slot occupied, 8 requirement missing (the fourth evolution skill, or the five option cards of the row), 9 inventory full, 10 highest level, 11 not enough cards, 12 not enough zen, 13 not enough stones, 14 elite level above the row. |
| 5 | 1 | Byte |  | Action |
| 6 | 1 | Byte |  | CoreSlot |
| 7 | 1 | Byte |  | InventorySlot |
| 8 | 32 | Binary |  | ItemData; The card after the action (the stones after a reduction) in the W32 item format; 0xFF when there is none. |