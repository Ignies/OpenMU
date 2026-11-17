# C1 EE 38 - ItemCustomizationReportResult (by server)

## Is sent when

The answer to ItemCustomizationReportRequest.

## Causes the following actions on the client side

The client says whether the report was sent.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x38  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result; 0 sent, 1 reported already (this item at this version), 2 not found (or no name or lore), 3 too fast (one report every ten seconds). |