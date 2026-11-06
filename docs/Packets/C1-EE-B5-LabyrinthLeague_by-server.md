# C1 EE B5 - LabyrinthLeague (by server)

## Is sent when

When the character enters the Labyrinth of Dimensions and when its run ended, while the plugin of the labyrinth league is on.

## Causes the following actions on the client side

The client shows the tier, the rank in the group and the points of the week of the character.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   14   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB5  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Tier; The tier: 0 Bronze, 1 Silver, 2 Gold, 3 Legend. |
| 5 | 1 | Byte |  | Rank; The rank in the group of the current week, 0 when the character has no points this week. |
| 6 | 1 | Byte |  | GroupSize; The number of characters of the group. |
| 7 | 1 | Byte |  | Day; The kind of the day: 0 no league day, 1 the first day of a week, 2 a play day, 3 the reward day. |
| 8 | 4 | IntegerLittleEndian |  | Points; The points of the current week. |
| 12 | 2 | ShortLittleEndian |  | FloorsToday; The number of floors cleared today. |