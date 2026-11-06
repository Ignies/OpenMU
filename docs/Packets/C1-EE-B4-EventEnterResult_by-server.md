# C1 EE B4 - EventEnterResult (by server)

## Is sent when

When the entry into a Season 21 event or party instance failed.

## Causes the following actions on the client side

The client shows why; the server also tells it in words.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB4  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | EventType |
| 6 | 1 | Byte |  | Result; The enter result of the server: 0 success, 1 failed, 2 not open, 3 level too high, 4 level too low, 5 full, 6 not enough money, 7 player killers can not enter (docs/babylon.md, "Events"). |