# C1 EE 32 - MagicEffectStatusWide (by server)

## Is sent when

A magic effect numbered past 255 (a later season's effect, such as the Blessing of Eunice 447) was added to or removed from the own player; the Season 6 packet (07) carries the number in a byte.

## Causes the following actions on the client side

The client shows the effect's icon with its remaining time, or hides it.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   13   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x32  | Packet header - sub packet type identifier |
| 4 | 1 | Boolean |  | IsActive |
| 5 | 2 | ShortBigEndian |  | ObjectId |
| 7 | 2 | ShortLittleEndian |  | EffectNumber |
| 9 | 4 | IntegerLittleEndian |  | SecondsLeft; The remaining seconds of the effect, 0 when it's removed or its duration isn't sent. |