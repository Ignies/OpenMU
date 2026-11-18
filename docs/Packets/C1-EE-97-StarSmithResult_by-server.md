# C1 EE 97 - StarSmithResult (by server)

## Is sent when

The answer to a smithing action.

## Causes the following actions on the client side

The client says what happened.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   12   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x97  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Action; 1 recolour, 2 rename, 3 lore, 4 lock. |
| 5 | 1 | Byte |  | Result; 0 done, 1 no hammer in a hand, 2 hammer broken, 3 the item is locked, 4 not here (trade, shop, duel, event map), 5 no such item, 6 text refused, 7 not enough Zen, 8 smithing is off. |
| 6 | 1 | Byte |  | Refusal; For result 6: the text moderation's refusal (1 empty, 2 too short, 3 too long, 4 too many lines, 5 double space, 6 bad character, 7 link, 8 colour code, 9 word). |
| 7 | 4 | IntegerLittleEndian |  | ZenCost; What the action cost in Zen. |