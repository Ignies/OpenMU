# C1 EE 5A - MuunSkillUsed (by server)

## Is sent when

The Muun of a player in view hit a monster with its attack skill.

## Causes the following actions on the client side

The client shows the attack of the Muun and the damage on the monster.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   13   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x5A  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | PlayerId |
| 6 | 2 | ShortBigEndian |  | TargetId |
| 8 | 1 | Byte |  | SkillLevel |
| 9 | 4 | IntegerLittleEndian |  | Damage |