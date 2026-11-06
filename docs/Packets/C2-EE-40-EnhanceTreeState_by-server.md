# C2 EE 40 - EnhanceTreeState (by server)

## Is sent when

With the skill list, to a fourth or fifth evolution of a world with the skill enhancement tree.

## Causes the following actions on the client side

The client fills its skill enhancement tree window.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x40  | Packet header - sub packet type identifier |
| 5 | 2 | ShortLittleEndian |  | FreePoints; The free points. |
| 7 | 2 | ShortLittleEndian |  | SpentPoints; The points spent in the tree. |
| 9 | 2 | ShortLittleEndian |  | SkillCount |
| 11 | EnhanceTreeSkill.Length * SkillCount | Array of EnhanceTreeSkill |  | Skills |

### EnhanceTreeSkill Structure

A learned skill of the skill enhancement tree.

Length: 3 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | SkillNumber; The skill number of the node or passive. |
| 2 | 1 | Byte |  | Level; The points in it. |