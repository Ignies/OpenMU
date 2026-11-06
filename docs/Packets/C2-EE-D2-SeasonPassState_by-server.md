# C2 EE D2 - SeasonPassState (by server)

## Is sent when

The character enters a map, the pass gains experience or a level, a reward is claimed, and on request.

## Causes the following actions on the client side

The client shows the season pass.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xD2  | Packet header - sub packet type identifier |
| 5 | 2 | ShortLittleEndian |  | Season; The number of the running season, 0 when none runs. |
| 7 | 4 | IntegerLittleEndian |  | EndsAt; The end of the season, in seconds since 1970-01-01 UTC. |
| 11 | 1 | Byte |  | Level; The pass level, 0 before the first. |
| 12 | 4 | IntegerLittleEndian |  | Experience; The pass experience. |
| 16 | 1 | Byte |  | Passes; Bit 0 the free pass, bit 1 the gold pass, bit 2 the premium pass. |
| 17 | 4 | IntegerLittleEndian |  | ClaimedFree; One bit per level (bit 0 = level 1) whose free reward was claimed. |
| 21 | 4 | IntegerLittleEndian |  | ClaimedGold; The same for the gold pass. |
| 25 | 4 | IntegerLittleEndian |  | ClaimedPremium; The same for the premium pass. |
| 29 | 1 | Byte |  | MissionCount |
| 30 | SeasonPassMission.Length * MissionCount | Array of SeasonPassMission |  | Missions |

### SeasonPassMission Structure

The progress of a daily mission of the season pass.

Length: 6 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Index; The mission index (S21 MUPassMissionText). |
| 1 | 1 | Byte |  | Completions; How often the mission was completed today. |
| 2 | 4 | IntegerLittleEndian |  | Progress; The progress towards the next completion. |