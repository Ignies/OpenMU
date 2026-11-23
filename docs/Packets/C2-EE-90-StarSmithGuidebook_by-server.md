# C2 EE 90 - StarSmithGuidebook (by server)

## Is sent when

With StarSmithState while the hammer is in a hand, and after an attempt is sealed.

## Causes the following actions on the client side

The client shows the Guidebook: the found recipes in full, the rest as sealed rows. Nothing names a recipe the smith has not found.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x90  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Total; How many recipes the world has, for "? of N found". |
| 6 | 2 | ShortLittleEndian |  | EntryCount; The entries: rows and the lines of the found rows. |
| 8 | StarSmithGuidebookEntry.Length * EntryCount | Array of StarSmithGuidebookEntry |  | Entries |

### StarSmithGuidebookEntry Structure

An entry of StarSmithGuidebook (v2): a found recipe (kind 0) followed by its lines, or a sealed one (kind 1) with its hint only. The fields below are a row's; a line has other bytes after its kind. Kind 2, an ingredient line: 1 the count in the line (1 to 12), then from 2 five bytes per ingredient: group, number (2, little endian), minimum, maximum. Kind 3, a boost line: 1 the count (1 to 8), then from 2 nine bytes per stat: the stat, the least and the most value (each a 32-bit little endian integer, the value times 100) over the outcomes the smith has seen, unscaled. The rest of a line is zero.

Length: 75 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Kind; 0 a found recipe, 1 a sealed one, 2 an ingredient line, 3 a boost line. |
| 1 | 2 | ShortLittleEndian |  | Recipe; The recipe's number; 0 for a sealed row. |
| 3 | 2 | ShortLittleEndian |  | Made; How often the smith made it; 0 for a sealed row. |
| 5 | 1 | Byte |  | ShapeLength; The stars of a found recipe's shape; 0 for a sealed row. |
| 6 | 1 | Byte |  | HintKind; A sealed row's hint: 0 none, 1 a riddle (the text), 2 an ingredient (the item below); 0 for a found row. |
| 7 | 1 | Byte |  | HintGroup; The item group of an ingredient hint. |
| 8 | 2 | ShortLittleEndian |  | HintNumber; The item number of an ingredient hint. |
| 10 | 1 | Byte |  | Lines; The ingredient and boost lines that follow a found row; 0 for a sealed row. |
| 11 | 64 | String |  | Text; UTF-8, zero padded: a found recipe's name, a sealed row's riddle; empty otherwise. |