# C1 EE F0 - ForkHello (by client)

## Is sent when

After the login succeeded, when the client logged in with the version of the Babylon client.

## Causes the following actions on the server side

The server answers with the ForkWelcome which tells the client the wire format and the features which are active.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   12   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xF0  | Packet header - sub packet type identifier |
| 4 | 2 | ShortLittleEndian |  | ClientWire; The version of the Babylon protocol which the client speaks. |
| 6 | 4 | IntegerLittleEndian |  | WantedFeatures; A bit per feature (see docs/babylon.md) which the client wants to use on this world. |
| 10 | 2 | ShortLittleEndian |  | ClientBuild; The build number of the client, for the logs. |