# C1 EE B9 - LabyrinthRoomMoveResult (by server)

## Is sent when

After a LabyrinthRoomMoveRequest.

## Causes the following actions on the client side

The client tells why the portal doesn't open, if it doesn't.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB9  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Direction |
| 5 | 1 | Byte |  | Result; 0 moved, 1 no portal or no room behind it, 2 the mission of the room isn't done, 3 not playing the rooms of a labyrinth. |