# C1 EE 9D - StarSmithForgeAttempt (by server)

## Is sent when

The answer to StarSmithForgeStart: the attempt, or why there is none.

## Causes the following actions on the client side

The client starts the beat and the drawing over the item. The faults are never sent, except the one a Jewel of Guardian reveals.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   179   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x9D  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Result; 0 started; else the StarSmithResult codes (1 no hammer, 2 hammer broken, 3 locked, 4 not here, 5 no such item, 8 off), 12 an attempt is already running, 13 a slot is wrong (empty, used twice, the hammer or the item), 14 no ingredients. Only the result and the target slot are filled when not 0. |
| 5 | 1 | Byte |  | TargetSlot |
| 6 | 1 | Byte |  | Strikes; How many nodes may be placed. |
| 7 | 1 | Byte |  | Risk; The worst fate among the attempt's constellations after protection: 0 the ingredients are lost, 1 the item is downgraded, 2 the item is destroyed. |
| 8 | 1 | Byte |  | FreeSlots; The constellation slots still free on this item. |
| 9 | 1 | Byte |  | Slots; The smith's slots by level, 1 to 4. |
| 10 | 2 | ShortLittleEndian |  | BeatIntervalMs |
| 12 | 2 | ShortLittleEndian |  | FirstBeatMs; The time from sending this packet to the first beat, in milliseconds. |
| 14 | 1 | Byte |  | Flags; 1 a fault is revealed. |
| 15 | 8 | Binary |  | RevealedFault; x1, y1, x2, y2 in box units (u16 little endian each); zero when none. |
| 23 | 2 | ShortLittleEndian |  | FaultRadius; In box units. |
| 25 | 2 | ShortLittleEndian |  | CloseRadius; In box units. |
| 27 | 2 | ShortLittleEndian |  | NodeGap; In box units. |
| 29 | 1 | Byte |  | FaultCount |
| 30 | 1 | Byte |  | LayerCount; The item's constellation layers, 0 to 4. |
| 31 | 148 | Binary |  | Layers; 4 layers of 37 bytes: number (u16), tier, look, hull point count, 8 points (x, y u16 each); unused zero. |