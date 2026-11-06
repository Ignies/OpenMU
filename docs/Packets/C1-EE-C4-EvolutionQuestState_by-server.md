# C1 EE C4 - EvolutionQuestState (by server)

## Is sent when

The character entered a map, completed a quest, or its experience got held at the level of its next evolution.

## Causes the following actions on the client side

The client shows what the next evolution asks for, and that the experience is held.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   10   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xC4  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | NextEvolution; The evolution of the next class (3 fourth, 4 fifth), 0xFF when no evolution quest leads further. |
| 5 | 1 | Boolean |  | RuleMet; The character meets the level rule of the next evolution and may start its quest. |
| 6 | 1 | Boolean |  | ExperienceHeld; The character gains no experience until it evolves. |
| 7 | 1 | Byte |  | LevelKey; What the required level counts: 1 level plus master level, 2 power level (level plus the worth of the stars), 3 Season 21 total level. |
| 8 | 2 | ShortLittleEndian |  | RequiredLevel; The level which the next evolution asks for. |