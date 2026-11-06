# C1 EE 63 - GremoryCaseClaimResponse (by server)

## Is sent when

After a claim of a reward of a gremory case.

## Causes the following actions on the client side

On success, the client removes the reward from the case; the item appears in the inventory by the usual packet.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   7   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x63  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind; 0 the case of the character, 1 the case of the account. |
| 5 | 1 | Byte |  | Slot |
| 6 | 1 | Byte |  | Result; 0 claimed, 1 inventory full, 2 no such reward, 3 expired, 4 the case is off. |