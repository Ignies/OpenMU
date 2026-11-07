# C1 EE BB - ArcaWarRequestResponse (by server)

## Is sent when

After an ArcaWarRequest.

## Causes the following actions on the client side

The client tells the result.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xBB  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind |
| 5 | 1 | Byte |  | Result; 0 success, 1 not open, 2 no guild, 3 not the guild master, 4 already registered, 5 too many guilds, 6 guild not registered, 7 too many members, 8 not registered. |