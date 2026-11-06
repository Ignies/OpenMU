# C1 EE 59 - MuunExchangeRequest (by client)

## Is sent when

The player takes an offer of the Muun exchange NPC.

## Causes the following actions on the server side

The server takes the items, puts the evolution stone into the bag of the Muun inventory and answers with MuunActionResponse (action 5).

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x59  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | OfferIndex |