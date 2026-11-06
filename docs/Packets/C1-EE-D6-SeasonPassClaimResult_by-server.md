# C1 EE D6 - SeasonPassClaimResult (by server)

## Is sent when

After a SeasonPassClaim.

## Causes the following actions on the client side

The client shows the result.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xD6  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Pass |
| 5 | 1 | Byte |  | Level |
| 6 | 1 | Byte |  | Result; 0 claimed, 1 level not reached, 2 already claimed, 3 the pass is not owned, 4 no reward at this level, 5 the reward could not be delivered, 6 the season pass is off. |