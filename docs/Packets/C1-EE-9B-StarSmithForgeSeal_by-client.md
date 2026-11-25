# C1 EE 9B - StarSmithForgeSeal (by client)

## Is sent when

The smith seals the attempt or stashes the figure being drawn.

## Causes the following actions on the server side

A seal applies the result and answers with StarSmithForgeResult (refused, outcome 4, without a running attempt or before the first node); a stash answers with StarSmithForgeNodeResult (kind 1). A 4-byte packet is a seal.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9B  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Action; 0 seal, 1 stash. |