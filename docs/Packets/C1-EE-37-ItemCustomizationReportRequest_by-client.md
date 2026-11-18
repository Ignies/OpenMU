# C1 EE 37 - ItemCustomizationReportRequest (by client)

## Is sent when

A player reports the name or lore of a customised item from its tooltip.

## Causes the following actions on the server side

The server stores a report for the website's moderation and answers with ItemCustomizationReportResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x37  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Storage |
| 5 | 2 | ShortBigEndian |  | ObjectId |
| 7 | 1 | Byte |  | Slot |