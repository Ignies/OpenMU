# C1 EE 87 - ElementInfoRequest (by client)

## Is sent when

The client needs the elemental stats of the own character, e.g. after an equipment change or to open the character window.

## Causes the following actions on the server side

The server answers with ElementInfo.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   4   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x87  | Packet header - sub packet type identifier |