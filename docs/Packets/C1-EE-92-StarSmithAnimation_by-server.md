# C1 EE 92 - StarSmithAnimation (by server)

## Is sent when

A smithing action is done: to the smith and every player who sees the smith, just before the smith's StarSmithResult.

## Causes the following actions on the client side

The client places a bench in front of the smith and plays the hammering strikes; the smith's own result line waits for the last strike.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x92  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | ObjectId; The smith, as the receiving client knows it. |
| 6 | 1 | Byte |  | Action; 1 recolour, 2 rename, 3 lore, 4 lock. |
| 7 | 1 | Byte |  | Strikes; How many hammer strikes to play (3). |