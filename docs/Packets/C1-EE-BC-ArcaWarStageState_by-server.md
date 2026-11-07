# C1 EE BC - ArcaWarStageState (by server)

## Is sent when

To every player when the stage of the Arca War changes, after a request and when a character enters the world while the war isn't closed.

## Causes the following actions on the client side

The client shows the stage and its countdown.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   16   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xBC  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Stage; 0 closed, 1 guild master registration, 2 member registration, 3 waiting, 4 war, 5 closing. |
| 5 | 1 | Byte |  | Guilds; The registered guilds. |
| 6 | 1 | Boolean |  | Registered; The character is registered. |
| 8 | 4 | IntegerLittleEndian |  | SecondsLeft; The seconds until the next stage. |
| 12 | 4 | IntegerLittleEndian |  | Points; The points of the character in this war. |