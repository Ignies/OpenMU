# C1 EE 65 - GremoryCaseListRequest (by client)

## Is sent when

The client wants the content of the gremory cases again, e.g. when the player opens the window.

## Causes the following actions on the server side

The server sends GremoryCaseList for both cases.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   4   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x65  | Packet header - sub packet type identifier |