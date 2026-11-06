# C1 EE B2 - EventScore (by server)

## Is sent when

While a Season 21 event runs: when the score, the round or the rank of the own character changes, and once a second for the timer.

## Causes the following actions on the client side

The client updates the score window of the event.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   20   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB2  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | EventType |
| 6 | 1 | Byte |  | Round; The round, floor or stage of the event, 0 when it has none. |
| 8 | 4 | IntegerLittleEndian |  | Score; The points, or the damage for the boss battle. |
| 12 | 2 | ShortLittleEndian |  | Rank; The rank of the own character (or its party), starting at 1; 0 when unranked. |
| 14 | 2 | ShortLittleEndian |  | PlayerCount |
| 16 | 4 | IntegerLittleEndian |  | SecondsLeft |