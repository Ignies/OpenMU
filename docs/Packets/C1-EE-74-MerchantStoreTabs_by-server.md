# C1 EE 74 - MerchantStoreTabs (by server)

## Is sent when

After the item list of a merchant whose store has more than one tab, and before the item list of another tab.

## Causes the following actions on the client side

The client shows the tabs; the next store item list (C2 31) holds the items of the active tab.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x74  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | TabCount |
| 5 | 1 | Byte |  | ActiveTab; The tab whose items the next store item list holds; 0 is the first. |