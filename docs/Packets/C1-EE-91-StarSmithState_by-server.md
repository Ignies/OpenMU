# C1 EE 91 - StarSmithState (by server)

## Is sent when

On world entry, when a Star Smith's Hammer is equipped or taken off, and after each smithing action.

## Causes the following actions on the client side

The client shows or hides the Star Smithing HUD with the smith's level and experience.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   16   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x91  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Flags; Bit 0: a Star Smith's Hammer is in a hand. Bit 1: that hammer is broken (no durability left). |
| 5 | 2 | ShortLittleEndian |  | Level |
| 7 | 4 | IntegerLittleEndian |  | Experience |
| 11 | 4 | IntegerLittleEndian |  | NextLevelExperience |
| 15 | 1 | Byte |  | HammerDurability |