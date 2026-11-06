# C1 EE E2 - HelperPlusMoveResult (by server)

## Is sent when

After a HelperPlusMoveRequest.

## Causes the following actions on the client side

The client continues the helper in the zone, or stops it.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xE2  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | ZoneIndex |
| 6 | 1 | Byte |  | Result; 0 moved, 1 unknown zone, 2 level too low for the zone, 3 not enough money, 4 the plus mode is not on, 5 the helper plus rules are off, 6 the character can not move now. |