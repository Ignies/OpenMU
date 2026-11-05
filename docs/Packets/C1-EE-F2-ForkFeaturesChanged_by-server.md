# C1 EE F2 - ForkFeaturesChanged (by server)

## Is sent when

After the active features of the connection changed while the client is connected.

## Causes the following actions on the client side

The client turns the features on or off.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   8   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xF2  | Packet header - sub packet type identifier |
| 4 | 4 | IntegerLittleEndian |  | ActiveFeatures; The features which are active from now on. |