# C1 EE F4 - ForkPong (by client)

## Is sent when

At once on a ForkPing, and never otherwise.

## Causes the following actions on the server side

The server takes the time since its ping as a round trip sample; another token or an answer after 5 seconds is ignored.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xF4  | Packet header - sub packet type identifier |
| 4 | 4 | IntegerLittleEndian |  | Token; The token of the ping it answers. |