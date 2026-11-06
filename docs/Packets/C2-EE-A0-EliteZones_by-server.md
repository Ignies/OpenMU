# C2 EE A0 - EliteZones (by server)

## Is sent when

When the own character enters a map with elite zones, on a world with the elite zones.

## Causes the following actions on the client side

The client marks the zones on the map and the minimap.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xA0  | Packet header - sub packet type identifier |
| 5 | 2 | ShortLittleEndian |  | MapNumber |
| 7 | 1 | Byte |  | ZoneCount |
| 8 | EliteZone.Length * ZoneCount | Array of EliteZone |  | Zones |

### EliteZone Structure

An elite zone of the map.

Length: 8 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Element; 1 fire, 2 water, 3 earth, 4 wind, 5 darkness. |
| 1 | 1 | Byte |  | EntranceX |
| 2 | 1 | Byte |  | EntranceY |
| 3 | 1 | Byte |  | X1; The lowest x coordinate of the area. |
| 4 | 1 | Byte |  | Y1; The lowest y coordinate of the area. |
| 5 | 1 | Byte |  | X2; The highest x coordinate of the area. |
| 6 | 1 | Byte |  | Y2; The highest y coordinate of the area. |
| 7 | 1 | Byte |  | CanEnter; 1 when the character meets the entry rule. |