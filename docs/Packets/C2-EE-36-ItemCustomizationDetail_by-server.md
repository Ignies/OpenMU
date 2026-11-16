# C2 EE 36 - ItemCustomizationDetail (by server)

## Is sent when

The answer to ItemCustomizationDetailRequest.

## Causes the following actions on the client side

The client shows the item's lore in its tooltip.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |   433   | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x36  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Storage |
| 6 | 2 | ShortBigEndian |  | ObjectId |
| 8 | 1 | Byte |  | Slot |
| 9 | 4 | IntegerLittleEndian |  | Version |
| 13 | 420 | Binary |  | Lore; The lore, UTF-8, at most 140 characters on at most 3 lines, zero padded; empty when the item has none. |