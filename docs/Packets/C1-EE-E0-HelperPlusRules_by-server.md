# C1 EE E0 - HelperPlusRules (by server)

## Is sent when

The character enters a map while the helper plus rules are on, and after a HelperPlusModeRequest.

## Causes the following actions on the client side

The client limits its helper settings to the ranges and shows whether the helper and its plus mode may run.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   13   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xE0  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Flags; Bit 0 the helper may run on this map, bit 1 the character may use the plus mode, bit 2 the plus mode is on. |
| 5 | 2 | ShortLittleEndian |  | PlusMinimumLevel; The total level (level + master level) the plus mode needs. |
| 7 | 1 | Byte |  | HuntRange; The largest hunting range of the normal helper, in tiles. |
| 8 | 1 | Byte |  | PickupRange; The largest pick-up range of the normal helper, in tiles. |
| 9 | 1 | Byte |  | PlusHuntRange; The largest hunting range of the plus mode, in tiles. |
| 10 | 1 | Byte |  | PlusPickupRange; The largest pick-up range of the plus mode, in tiles. |
| 11 | 2 | ShortLittleEndian |  | PlusCostPercent; The cost of the plus mode in percent of the normal helper cost. |