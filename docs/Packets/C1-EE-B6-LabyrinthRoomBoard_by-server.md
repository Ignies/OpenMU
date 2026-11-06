# C1 EE B6 - LabyrinthRoomBoard (by server)

## Is sent when

When a floor of the Labyrinth of Dimensions starts in its entrance room, after each move to another room and when the mission of the room is done, while the plugin of the labyrinth rooms is on.

## Causes the following actions on the client side

The client draws the area of the other map as the room, its portals and the board of the floor.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   44   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB6  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | PathNumber; The number of the path of the floor (1-30 in the data). |
| 5 | 1 | Byte |  | RoomIndex; The room the character is in: row * 10 + column. |
| 6 | 2 | ShortLittleEndian |  | MapNumber; The map whose area the room shows. |
| 8 | 1 | Byte |  | StartX |
| 9 | 1 | Byte |  | StartY |
| 10 | 1 | Byte |  | EndX |
| 11 | 1 | Byte |  | EndY |
| 12 | 1 | Byte |  | AreaType; The kind of room: 1 entrance, 2 normal, 3 mission, 4 main mission. |
| 13 | 1 | Byte |  | AreaNumber; The number of the area (EventMaps ID of the data). |
| 14 | 1 | Byte |  | PortalTop; The portal to the row above: 0 closed, 1 open, 2 opens with the mission of the room. |
| 15 | 1 | Byte |  | PortalBottom |
| 16 | 1 | Byte |  | PortalLeft |
| 17 | 1 | Byte |  | PortalRight |
| 18 | 13 | Binary |  | Discovered; The rooms the character opened on this floor: bit (index % 8) of byte (index / 8). |
| 31 | 13 | Binary |  | Cleared; The rooms whose mission is done, in the same bit order. |