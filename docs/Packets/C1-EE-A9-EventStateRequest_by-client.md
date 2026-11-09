# C1 EE A9 - EventStateRequest (by client)

## Is sent when

The client shows the entry of a Season 21 event of a game level, e.g. its entry window or its row under the event timers.

## Causes the following actions on the server side

The server answers with EventState: open with the seconds until the entrance closes, playing, or closed with the seconds until the next start.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xA9  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | EventType; The mini game type of the server (docs/babylon.md, "Events"). |
| 6 | 1 | Byte |  | GameLevel |