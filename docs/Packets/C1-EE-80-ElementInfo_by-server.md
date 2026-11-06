# C1 EE 80 - ElementInfo (by server)

## Is sent when

After an ElementInfoRequest, and after an errtel was put into or taken out of the equipped pentagram.

## Causes the following actions on the client side

The client shows the elemental stats of the own character in its character window.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   27   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x80  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Element; 0 none, 1 fire, 2 water, 3 earth, 4 wind, 5 darkness. |
| 5 | 4 | IntegerLittleEndian |  | DamageMin; The minimum elemental damage with its bonuses. |
| 9 | 4 | IntegerLittleEndian |  | DamageMax; The maximum elemental damage with its bonuses. |
| 13 | 4 | IntegerLittleEndian |  | Defense |
| 17 | 4 | IntegerLittleEndian |  | AttackRate |
| 21 | 4 | IntegerLittleEndian |  | DefenseRate |
| 25 | 1 | Byte |  | CriticalChance; The elemental critical damage chance in percent. |
| 26 | 1 | Byte |  | DodgeChance; The elemental perfect dodge chance in percent. |