# C2 EE D1 - HuntingRecordList (by server)

## Is sent when

After a HuntingRecordRequest.

## Causes the following actions on the client side

The client shows the hunting record of the map, one row a day, newest first.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0xD1  | Packet header - sub packet type identifier |
| 5 | 2 | ShortLittleEndian |  | MapNumber |
| 7 | 1 | Byte |  | DayCount |
| 8 | HuntingRecordDay.Length * DayCount | Array of HuntingRecordDay |  | Days |

### HuntingRecordDay Structure

The hunting of one day on one map.

Length: 28 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 4 | IntegerLittleEndian |  | Day; The start of the day (UTC), in seconds since 1970-01-01. |
| 4 | 4 | IntegerLittleEndian |  | Seconds; The seconds the character spent on the map. |
| 8 | 4 | IntegerLittleEndian |  | Kills; The monsters the character killed. |
| 12 | 8 | LongLittleEndian |  | Experience; The experience the character gained by kills, normal and master. |
| 20 | 8 | LongLittleEndian |  | Damage; The damage the character dealt to monsters. |