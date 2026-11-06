# C1 EE 73 - RuudShopBuyResponse (by server)

## Is sent when

After a purchase in the Ruud shop.

## Causes the following actions on the client side

The client shows the result; the item appears in the inventory by the usual packet.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   11   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x73  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result; 0 bought, 1 not enough Ruud, 2 inventory full, 3 no such item for the class, 4 the shop is closed. |
| 5 | 2 | ShortLittleEndian |  | Index |
| 7 | 4 | IntegerLittleEndian |  | Ruud; The Ruud of the character after the purchase. |