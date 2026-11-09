# C1 EE C1 - GuideQuestRequest (by client)

## Is sent when

The player opens the quest guide, or accepts or completes its step.

## Causes the following actions on the server side

The server answers with GuideQuestState; a request for another step than the current one only gets the state.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xC1  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Action; 0 show the state, 1 accept, 2 complete. |
| 6 | 2 | ShortLittleEndian |  | StepNumber; The step the client shows. |