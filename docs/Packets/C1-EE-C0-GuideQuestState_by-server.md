# C1 EE C0 - GuideQuestState (by server)

## Is sent when

The character entered a map, asked for its guide quest, accepted or completed a step, or the progress of its step changed.

## Causes the following actions on the client side

The client shows the step in the quest guide window and its line on the quest tracker.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xC0  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Status; 0 the class has no guide quests, 1 the step can be accepted, 2 accepted, 3 accepted and its objective is met, 4 the step needs a higher level, 5 every step is done. |
| 5 | 1 | Byte |  | Result; The answer to the request which caused it: 0 none, 1 accepted, 2 completed, 3 the objective isn't met, 4 the level is too low. |
| 6 | 2 | ShortLittleEndian |  | StepNumber; The number of the step (S21Emu QuestGuide.txt); 0 when there is none. |
| 8 | 2 | ShortLittleEndian |  | StepIndex; The place of the step in the chain of the class, from 1. |
| 10 | 2 | ShortLittleEndian |  | StepCount; The number of steps in the chain of the class. |
| 12 | 1 | Byte |  | Chapter |
| 13 | 1 | Byte |  | Objective; 1 collect items, 2 hunt monsters, 3 reach a level, 4 finish a class quest, 5 go to a spot. |
| 14 | 2 | ShortLittleEndian |  | MinimumLevel |
| 16 | 4 | IntegerLittleEndian |  | Target; The monster number, the level, the class quest number (1 second class, 2 hero status, 6 third class) or the item group. |
| 20 | 2 | ShortLittleEndian |  | TargetNumber; The item number of a collect objective. |
| 22 | 2 | ShortLittleEndian |  | RequiredCount; The kills, items or level the objective asks for; 1 for a move or a class quest. |
| 24 | 2 | ShortLittleEndian |  | Progress; How far the objective is: kills, items carried, the level, or 1 when the spot was reached or the class quest is done. |
| 26 | 2 | ShortLittleEndian |  | MapNumber; The map of the spot: where to go, or where the monsters are. |
| 28 | 1 | Byte |  | X |
| 29 | 1 | Byte |  | Y |
| 30 | 2 | ShortLittleEndian |  | Gate; The warp gate of the data which leads near the spot. |
| 32 | 4 | IntegerLittleEndian |  | Experience |
| 36 | 4 | IntegerLittleEndian |  | Money |
| 40 | 40 | String |  | Name; The name of the step, UTF-8, zero padded. |
| 80 | 1 | Byte |  | RewardItemCount |
| 84 | GuideQuestRewardItem.Length * RewardItemCount | Array of GuideQuestRewardItem |  | RewardItems |

### GuideQuestRewardItem Structure

An item reward of a guide quest step.

Length: 4 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Group |
| 1 | 1 | Byte |  | Count |
| 2 | 2 | ShortLittleEndian |  | Number |