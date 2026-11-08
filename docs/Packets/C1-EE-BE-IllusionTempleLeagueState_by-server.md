# C1 EE BE - IllusionTempleLeagueState (by server)

## Is sent when

When a character of a guild enters the world on a league day, when a league match starts and after it ended, while the plugin of the Illusion Temple League is on.

## Causes the following actions on the client side

The client shows the league of the guild: the stage of the day, the rank, the entries and the qualification.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   20   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xBE  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Stage; The stage of the day: 0 no league day, 1 eliminations, 2 semi-finals, 3 final, 4 reward day. |
| 5 | 1 | Byte |  | Rank; The rank of the guild in the eliminations of the month, 0 when it played none. |
| 6 | 1 | Byte |  | GuildEntries; The elimination matches the guild played this month. |
| 7 | 1 | Byte |  | MaximumGuildEntries; The elimination matches a guild may play, 0 for no limit. |
| 8 | 2 | ShortLittleEndian |  | Stones; The occupied stones of the guild in the eliminations. |
| 10 | 2 | ShortLittleEndian |  | Kills; The kills of the guild in the eliminations. |
| 12 | 4 | IntegerLittleEndian |  | LevelSum; The level sums of the guild in the eliminations. |
| 16 | 1 | Byte |  | Qualification; 0 none, 1 semi-final in temple 1, 2 semi-final in temple 2, 3 finalist, 4 champion. |
| 17 | 1 | Byte |  | PlayerEntries; The elimination matches the character played this month. |
| 18 | 1 | Byte |  | MaximumPlayerEntries; The elimination matches a character may play, 0 for no limit. |