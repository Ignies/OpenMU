# C1 EE B0 - EventState (by server)

## Is sent when

When the state of a Season 21 event or party instance changes for the own character (entrance opened or closed, the game started or ended), and when the character talks to its entrance NPC.

## Causes the following actions on the client side

The client shows the entry window or the state and timer of the event.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   12   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB0  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | EventType; The mini game type of the server (docs/babylon.md, "Events"). |
| 6 | 1 | Byte |  | GameLevel |
| 7 | 1 | Byte |  | State; 0 open (entrance), 1 closed, 2 playing, 3 ended. |
| 8 | 4 | IntegerLittleEndian |  | SecondsLeft; The seconds until the next state; 0 when unknown. |