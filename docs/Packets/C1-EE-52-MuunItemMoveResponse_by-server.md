# C1 EE 52 - MuunItemMoveResponse (by server)

## Is sent when

After a MuunItemMove.

## Causes the following actions on the client side

On success, the client shows the item at its new place; the inventory side also gets the Season 6 item removed / item added packets.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   41   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x52  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result; 0 done, 1 off, 2 no item, 3 wrong material, 4 highest level, 5 level too low, 6 no evolution, 7 not enough items, 8 not at the exchange NPC, 9 no free slot, 10 riding not allowed here, 11 no mount, 12 slot occupied, 13 not a Muun, 14 the same Muun is equipped, 15 slot locked (the hero slot before the highest level, or no hero Muun). |
| 5 | 1 | Byte |  | FromStorage; 0 the inventory, 1 the Muun inventory. |
| 6 | 1 | Byte |  | FromSlot |
| 7 | 1 | Byte |  | ToStorage; 0 the inventory, 1 the Muun inventory. |
| 8 | 1 | Byte |  | ToSlot |
| 9 | 32 | Binary |  | ItemData; The item in the W32 item format; 0xFF when there is none. |