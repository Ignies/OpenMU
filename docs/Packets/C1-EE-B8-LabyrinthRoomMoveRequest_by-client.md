# C1 EE B8 - LabyrinthRoomMoveRequest (by client)

## Is sent when

The character walked into a portal of its room in the Labyrinth of Dimensions.

## Causes the following actions on the server side

The server moves the character into the next room (LabyrinthRoomBoard and LabyrinthRoomMission follow) and answers with LabyrinthRoomMoveResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB8  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Direction; 0 top, 1 bottom, 2 left, 3 right. |