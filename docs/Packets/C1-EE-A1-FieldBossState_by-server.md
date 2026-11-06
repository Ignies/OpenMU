# C1 EE A1 - FieldBossState (by server)

## Is sent when

When the own character enters the map of a field boss, and when the boss spawns, changes its stage, loses a tenth of its health, is killed or the battle ends.

## Causes the following actions on the client side

The client shows the boss state, e.g. the health bar, the respawn timer and the light of the boss zone.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   14   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xA1  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | MapNumber |
| 6 | 2 | ShortLittleEndian |  | BossNumber; The monster number of the boss itself (the last stage). |
| 8 | 1 | Byte |  | Status; 0 waiting, 1 alive, 2 in a fight, 3 defeated. |
| 9 | 1 | Byte |  | HealthPercent; The health of the monster of the current stage, 0 when none lives. |
| 10 | 4 | IntegerLittleEndian |  | SecondsLeft; The seconds to the spawn (waiting), to the end of the battle (in a fight) or to the move out of the arena (defeated); 0 when unknown. |