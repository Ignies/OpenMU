# C2 EE 50 - MuunInventoryList (by server)

## Is sent when

When the character enters a map while the Muun system is on.

## Causes the following actions on the client side

The client shows the Muun inventory.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x50  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | ItemCount |
| 6 | MuunInventoryItem.Length * ItemCount | Array of MuunInventoryItem |  | Items |

### MuunInventoryItem Structure

An item of the Muun inventory.

Length: 33 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Slot; 0 the main Muun, 1 the sub Muun, 2-101 the bag. |
| 1 | 32 | Binary |  | ItemData; The item in the W32 item format. |