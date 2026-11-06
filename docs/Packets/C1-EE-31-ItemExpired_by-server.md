# C1 EE 31 - ItemExpired (by server)

## Is sent when

An item of the character or its account expired and was removed (fork worlds with item expiry).

## Causes the following actions on the client side

The client tells the player which item expired. The removal itself is sent as usual (item removed, gremory case expired).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   9   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x31  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Storage; 0 inventory, 1 vault, 2 gremory case of the character, 3 gremory case of the account. |
| 5 | 1 | Byte |  | Slot; The slot of the item in its storage. |
| 6 | 1 | Byte |  | ItemGroup |
| 7 | 2 | ShortLittleEndian |  | ItemNumber |