# C2 EE 98 - StarSmithDyes (by server)

## Is sent when

With StarSmithState while the hammer is in a hand, and after the dyes change.

## Causes the following actions on the client side

The client shows the swatches of the dye in the dye square, the ones the server accepts.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x98  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Count |
| 6 | StarSmithDyeEntry.Length * Count | Array of StarSmithDyeEntry |  | Dyes |

### StarSmithDyeEntry Structure

One dye of StarSmithDyes: the item, what it allows and its swatches.

Length: 30 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Group |
| 1 | 2 | ShortBigEndian |  | Number |
| 3 | 1 | Byte |  | Flags; 1 the hue shift (Prismatic), 2 the keep mask (rare), 4 the Radiant Dye (a glow). |
| 4 | 1 | Byte |  | MaximumStrength |
| 5 | 1 | Byte |  | SwatchCount; How many of the 8 swatches are set. |
| 6 | 24 | Binary |  | Swatches; 8 colours, r g b each. |