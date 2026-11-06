# C2 EE 60 - GremoryCaseList (by server)

## Is sent when

When the character enters a map, and on request, for each of the two cases.

## Causes the following actions on the client side

The client shows the rewards of the case.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x60  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | Kind; 0 the case of the character, 1 the case of the account. |
| 6 | 1 | Byte |  | Capacity; The number of rewards the case holds. |
| 7 | 1 | Byte |  | ItemCount |
| 8 | GremoryCaseItem.Length * ItemCount | Array of GremoryCaseItem |  | Items |

### GremoryCaseItem Structure

A reward in a gremory case.

Length: 37 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Slot; The slot of the reward in the case, which a claim refers to. |
| 1 | 4 | IntegerLittleEndian |  | ExpiresAt; The point in time at which the reward expires, in seconds since 1970-01-01 UTC. |
| 5 | 32 | Binary |  | ItemData; The item in the W32 item format. |