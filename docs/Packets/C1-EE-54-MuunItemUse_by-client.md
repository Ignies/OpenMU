# C1 EE 54 - MuunItemUse (by client)

## Is sent when

The player drops an item on a Muun item: a Muun of level 1 on the same Muun (level up), an evolution stone on a Muun (evolve), a Muun or an evolution stone on a Muun energy converter (charge), a jewel of life on a Muun (restore).

## Causes the following actions on the server side

The server uses up the source item, changes the target item and answers with MuunActionResponse.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   9   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x54  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Action; 1 level up, 2 evolve, 3 charge energy, 4 restore. |
| 5 | 1 | Byte |  | SourceStorage; 0 the inventory, 1 the Muun inventory. |
| 6 | 1 | Byte |  | SourceSlot |
| 7 | 1 | Byte |  | TargetStorage; 0 the inventory, 1 the Muun inventory. |
| 8 | 1 | Byte |  | TargetSlot |