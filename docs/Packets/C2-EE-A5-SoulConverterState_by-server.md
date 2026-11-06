# C2 EE A5 - SoulConverterState (by server)

## Is sent when

The character enters a map, the state changes (a soul is absorbed, a mission is done, the week ends) and on request.

## Causes the following actions on the client side

The client shows the active monster soul converter with its missions.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xA5  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Tier; The tier of the active converter, 1 to 5; 0 when none is active. |
| 6 | 1 | Byte |  | PurchasesLeft; The converters the character may still activate this week. |
| 7 | 4 | IntegerLittleEndian |  | ExpiresAt; The point in time at which the converter expires, in seconds since 1970-01-01 UTC; 0 when none. |
| 11 | 1 | Byte |  | MissionCount |
| 12 | SoulConverterMission.Length * MissionCount | Array of SoulConverterMission |  | Missions |

### SoulConverterMission Structure

A soul mission of a monster soul converter.

Length: 8 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | MapNumber; The map of the monster. |
| 2 | 2 | ShortLittleEndian |  | MonsterNumber; The monster whose souls count. |
| 4 | 2 | ShortLittleEndian |  | Souls; The souls absorbed so far. |
| 6 | 2 | ShortLittleEndian |  | RequiredSouls; The souls the mission needs. |