# C2 EE 95 - StarSmithLoreRequest (by client)

## Is sent when

The smith writes the lore of an item of the inventory.

## Causes the following actions on the server side

The server checks the lore with the text moderation, saves it and answers with StarSmithResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |   426   | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x95  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Slot; The inventory slot of the item. |
| 6 | 420 | Binary |  | Lore; UTF-8, lines split by a line feed, zero padded; empty removes the lore. |