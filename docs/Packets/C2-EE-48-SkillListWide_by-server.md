# C2 EE 48 - SkillListWide (by server)

## Is sent when

When the own character enters the world, to a Babylon client.

## Causes the following actions on the client side

The client shows the skills. The Season 6 list counts them in a byte of a C1 packet, so it holds at most 62; a fourth or fifth evolution with its tree skills has more.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x48  | Packet header - sub packet type identifier |
| 5 | 2 | ShortLittleEndian |  | SkillCount |
| 7 | SkillLevelWide.Length * SkillCount | Array of SkillLevelWide |  | Skills |

### SkillLevelWide Structure

A skill of the own character with its number and level.

Length: 5 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | SkillIndex; The index of the skill in the list (the Season 6 skill index, wider). |
| 2 | 2 | ShortLittleEndian |  | SkillNumber; The skill number. |
| 4 | 1 | Byte |  | Level; The level of a master or tree skill, else 0. |