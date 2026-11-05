# C1 EE 12 - CharacterClassCodes (by server)

## Is sent when

Right after the character list, to a Babylon client.

## Causes the following actions on the client side

The client takes the class of each character from its code instead of the 5 bits of the Season 6 appearance.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x12  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | CharacterCount |
| 5 | CharacterClassCode.Length * CharacterCount | Array of CharacterClassCode |  | Characters |

### CharacterClassCode Structure

The class code of a character of the character list.

Length: 2 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | SlotIndex; The slot of the character, as in the character list. |
| 1 | 1 | Byte |  | ClassCode; The class code: base class times 8 plus evolution. |