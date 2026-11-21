# C1 EE 33 - ItemCustomizationRequest (by client)

## Is sent when

Players come into view or change their appearance, the own inventory changes, or a window with another storage (vault, trade, personal shop, Gremory case) shows its items. At most 32 places; one request is out at a time.

## Causes the following actions on the server side

The server answers with ItemCustomizations, covering exactly the asked places.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |      | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0x33  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Count |
| 5 | ItemCustomizationAsk.Length * Count | Array of ItemCustomizationAsk |  | Asks |

### ItemCustomizationAsk Structure

A place whose items the client asks the customisation of.

Length: 3 Bytes

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 | Byte |  | Storage; 0 worn by the object, 1 the own inventory (worn and bag), 2 the own vault, 3 the trade offer of the object, 4 the personal shop of the object, 5 the own Gremory cases (object id 0 the character's, 1 the account's), 6 an item on the ground in view (object id its drop id, slot 0). |
| 1 | 2 | ShortBigEndian |  | ObjectId; The player the storage belongs to; the own id for the own storages. |