# C2 EE 34 - ItemCustomizations (by server)

## Is sent when

The answer to ItemCustomizationRequest.

## Causes the following actions on the client side

For each asked place, the client replaces what it knew with the listed items; an asked place with no entry has no customised item.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x34  | Packet header - sub packet type identifier |
| 5 | 2 | ShortLittleEndian |  | Count |
| 7 | ItemCustomizationEntry.Length * Count | Array of ItemCustomizationEntry |  | Entries |

### ItemCustomizationEntry Structure

The customisation of one item: its look, name and rarity (docs/babylon.md, "Item customisation").

Length: 123 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Storage; The storage of the item, as in ItemCustomizationAsk. |
| 1 | 2 | ShortBigEndian |  | ObjectId |
| 3 | 1 | Byte |  | Slot; The slot of the item in its storage. |
| 4 | 4 | IntegerLittleEndian |  | Version; Goes up on every change of the customisation; the client keeps the detail by it. |
| 8 | 1 | Byte |  | Flags; 1 look, 2 name, 4 lore waiting (ask ItemCustomizationDetailRequest), 8 aura, 16 particles. |
| 9 | 1 | Byte |  | Rarity; 0 Common, 1 Uncommon, 2 Rare, 3 Epic, 4 Legendary, 5 Mythic. |
| 10 | 31 | Binary |  | Look; The recolour look: mode, flags, tone RGB, strength, hue (2, big endian), keep from (2, big endian), keep range, glow RGB, weapon fx, fx colour count, 3 fx colours, 6 fx tuning values. |
| 41 | 72 | Binary |  | Name; The custom name, UTF-8, zero padded. |
| 113 | 2 | ShortLittleEndian |  | AuraId; The aura look of the item, 0 when none. |
| 115 | 8 | Binary |  | Particles; The particle layer of the item: kind, RGB, density, rate, size, reserved; zero when none. |