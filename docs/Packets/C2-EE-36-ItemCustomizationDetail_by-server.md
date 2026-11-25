# C2 EE 36 - ItemCustomizationDetail (by server)

## Is sent when

The answer to ItemCustomizationDetailRequest.

## Causes the following actions on the client side

The client shows the item's lore, and a smithed item's boosts and smith level, in its tooltip.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |   600   | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x36  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Storage |
| 6 | 2 | ShortBigEndian |  | ObjectId |
| 8 | 1 | Byte |  | Slot |
| 9 | 4 | IntegerLittleEndian |  | Version |
| 13 | 420 | Binary |  | Lore; The lore, UTF-8, at most 140 characters on at most 3 lines, zero padded; empty when the item has none. |
| 433 | 2 | ShortLittleEndian |  | SmithLevel; The smith's level when the item was forged; 0 when it was not (Star Smithing). |
| 435 | 1 | Byte |  | BoostCount; The smithed boosts, 0 to 3. |
| 436 | 15 | Binary |  | Boosts; Three 5-byte boosts: the stat (0 damage %, 1 defense %, 2 max HP, 3 max mana, 4 attack speed, 5 critical rate %, 6 excellent rate %, 7 damage absorbed %), then its value times 100 (32-bit, little endian); unused ones zero. |
| 451 | 1 | Byte |  | LayerCount; The item's constellation layers, 0 to 4 (Star Smithing PR 7a). |
| 452 | 148 | Binary |  | Layers; 4 layers of 37 bytes: number (u16 little endian), tier (0 Faint to 5 Chroma), look, hull point count (3 to 8), 8 points (x, y u16 little endian each, 0 to 65535 in the item's box); unused zero. |