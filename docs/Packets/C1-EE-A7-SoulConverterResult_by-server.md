# C1 EE A7 - SoulConverterResult (by server)

## Is sent when

After a SoulConverterRewardRequest.

## Causes the following actions on the client side

The client shows the result.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xA7  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result; 0 rewarded, 1 missions not done, 2 no active converter, 3 the reward kind is not offered at this tier, 4 the converter is off, 5 the reward could not be delivered. |
| 5 | 1 | Byte |  | RewardKind |