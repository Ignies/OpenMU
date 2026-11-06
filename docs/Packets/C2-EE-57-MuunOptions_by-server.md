# C2 EE 57 - MuunOptions (by server)

## Is sent when

When the character enters a map, and when an option of an equipped Muun changes (equip, level, evolution, its condition, the end of the additional option).

## Causes the following actions on the client side

The client shows the options of the equipped Muuns and which of them apply now.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x57  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | OptionCount |
| 6 | MuunOptionState.Length * OptionCount | Array of MuunOptionState |  | Options |

### MuunOptionState Structure

The option of an equipped Muun.

Length: 5 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Slot; 0 the main Muun, 1 the sub Muun. |
| 1 | 1 | Byte |  | OptionType; The option type of the Season 21 data (MuunOption.xml OptionType). |
| 2 | 2 | ShortLittleEndian |  | Value; The value, raised by the additional option while it lasts. |
| 4 | 1 | Byte |  | Flags; Bit 0: it applies now (its condition is met). Bit 1: the additional option raises it. |