# C1 EE 55 - MuunActionResponse (by server)

## Is sent when

After a MuunItemUse, a MuunExchangeRequest or a MuunRideRequest, and when an equipped Muun lost health (action 4).

## Causes the following actions on the client side

The client shows the changed or created item at its place.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   40   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x55  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Action; 1 level up, 2 evolve, 3 charge energy, 4 restore or health changed, 5 exchange, 6 ride. |
| 5 | 1 | Byte |  | Result; 0 done, 1 off, 2 no item, 3 wrong material, 4 highest level, 5 level too low, 6 no evolution, 7 not enough items, 8 not at the exchange NPC, 9 no free slot, 10 riding not allowed here, 11 no mount, 12 slot occupied, 13 not a Muun. |
| 6 | 1 | Byte |  | Storage; 0 the inventory, 1 the Muun inventory. |
| 7 | 1 | Byte |  | Slot |
| 8 | 32 | Binary |  | ItemData; The item in the W32 item format; 0xFF when there is none. |