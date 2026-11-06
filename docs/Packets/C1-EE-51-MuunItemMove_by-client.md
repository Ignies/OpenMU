# C1 EE 51 - MuunItemMove (by client)

## Is sent when

The player moves an item between the inventory and the Muun inventory, or within the Muun inventory; slots 0 and 1 equip a Muun.

## Causes the following actions on the server side

The server moves it and answers with MuunItemMoveResponse.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x51  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | FromStorage; 0 the inventory, 1 the Muun inventory. |
| 5 | 1 | Byte |  | FromSlot |
| 6 | 1 | Byte |  | ToStorage; 0 the inventory, 1 the Muun inventory. |
| 7 | 1 | Byte |  | ToSlot |