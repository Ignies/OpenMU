# C1 EE 96 - StarSmithLockRequest (by client)

## Is sent when

The smith locks an item of the inventory (permanent for players).

## Causes the following actions on the server side

The server locks the item's customisation and answers with StarSmithResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x96  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Slot; The inventory slot of the item. |