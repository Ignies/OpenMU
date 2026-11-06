# C1 EE B1 - EventEnterRequest (by client)

## Is sent when

The player wants to enter a Season 21 event or party instance of a game level, e.g. by the entry window.

## Causes the following actions on the server side

The server checks the entry rules and warps the character into the event, or answers with EventEnterResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB1  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | EventType |
| 6 | 1 | Byte |  | GameLevel |