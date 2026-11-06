# C1 EE 83 - PentagramResult (by server)

## Is sent when

After an ErrtelSocketRequest.

## Causes the following actions on the client side

The client shows the result; the changed items come with the usual item packets.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x83  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Mode; The mode of the request: 0 insert, 1 take out. |
| 5 | 1 | Byte |  | Result; 0 success, 1 failed (taking out destroyed the errtel), 2 refused (wrong item, element, socket, or no space). |
| 6 | 1 | Byte |  | PentagramSlot |
| 7 | 1 | Byte |  | SocketIndex |