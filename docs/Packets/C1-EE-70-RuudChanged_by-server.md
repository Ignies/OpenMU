# C1 EE 70 - RuudChanged (by server)

## Is sent when

When the character enters a map, when the Ruud shop opens, and when the Ruud of the character changed.

## Causes the following actions on the client side

The client shows the Ruud of the character.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   12   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x70  | Packet header - sub packet type identifier |
| 4 | 4 | IntegerLittleEndian |  | Ruud; The Ruud of the character. |
| 8 | 4 | IntegerLittleEndian |  | Change; The change which led to it, signed (two's complement); 0 when it is just shown. |