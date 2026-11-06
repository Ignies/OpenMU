# C1 EE 42 - EnhanceTreeAddPointResult (by server)

## Is sent when

After the player asked to add a point to the skill enhancement tree.

## Causes the following actions on the client side

The client shows the new level and the free points, or why the point was refused.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   10   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x42  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result; 0 added, 1 no tree, 2 unknown node, 3 requirement missing, 4 not enough points, 5 maximum reached. |
| 5 | 2 | ShortLittleEndian |  | SkillNumber; The skill number of the node. |
| 7 | 1 | Byte |  | Level; The level of the node after the request. |
| 8 | 2 | ShortLittleEndian |  | FreePoints; The free points after the request. |