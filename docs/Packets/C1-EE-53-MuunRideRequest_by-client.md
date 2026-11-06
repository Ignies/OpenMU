# C1 EE 53 - MuunRideRequest (by client)

## Is sent when

The player mounts or dismounts the Muun of the main slot (Ctrl+Z in Season 21).

## Causes the following actions on the server side

The server answers with MuunActionResponse (action 6) and shows MuunAppearance to the player and the players around.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x53  | Packet header - sub packet type identifier |
| 4 | 1 | Boolean |  | Ride; True to mount, false to dismount. |