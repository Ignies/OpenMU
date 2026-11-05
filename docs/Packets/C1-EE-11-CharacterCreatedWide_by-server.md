# C1 EE 11 - CharacterCreatedWide (by server)

## Is sent when

After the server created a character which the Babylon client asked for.

## Causes the following actions on the client side

The client shows the new character in its slot.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   16   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x11  | Packet header - sub packet type identifier |
| 4 | 10 | String |  | Name |
| 14 | 1 | Byte |  | SlotIndex |
| 15 | 1 | Byte |  | ClassCode; The class code: base class times 8 plus evolution. |