# C2 EE 46 - MasterSkillListWide (by server)

## Is sent when

With the master skill list, to a Babylon client.

## Causes the following actions on the client side

The client places the master skills in its tree by their numbers; the Season 6 list identifies them by a slot of its own client tree, which the classes after Season 6 do not have.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x46  | Packet header - sub packet type identifier |
| 5 | 2 | ShortLittleEndian |  | MasterLevelUpPoints; The free master points. |
| 7 | 2 | ShortLittleEndian |  | SkillCount |
| 9 | MasterSkillWide.Length * SkillCount | Array of MasterSkillWide |  | Skills |

### MasterSkillWide Structure

A learned master skill, by its number instead of the index of the Season 6 client tree.

Length: 11 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | SkillNumber; The skill number. |
| 2 | 1 | Byte |  | Level; The level. |
| 3 | 4 | Float |  | DisplayValue; The value at the level. |
| 7 | 4 | Float |  | DisplayValueOfNextLevel; The value at the next level. |