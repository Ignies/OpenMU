# C1 EE BA - ArcaWarRequest (by client)

## Is sent when

A guild master registers the guild for the Arca War, a member registers, or a registered member enters the war.

## Causes the following actions on the server side

The server answers with ArcaWarRequestResponse; an entry warps the character to the spawns of its guild.

## Structure

| Index | Length | Data Type | Value | Description |
|-------|--------|-----------|-------|-------------|
| 0 | 1 |   Byte   | 0xC1  | [Packet type](PacketTypes.md) |
| 1 | 1 |    Byte   |   5   | Packet header - length of the packet |
| 2 | 1 |    Byte   | 0xEE  | Packet header - packet type identifier |
| 3 | 1 |    Byte   | 0xBA  | Packet header - sub packet type identifier |
| 4 | 1 | Byte |  | Kind; 0 register the guild (master), 1 register as a member, 2 enter the war. |