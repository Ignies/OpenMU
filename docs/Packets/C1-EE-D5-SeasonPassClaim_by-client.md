# C1 EE D5 - SeasonPassClaim (by client)

## Is sent when

The player claims the reward of a pass level.

## Causes the following actions on the server side

The server delivers it into the gremory case and answers with SeasonPassClaimResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xD5  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Pass; 0 free, 1 gold, 2 premium. |
| 5 | 1 | Byte |  | Level; The pass level of the reward. |