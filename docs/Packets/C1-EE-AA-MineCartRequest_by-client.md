# C1 EE AA - MineCartRequest (by client)

## Is sent when

The player clicks the Kubera Mine cart beside it to ride, or the ride reaches the rail's end.

## Causes the following actions on the server side

The server moves the player at once onto the rail (board) or past its end (leave), or does nothing.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xAA  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Action; 0 board the cart, 1 leave it. |