# C1 EE 64 - GremoryCaseItemExpired (by server)

## Is sent when

A reward of a gremory case expired before it was claimed.

## Causes the following actions on the client side

The client removes the reward from the case.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   6   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x64  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind; 0 the case of the character, 1 the case of the account. |
| 5 | 1 | Byte |  | Slot |