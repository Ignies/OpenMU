# C1 EE 85 - PentagramErrtels (by server)

## Is sent when

After a PentagramErrtelsRequest, and after an errtel was put into or taken out of the pentagram.

## Causes the following actions on the client side

The client shows the errtels of the pentagram's sockets.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x85  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | PentagramSlot |
| 5 | 1 | Byte |  | ErrtelCount |
| 6 | SocketedErrtel.Length * ErrtelCount | Array of SocketedErrtel |  | Errtels |

### SocketedErrtel Structure

An errtel in a socket of a pentagram, with its rank options.

Length: 14 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | SocketIndex |
| 1 | 2 | ShortLittleEndian |  | ErrtelNumber; The item number of the errtel (group 12). |
| 3 | 1 | Byte |  | RankCount |
| 4 | 10 | Binary |  | RankOptions; Five pairs of option number and level, by rank; 0xFF 0 for a rank the errtel doesn't have. The option numbers are the S21 errtel option indexes, as in the W32 slots of an errtel. |