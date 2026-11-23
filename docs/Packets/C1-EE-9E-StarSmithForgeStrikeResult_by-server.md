# C1 EE 9E - StarSmithForgeStrikeResult (by server)

## Is sent when

The answer to StarSmithForgeStrike.

## Causes the following actions on the client side

The client draws the link (or the crack) and shows the struck star's element and value. A refused strike changes nothing; its star fields are 0.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9E  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Answer; 0 linked, 1 cracked, 2 refused: no attempt running, 3 refused: no such star, 4 refused: no strikes left, 5 refused: the star is struck, 6 refused: not linked to the last star. |
| 5 | 1 | Byte |  | Star |
| 6 | 1 | Byte |  | Element; 0 fire, 1 frost, 2 storm, 3 shadow, 4 light. |
| 7 | 1 | Byte |  | Value; 1 to 5. |
| 8 | 1 | Byte |  | Flags; 1 the star is a void. |
| 9 | 1 | Byte |  | Timing; 0 clean, 1 shaky, 2 late. |
| 10 | 1 | Byte |  | StrikesLeft |
| 11 | 1 | Byte |  | GuidanceCount |
| 12 | StarSmithForgeGuidance.Length * GuidanceCount | Array of StarSmithForgeGuidance |  | Guidance; The smith's known recipes the path is still a prefix of, at most 60. |

### StarSmithForgeGuidance Structure

A recipe number of the guidance in StarSmithForgeStrikeResult.

Length: 2 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | Recipe |