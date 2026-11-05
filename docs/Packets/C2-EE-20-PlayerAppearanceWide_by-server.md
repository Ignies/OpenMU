# C2 EE 20 - PlayerAppearanceWide (by server)

## Is sent when

Right after the Season 6 packet which adds a player to the view of a Babylon client.

## Causes the following actions on the client side

The client takes the class of the player from its code and its gear from the wide item data, instead of the 5 bit class and the Season 6 appearance.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x20  | Packet header - sub packet type identifier |
| 5 | 2 | ShortBigEndian |  | PlayerId; The id of the player, as in the Season 6 packet. |
| 7 | 1 | Byte |  | ClassCode; The class code: base class times 8 plus evolution. |
| 8 | 1 | Byte |  | ItemCount |
| 9 | EquippedItemWide.Length * ItemCount | Array of EquippedItemWide |  | Items |

### EquippedItemWide Structure

A piece of gear a player wears, in the W32 item format.

Length: 33 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Slot; The inventory slot (0-11 of Season 6, or one of the slots after it). |
| 1 | 32 | Binary |  | ItemData; The item data in the item format of the connection (see ForkWelcome.ItemFormat), zero filled to 32 bytes. |