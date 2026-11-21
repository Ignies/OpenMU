# C1 EE 93 - StarSmithRecolorRequest (by client)

## Is sent when

The smith recolours an item of the inventory.

## Causes the following actions on the server side

The server checks the look against the dyes named, takes the dyes, the Zen and the hammer's durability, saves the look and answers with StarSmithResult.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   38   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x93  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Slot; The inventory slot of the item. |
| 5 | 31 | Binary |  | Look; The 31 look bytes of an ItemCustomizationEntry; all zero gives the item its own colours back. |
| 36 | 1 | Byte |  | DyeSlot; The inventory slot of the colour dye spent (a basic, rare or Prismatic Dye), 0xFF for none (the original colours). |
| 37 | 1 | Byte |  | RadiantSlot; The inventory slot of the Radiant Dye spent for a glow, 0xFF for none. |