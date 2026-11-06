# C1 EE 75 - MerchantStoreTabSelect (by client)

## Is sent when

The player selected another tab of the store of the opened merchant.

## Causes the following actions on the server side

The server answers with MerchantStoreTabs and the store item list of the tab; purchases then refer to its items.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x75  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Tab |