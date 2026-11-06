# C1 EE A3 - MiningResult (by server)

## Is sent when

The player talked to a mining spot, mined a stage or collected.

## Causes the following actions on the client side

The client shows the mining window: the stage, the pending reward or the loss.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   12   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xA3  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | NpcId; The object id of the mining spot. |
| 6 | 1 | Byte |  | Result; 0 opened, 1 stage mined, 2 failed (the pending reward is lost), 3 collected, 4 no pickaxe in the hands, 5 pickaxe worn out, 6 no spot in reach, 7 another player mines the spot, 8 mining is off. |
| 7 | 1 | Byte |  | Stage; The stage reached, 0 before the first. |
| 8 | 2 | ShortLittleEndian |  | ItemCount; The items the stage gives (pending), or the collected ones. |
| 10 | 2 | ShortLittleEndian |  | BonusCount; The bonus items of the jackpot of the last stage, 0 when none. |