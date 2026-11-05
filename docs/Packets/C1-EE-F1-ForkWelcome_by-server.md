# C1 EE F1 - ForkWelcome (by server)

## Is sent when

After the client sent the ForkHello.

## Causes the following actions on the client side

The client switches to the wire formats and features of the fork. When it does not arrive, the client reconnects with the Season 6 protocol.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   16   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xF1  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | ServerWire; The version of the Babylon protocol which the server speaks. |
| 6 | 4 | IntegerLittleEndian |  | ActiveFeatures; The wanted features which the server supports and uses for this connection. |
| 10 | 1 | Byte |  | ItemFormat; The item data format of every item of this connection: 0 = Season 6 (12 bytes), 1 = W32 (32 bytes). |
| 11 | 1 | Byte |  | ClassEncoding; The class encoding of the Babylon packets: 0 = Season 6 (5 bits), 1 = class code (base class times 8 plus evolution). |
| 12 | 2 | ShortLittleEndian |  | MaximumLevel; The normal level cap of the world. |
| 14 | 2 | ShortLittleEndian |  | MaximumStars; The maximum number of stars of the world, 0 when it has none. |