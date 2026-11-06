# C1 EE B7 - LabyrinthRoomMission (by server)

## Is sent when

When the character enters a room of the Labyrinth of Dimensions, when the progress of its mission changes (damage and helper time at most once a second) and when the mission is done.

## Causes the following actions on the client side

The client shows the mission of the room and its progress.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xB7  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | AreaType |
| 5 | 1 | Byte |  | MissionIndex |
| 6 | 1 | Boolean |  | Completed |
| 7 | 1 | Byte |  | GoalCount |
| 8 | LabyrinthGoalState.Length * GoalCount | Array of LabyrinthGoalState |  | Goals |

### LabyrinthGoalState Structure

A goal of the mission of a room of the Labyrinth of Dimensions and its progress.

Length: 12 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Kind; The kind: 1 kill, 2 kill the monsters of the room in time, 3 damage, 4 MU Helper seconds, 5 kill the monsters of the room in time without a health potion, 6 the same without a mana potion, 7 experience (counts as met). |
| 4 | 4 | IntegerLittleEndian |  | Target; The target: monsters (1), minutes (2, 5, 6), damage (3), seconds (4) or experience (7). |
| 8 | 4 | IntegerLittleEndian |  | Progress; The progress since the mission (re)started: the kills for 1, 2, 5 and 6, the damage for 3, the seconds for 4. |