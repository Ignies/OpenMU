# C1 EE BF - IllusionTempleStones (by server)

## Is sent when

To the players of a renewed Illusion Temple when the battle starts and whenever a stone is sealed or unsealed or the Cursed Hollow changes hands.

## Causes the following actions on the client side

The client shows the stones, their holders and where the hollow is.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xBF  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | StoneCount |
| 5 | 1 | Byte |  | OwnTeam; The team of the player: 1 allied forces, 2 illusion forces. |
| 6 | 1 | Byte |  | HollowState; Where the Cursed Hollow is: 0 in the Cursed Statue, 1 carried, 2 on the ground, 3 none (the statue comes back). |
| 8 | IllusionTempleStoneState.Length * StoneCount | Array of IllusionTempleStoneState |  | Stones |

### IllusionTempleStoneState Structure

A Captured Stone Statue of the renewed Illusion Temple.

Length: 4 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Index; The index of the stone, 0-9 (NPC 659-668). |
| 1 | 1 | Byte |  | Holder; The team which sealed the hollow into it: 0 none, 1 allied forces, 2 illusion forces. |
| 2 | 1 | Byte |  | X |
| 3 | 1 | Byte |  | Y |