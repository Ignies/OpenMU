# C1 EE D7 - SeasonPassStateRequest (by client)

## Is sent when

The client opens the season pass window.

## Causes the following actions on the server side

The server answers with SeasonPassState.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   4   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xD7  | Packet header - sub packet type identifier |