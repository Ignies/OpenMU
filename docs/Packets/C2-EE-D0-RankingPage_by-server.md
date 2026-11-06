# C2 EE D0 - RankingPage (by server)

## Is sent when

After a RankingRequest.

## Causes the following actions on the client side

The client shows the page of the ranking.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xD0  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Kind; 0 total level, 1 monster kills of the week, 2 hunting experience of the week. |
| 6 | 1 | Byte |  | Page; The page, from 0. |
| 7 | 1 | Byte |  | PageCount |
| 8 | 2 | ShortLittleEndian |  | OwnRank; The rank of the own character, 0 when it is not ranked. |
| 10 | 1 | Byte |  | EntryCount |
| 11 | RankingEntry.Length * EntryCount | Array of RankingEntry |  | Entries |

### RankingEntry Structure

A character of a ranking page.

Length: 17 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | Rank; The rank, from 1. |
| 2 | 10 | String |  | Name; The name of the character. |
| 12 | 1 | Byte |  | ClassCode; The class code of the character (docs/babylon.md, "Class code"). |
| 13 | 4 | IntegerLittleEndian |  | Value; The value the ranking orders by. |