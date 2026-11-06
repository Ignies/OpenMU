# C1 EE 13 - ClassEvolved (by server)

## Is sent when

A character in the view of a Babylon client (or its own) evolved into its next class.

## Causes the following actions on the client side

The client shows the evolution and the body of the new class.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x13  | Packet header - sub packet type identifier |
| 4 | 2 | ShortBigEndian |  | PlayerId |
| 6 | 1 | Byte |  | ClassCode; The class code of the new class. |
| 7 | 1 | Byte |  | Reason; 0 quest, 1 game master, 2 item. |