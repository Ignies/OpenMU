# C1 EE B3 - EventResult (by server)

## Is sent when

When a Season 21 event or party instance ended for the own character.

## Causes the following actions on the client side

The client shows the result window; item rewards arrive in the gremory case or the inventory by their own packets.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   20   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB3  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | EventType |
| 6 | 1 | Boolean |  | Success |
| 8 | 4 | IntegerLittleEndian |  | Score |
| 12 | 2 | ShortLittleEndian |  | Rank |
| 16 | 4 | IntegerLittleEndian |  | Experience; The experience of the reward, at most 4294967295. |