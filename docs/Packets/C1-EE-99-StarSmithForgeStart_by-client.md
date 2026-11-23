# C1 EE 99 - StarSmithForgeStart (by client)

## Is sent when

The smith starts a recipe attempt in the Constellation Forge.

## Causes the following actions on the server side

The server checks the slots again, deals the sky from a seed of its own, starts the beat on its clock and answers with StarSmithForgeSky.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   21   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x99  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | TargetSlot; The inventory slot of the item worked on. |
| 5 | 1 | Byte |  | IngredientCount; How many of the ingredient slots are used, at most 10. |
| 6 | 1 | Byte |  | CatalystCount; How many of the catalyst slots are used, at most 4. |
| 7 | 10 | Binary |  | IngredientSlots; 10 inventory slots of ingredients; the unused ones are 0xFF. Each slot is one unit of its item (a stack gives one). |
| 17 | 4 | Binary |  | CatalystSlots; 4 inventory slots of catalysts (a Jewel of Guardian reveals a void and protects the item); the unused ones are 0xFF. |