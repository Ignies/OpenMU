# C1 EE 14 - CreatableClasses (by server)

## Is sent when

Right after the class codes of the character list, to a Babylon client.

## Causes the following actions on the client side

The client offers the classes in the character creation and greys out those the server refuses.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x14  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | ClassCount |
| 5 | CreatableClass.Length * ClassCount | Array of CreatableClass |  | Classes |

### CreatableClass Structure

A class which the character creation offers, with the state of the server's creation rule for the account.

Length: 4 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | ClassCode; The class code: base class times 8 plus evolution. |
| 1 | 1 | Byte |  | State; 0 creatable, 1 needs another character at the level, 2 needs the Season 6 unlock (the client checks it), 3 unavailable in this world. |
| 2 | 2 | ShortLittleEndian |  | Level; The creation level of the class, 0 for none. |