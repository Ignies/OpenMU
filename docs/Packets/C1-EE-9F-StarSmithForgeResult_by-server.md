# C1 EE 9F - StarSmithForgeResult (by server)

## Is sent when

An attempt ends (sealed, cracked, abandoned or timed out), or a seal is refused.

## Causes the following actions on the client side

The client shows the result; the item's new customisation follows as ItemCustomization (EE 39).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   19   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9F  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Outcome; 0 sealed, 1 cracked, 2 abandoned, 3 timed out (as abandoned), 4 seal refused (no attempt running or nothing struck; nothing else is filled). |
| 5 | 1 | Byte |  | Flags; 1 the attunement (no recipe), 2 the smith's first completion of the recipe. |
| 6 | 2 | ShortLittleEndian |  | Recipe; The recipe's number (on a crack, the one the path was on); 0 for the attunement. |
| 8 | 1 | Byte |  | Rarity; 0 Common to 5 Mythic (Common on a crack). |
| 9 | 1 | Byte |  | Extension; The links past an extendable shape. |
| 10 | 1 | Byte |  | Dominant; The dominant element (0 fire to 4 light), 0xFF none. |
| 11 | 1 | Byte |  | Fate; The item's fate when not sealed: 0 the ingredients are lost, 1 the item is downgraded, 2 the item is destroyed; 0xFF when sealed. |
| 12 | 5 | Binary |  | ElementMix; The struck stars' values summed by element: fire, frost, storm, shadow, light. |
| 17 | 1 | Byte |  | CleanStrikes |
| 18 | 1 | Byte |  | StrikeCount |