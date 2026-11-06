# C2 EE 58 - MuunExchangeList (by server)

## Is sent when

The player talked to the Muun exchange NPC (Monica, 682).

## Causes the following actions on the client side

The client opens the exchange with its offers.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC2  | [Packet type](PacketTypes.md) |
| 1 | 2 |    Short   |      | Packet header - length of the packet |
| 3 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 4 | 1 |    Byte   | 0x58  | Packet header - sub packet type identifier |
| 5 | 1 | Byte |  | OfferCount |
| 6 | MuunExchangeOfferEntry.Length * OfferCount | Array of MuunExchangeOfferEntry |  | Offers |

### MuunExchangeOfferEntry Structure

An offer of the Muun exchange NPC.

Length: 10 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Index; The index of the offer, which the exchange request refers to. |
| 1 | 1 | Byte |  | CostGroup |
| 2 | 2 | ShortLittleEndian |  | CostNumber |
| 4 | 1 | Byte |  | CostCount |
| 5 | 1 | Byte |  | ResultGroup |
| 6 | 2 | ShortLittleEndian |  | ResultNumber |
| 8 | 2 | ShortLittleEndian |  | TargetNumber; The Muun which the evolution stone evolves (item group 16); 0xFFFF for none. |