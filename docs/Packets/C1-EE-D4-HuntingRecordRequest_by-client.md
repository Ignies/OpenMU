# C1 EE D4 - HuntingRecordRequest (by client)

## Is sent when

The player opens the hunting record of a map.

## Causes the following actions on the server side

The server answers with HuntingRecordList.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xD4  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | MapNumber; The map; 0xFFFF for the current one. |