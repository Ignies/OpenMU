# C1 EE E1 - HelperPlusMoveRequest (by client)

## Is sent when

The helper plus moves the character to a hunting zone.

## Causes the following actions on the server side

The server checks the zone and the warp rules, moves the character and answers with HelperPlusMoveResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xE1  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | ZoneIndex; The index of the hunting zone (S21 MuHelperPlus.xml HuntingZone). |