# C1 EE 84 - ElementalDamage (by server)

## Is sent when

A hit of the own character, or a hit on it, deals elemental damage, on a world with the elements.

## Causes the following actions on the client side

The client shows the elemental damage beside the hit, in the color of its element. The damage is a part of the hit's damage, not an extra.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   11   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x84  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | TargetId; The id of the object which took the damage, as in the Season 6 hit packet. |
| 6 | 1 | Byte |  | Element; 1 fire, 2 water, 3 earth, 4 wind, 5 darkness. |
| 7 | 4 | IntegerLittleEndian |  | Damage |