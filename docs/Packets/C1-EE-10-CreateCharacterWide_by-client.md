# C1 EE 10 - CreateCharacterWide (by client)

## Is sent when

The player creates a character of any class, on the character selection screen.

## Causes the following actions on the server side

The server creates the character and answers with the CharacterCreatedWide, or with the character creation failure of Season 6.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   15   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x10  | Packet header - sub packet type identifier |
| 4 | 10 | String |  | Name |
| 14 | 1 | Byte |  | ClassCode; The class code: base class times 8 plus evolution. |