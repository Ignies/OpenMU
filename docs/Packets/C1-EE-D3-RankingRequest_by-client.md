# C1 EE D3 - RankingRequest (by client)

## Is sent when

The player opens a page of a ranking.

## Causes the following actions on the server side

The server answers with RankingPage.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xD3  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind; 0 total level, 1 monster kills of the week, 2 hunting experience of the week. |
| 5 | 1 | Byte |  | Page; The page, from 0. |