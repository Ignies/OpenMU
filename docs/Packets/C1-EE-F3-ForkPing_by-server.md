# C1 EE F3 - ForkPing (by server)

## Is sent when

With StarSmithState while Star Smithing is on, at most one out and at most one per 2 seconds.

## Causes the following actions on the client side

The client answers at once with ForkPong and the same token; the server measures the round trip on its own clock (the Constellation Forge's beat window).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xF3  | Packet header - sub packet type identifier |
| 4 | 4 | IntegerLittleEndian |  | Token; The token the answer carries back. |