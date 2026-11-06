# C1 EE A6 - SoulConverterRewardRequest (by client)

## Is sent when

The player chooses the reward of a completed monster soul converter.

## Causes the following actions on the server side

The server delivers the reward and answers with SoulConverterResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xA6  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | RewardKind; 0 Ruud, 1 jewels, 2 elemental options, 3 accessory. |