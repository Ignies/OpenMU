# C1 EE 44 - AbilityCoreRequest (by client)

## Is sent when

The player puts a card into the core, takes one out, levels one up or reduces one into stones.

## Causes the following actions on the server side

The server checks and does it, and answers with AbilityCoreResponse; after a success also with AbilityCoreState and the inventory packets.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x44  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Action; 0 insert (inventory slot into core slot), 1 remove (core slot into inventory slot, 0xFF the first free one), 2 level up (the card of the inventory slot, or of the core slot when it is 0xFF), 3 reduce (the card of the inventory slot). |
| 5 | 1 | Byte |  | CoreSlot |
| 6 | 1 | Byte |  | InventorySlot |