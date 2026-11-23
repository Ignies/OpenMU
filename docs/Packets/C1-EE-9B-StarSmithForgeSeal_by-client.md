# C1 EE 9B - StarSmithForgeSeal (by client)

## Is sent when

The smith seals the drawn constellation.

## Causes the following actions on the server side

The server applies the result and answers with StarSmithForgeResult; refused (outcome 4) without a running attempt or before the first strike.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   4   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9B  | Packet header - sub packet type identifier |