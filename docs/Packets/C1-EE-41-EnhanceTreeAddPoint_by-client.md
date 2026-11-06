# C1 EE 41 - EnhanceTreeAddPoint (by client)

## Is sent when

The player adds a point to a node or passive of the skill enhancement tree.

## Causes the following actions on the server side

The server checks the node and the points and answers with EnhanceTreeAddPointResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x41  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | SkillNumber; The skill number of the node or passive. |