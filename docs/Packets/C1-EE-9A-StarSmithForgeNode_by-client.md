# C1 EE 9A - StarSmithForgeNode (by client)

## Is sent when

The smith places a node of the running attempt (PR 7a).

## Causes the following actions on the server side

The server stamps the time on arrival, checks the node (a close, too near, a fault), judges it against the beat, matches the figure and answers with StarSmithForgeNodeResult (and StarSmithForgeResult on a crack).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9A  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | X; The point in the item's square picture box, 0 (left) to 65535 (right). |
| 6 | 2 | ShortLittleEndian |  | Y; 0 (top) to 65535 (bottom). |