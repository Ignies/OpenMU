# C1 EE 35 - ItemCustomizationDetailRequest (by client)

## Is sent when

A tooltip opens for an item whose customisation says its lore is waiting, and the client has no lore at that version.

## Causes the following actions on the server side

The server answers with ItemCustomizationDetail.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x35  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Storage |
| 5 | 2 | ShortBigEndian |  | ObjectId |
| 7 | 1 | Byte |  | Slot |