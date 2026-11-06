# C2 EE 71 - RuudShopList (by server)

## Is sent when

The player talked to the NPC of the Ruud shop (Priest James).

## Causes the following actions on the client side

The client opens the Ruud shop with the items which the class of the character may buy.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x71  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | ItemCount |
| 6 | RuudShopItem.Length * ItemCount | Array of RuudShopItem |  | Items |

### RuudShopItem Structure

An item of the Ruud shop.

Length: 12 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | Index; The index of the item in the shop, which a purchase refers to. |
| 2 | 1 | Byte |  | ItemGroup |
| 3 | 2 | ShortLittleEndian |  | ItemNumber |
| 5 | 1 | Byte |  | ItemLevel |
| 6 | 1 | Byte |  | Count; The count (durability) of the item; 0 for the durability of its definition. |
| 7 | 1 | Byte |  | Flags; Bit 0: the item may get luck; bit 1: the item may get the additional option. |
| 8 | 4 | IntegerLittleEndian |  | Price; The price in Ruud. |