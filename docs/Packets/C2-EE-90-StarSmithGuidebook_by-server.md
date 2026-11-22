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
| 5 | 1 | Byte |  | Total; How many recipes the world has, for "? of ? found". |
| 6 | 1 | Byte |  | FoundCount |
| 7 | StarSmithGuidebookEntry.Length * FoundCount | Array of StarSmithGuidebookEntry |  | Found |

### StarSmithGuidebookEntry Structure

A found recipe of StarSmithGuidebook.

Length: 36 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 2 | ShortLittleEndian |  | Recipe |
| 2 | 2 | ShortLittleEndian |  | Made; How often the smith made it. |
| 4 | 32 | String |  | Name; UTF-8, zero padded. |