# C2 EE 43 - AbilityCoreState (by server)

## Is sent when

When a fifth evolution enters a map while the Ability Enhancement Core is on, and after every successful core action.

## Causes the following actions on the client side

The client fills its Ability Enhancement Core window.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x43  | Packet header - sub packet type identifier |
| 5 | 4 | IntegerLittleEndian |  | OpenSlots; The open slots, bit n for the slot n. |
| 9 | 4 | IntegerLittleEndian |  | ActiveSlots; The slots whose card takes effect, bit n for the slot n. |
| 13 | 1 | Byte |  | CardCount |
| 14 | AbilityCoreCard.Length * CardCount | Array of AbilityCoreCard |  | Cards |

### AbilityCoreCard Structure

A card in a slot of the Ability Enhancement Core.

Length: 33 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Slot; 0 the extra damage slot, 1-3 the skill slots, 4-18 the option slots (rows of five: blue, green, purple), 19-21 the elite slots. |
| 1 | 32 | Binary |  | ItemData; The card in the W32 item format: its level is the card level, its option the card option. |