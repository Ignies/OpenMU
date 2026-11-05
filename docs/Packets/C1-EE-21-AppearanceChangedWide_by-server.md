# C1 EE 21 - AppearanceChangedWide (by server)

## Is sent when

A player in the view of a Babylon client put on or took off a piece of gear.

## Causes the following actions on the client side

The client updates the gear of the player.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   40   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x21  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | PlayerId |
| 6 | 1 | Byte |  | Slot |
| 7 | 1 | Byte |  | ClassCode; The class code of the player, so an evolution shows too. |
| 8 | 32 | Binary |  | ItemData; The item data in the item format of the connection, zero filled to 32 bytes; all 0xFF when the slot got empty. |