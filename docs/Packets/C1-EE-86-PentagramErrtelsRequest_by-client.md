# C1 EE 86 - PentagramErrtelsRequest (by client)

## Is sent when

The client needs the errtels in the sockets of a pentagram, e.g. for its tooltip.

## Causes the following actions on the server side

The server answers with PentagramErrtels.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x86  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | PentagramSlot; The inventory slot of the pentagram. |