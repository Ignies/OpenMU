# C1 EE 61 - GremoryCaseItemAdded (by server)

## Is sent when

A reward was put into a gremory case of the character or its account.

## Causes the following actions on the client side

The client adds the reward to the case and tells the player about it.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   42   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x61  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind; 0 the case of the character, 1 the case of the account. |
| 5 | 1 | Byte |  | Slot |
| 6 | 4 | IntegerLittleEndian |  | ExpiresAt; The point in time at which the reward expires, in seconds since 1970-01-01 UTC. |
| 10 | 32 | Binary |  | ItemData; The item in the W32 item format. |