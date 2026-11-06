# C1 EE 56 - MuunAppearance (by server)

## Is sent when

When a player with a Muun comes into view, and when the main Muun of a player changes or it mounts or dismounts.

## Causes the following actions on the client side

The client shows the Muun beside the player, or the player riding it.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   11   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x56  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | PlayerId |
| 6 | 1 | Byte |  | ItemGroup; 16; 0xFF when the player has no main Muun. |
| 7 | 2 | ShortLittleEndian |  | ItemNumber |
| 9 | 1 | Byte |  | ItemLevel; The level of the Muun. |
| 10 | 1 | Boolean |  | IsRiding |