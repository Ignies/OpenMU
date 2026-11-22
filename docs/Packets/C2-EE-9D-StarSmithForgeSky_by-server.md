# C2 EE 9D - StarSmithForgeSky (by server)

## Is sent when

The answer to StarSmithForgeStart: the dealt sky, or why there is none.

## Causes the following actions on the client side

The client lays the stars out over the item's silhouette and starts the beat. Element, value and void of a star are never sent before it is struck, except the void a Jewel of Guardian reveals.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x9D  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Result; 0 started; else the StarSmithResult codes (1 no hammer, 2 hammer broken, 3 locked, 4 not here, 5 no such item, 8 off), 11 no recipe for these ingredients, 12 an attempt is already running, 13 a slot is wrong (empty, used twice, the hammer or the item). Only the header is filled when not 0. |
| 6 | 1 | Byte |  | TargetSlot |
| 7 | 1 | Byte |  | Strikes; How many stars may be struck. |
| 8 | 1 | Byte |  | Risk; The worst fate among the sky's recipes after protection: 0 the ingredients are lost, 1 the item is downgraded, 2 the item is destroyed. |
| 9 | 1 | Byte |  | StarCount |
| 10 | 2 | ShortLittleEndian |  | BeatIntervalMs |
| 12 | 2 | ShortLittleEndian |  | FirstBeatMs; The time from sending this packet to the first beat, in milliseconds. |
| 14 | 1 | Byte |  | EntryCount |
| 15 | StarSmithForgeSkyEntry.Length * EntryCount | Array of StarSmithForgeSkyEntry |  | Entries; The stars (in index order), then the links, then the hints. |

### StarSmithForgeSkyEntry Structure

One entry of StarSmithForgeSky: a star, a link or a hint.

Length: 5 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Kind; 0 a star, 1 a link, 2 a hint. |
| 1 | 1 | Byte |  | A; A star: its index. A link or hint: one star. |
| 2 | 1 | Byte |  | B; A star: X, 0 to 255 across the item's box (a layout hint). A link or hint: the other star. |
| 3 | 1 | Byte |  | C; A star: Y, 0 to 255 down the item's box. A hint: its clarity, 0 to 255. A link: 0. |
| 4 | 1 | Byte |  | D; A star: flags, 1 a void revealed by a Jewel of Guardian. Else 0. |