# C1 EE 9C - StarSmithForgeAbandon (by client)

## Is sent when

The smith walks away from the running attempt.

## Causes the following actions on the server side

The ingredients and the stashed figures are lost and the item stays. Answered with StarSmithForgeResult (outcome 2).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   4   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9C  | Packet header - sub packet type identifier |