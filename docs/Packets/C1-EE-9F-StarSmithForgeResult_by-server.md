# C1 EE 9F - StarSmithForgeResult (by server)

## Is sent when

An attempt ends (sealed, cracked, abandoned or timed out), or a seal is refused.

## Causes the following actions on the client side

The client shows the result; the item's new customisation follows as ItemCustomization (EE 39).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   41   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9F  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Outcome; 0 sealed, 1 cracked, 2 abandoned, 3 timed out (as abandoned), 4 seal refused (no attempt running or nothing placed; nothing else is filled). |
| 5 | 1 | Byte |  | Flags; 1 the attunement (nothing sealed as a layer), 2 a first seal doubled experience. |
| 6 | 1 | Byte |  | Rarity; The item's rarity after it, 0 Common to 5 Mythic. |
| 7 | 1 | Byte |  | Fate; 0 the ingredients are lost, 1 the item is downgraded, 2 the item is destroyed; 0xFF when sealed. |
| 8 | 1 | Byte |  | CleanPlacements |
| 9 | 1 | Byte |  | Placements |
| 10 | 4 | IntegerLittleEndian |  | Experience |
| 14 | 2 | ShortLittleEndian |  | FateConstellation; On a crack, the constellation whose onFail decided; 0 none. |
| 16 | 1 | Byte |  | FigureCount; The sealed figures; on a crack or an abandon the stashed ones lost. |
| 17 | 24 | Binary |  | Figures; 4 x 6 bytes: number (u16), tier, look, result (0 added, 1 raised, 2 kept), the layer's index on the item. |