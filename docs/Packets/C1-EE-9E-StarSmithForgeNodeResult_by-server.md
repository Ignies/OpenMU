# C1 EE 9E - StarSmithForgeNodeResult (by server)

## Is sent when

The answer to StarSmithForgeNode, or to a stash (StarSmithForgeSeal action 1).

## Causes the following actions on the client side

The client draws the node (or the crack), the live match, the guide and the accuracy per node; a first find plays the "Constellation of {Element} found." popup. A refusal changes nothing.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   107   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9E  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind; 0 node, 1 stash. |
| 5 | 1 | Byte |  | Answer; Node: 0 placed, 1 cracked, 2 no attempt, 3 no strikes left, 4 too near a node, 5 figure full. Stash: 0 banked, 2 no attempt, 6 no match, 7 overlap, 8 no free slot. |
| 6 | 1 | Byte |  | Timing; 0 clean, 1 shaky, 2 late; 0xFF on a refusal and on a stash. |
| 7 | 1 | Byte |  | StrikesLeft |
| 8 | 1 | Byte |  | Flags; 1 the node closed the figure, 2 it hit a fault, 4 the figure's hull overlaps a used hull, 8 a first find, 16 the match counts its extension, 32 the match is a reseal of the item's layer. |
| 9 | 1 | Byte |  | FigurePlacements |
| 10 | 1 | Byte |  | Banked |
| 11 | 2 | ShortLittleEndian |  | Match; The constellation the figure matches (on a stash, the banked one); 0 none. |
| 13 | 1 | Byte |  | Look |
| 14 | 1 | Byte |  | Tier; 0 Faint to 5 Chroma; 0xFF none. |
| 15 | 2 | ShortLittleEndian |  | Score |
| 17 | 2 | ShortLittleEndian |  | Turn; The node's measured turn, signed tenths of a degree (two's complement), left positive. |
| 19 | 2 | ShortLittleEndian |  | Ratio; The node's measured length ratio, permille of the first segment. |
| 21 | 1 | Byte |  | GuideCount |
| 22 | 16 | Binary |  | Guide; 8 known constellations (u16) the figure is a prefix of; unused zero. |
| 38 | 36 | Binary |  | Placements; 12 x 3 bytes per placement of the figure: timing, accuracy (u16; 0xFFFF none). |
| 74 | 1 | Byte |  | HullCount |
| 75 | 32 | Binary |  | Hull; 8 points, x and y u16 each, in box units; unused zero. |