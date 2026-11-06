# C1 EE 62 - GremoryCaseClaim (by client)

## Is sent when

The player wants to take a reward of a gremory case into the inventory.

## Causes the following actions on the server side

The server moves the reward into the inventory and answers with GremoryCaseClaimResponse.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x62  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind; 0 the case of the character, 1 the case of the account. |
| 5 | 1 | Byte |  | Slot |