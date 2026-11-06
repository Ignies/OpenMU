# C1 EE A8 - SoulConverterStateRequest (by client)

## Is sent when

The client opens the monster soul converter window.

## Causes the following actions on the server side

The server answers with SoulConverterState.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   4   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xA8  | Packet header - sub packet type identifier |