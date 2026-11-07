# C1 EE BD - ArcaWarObelisks (by server)

## Is sent when

Once a second during the Arca War, to the players on its maps.

## Causes the following actions on the client side

The client shows the obelisks, their holders, health, auras and shields.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xBD  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | ObeliskCount |
| 8 | ArcaObeliskState.Length * ObeliskCount | Array of ArcaObeliskState |  | Obelisks |

### ArcaObeliskState Structure

An obelisk of the Arca War as the player sees it.

Length: 24 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Index; The index of the obelisk (1-3 in the data). |
| 1 | 1 | Byte |  | Element; The element of this war: 1 fire, 2 water, 3 earth, 4 wind, 5 darkness. |
| 2 | 2 | ShortLittleEndian |  | MapNumber |
| 4 | 1 | Byte |  | X |
| 5 | 1 | Byte |  | Y |
| 6 | 1 | Byte |  | HealthPercent; The health in percent; 0 while the obelisk is destroyed and comes back. |
| 7 | 1 | Byte |  | AuraMask; The auras members of the player's guild stand on: bit i for aura i. |
| 8 | 1 | Boolean |  | IsOpen; The shield is down for the player's guild, which may destroy the obelisk. |
| 12 | 4 | IntegerLittleEndian |  | OwnerGuildId; The guild which holds the obelisk, 0 for none. |
| 16 | 8 | String |  | OwnerGuildName |