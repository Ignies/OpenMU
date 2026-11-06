# C1 EE 81 - ErrtelSocketRequest (by client)

## Is sent when

The player puts an errtel into a socket of a pentagram, or takes one out.

## Causes the following actions on the server side

The server checks the pentagram, the errtel and the socket, moves the errtel and answers with PentagramResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x81  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Mode; 0 inserts the errtel of ErrtelSlot, 1 takes the errtel of SocketIndex out. |
| 5 | 1 | Byte |  | PentagramSlot; The inventory slot of the pentagram (also the equipped slot 236). |
| 6 | 1 | Byte |  | ErrtelSlot; The inventory slot of the errtel to insert; ignored when taking out. |
| 7 | 1 | Byte |  | SocketIndex; The socket of the pentagram, 0-4 (the socket of the errtel's kind: anger, blessing, integrity, divinity, radiance). |