# C1 EE A2 - MiningRequest (by client)

## Is sent when

The player mines the next stage of a mining spot, or stops and collects what the spot gave so far.

## Causes the following actions on the server side

The server answers with MiningResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xA2  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | NpcId; The object id of the mining spot. |
| 6 | 1 | Byte |  | Action; 0 mine the next stage, 1 stop and collect. |