# C1 EE 9A - StarSmithForgeStrike (by client)

## Is sent when

The smith strikes a star of the running attempt.

## Causes the following actions on the server side

The server stamps the time on arrival, judges it against the beat and answers with StarSmithForgeStrikeResult (and StarSmithForgeResult on a crack).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9A  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Star; The star's index in the sky. |