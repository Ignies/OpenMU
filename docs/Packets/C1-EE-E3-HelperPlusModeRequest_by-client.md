# C1 EE E3 - HelperPlusModeRequest (by client)

## Is sent when

The player switches the plus mode of the helper on or off.

## Causes the following actions on the server side

The server answers with HelperPlusRules.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xE3  | Packet header - sub packet type identifier |
| 4 | 1 | Boolean |  | Enabled |