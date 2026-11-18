# C1 EE 94 - StarSmithRenameRequest (by client)

## Is sent when

The smith names an item of the inventory.

## Causes the following actions on the server side

The server checks the name with the text moderation, saves it and answers with StarSmithResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   77   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x94  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Slot; The inventory slot of the item. |
| 5 | 72 | Binary |  | Name; UTF-8, zero padded; empty removes the name. |