# C1 EE 72 - RuudShopBuy (by client)

## Is sent when

The player wants to buy an item of the Ruud shop.

## Causes the following actions on the server side

The server answers with RuudShopBuyResponse.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x72  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | Index; The index of the item, as in RuudShopList. |