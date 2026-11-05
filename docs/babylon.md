# Ignies/OpenMU: the fork marker

This repository is the OpenMU of Hidden-Babylon: upstream MUnique/OpenMU master plus the Season 21 server work and
fixes. The shipped line is the **master of Ignies/OpenMU**, at the commit the Hidden-Babylon installer pins.
Upstream changes come in by a deliberate merge of upstream/master, tested, then a new pin.

A world can run this fork or plain upstream. Everything that uses fork-only data (Season 21 switches, plugin glue,
the client) first checks the marker below; on upstream the Season 21 switches stay hidden and off.

## Contract (marker version 1)

The marker exists in two places which always say the same (a test, `ForkInfoTest`, keeps them equal):

1. `fork.json` in the repository root, for installers and websites that see the source (a clone, or
   `https://raw.githubusercontent.com/Ignies/OpenMU/<commit>/fork.json`). Upstream has no such file.

   ```json
   {
     "fork": "Ignies/OpenMU",
     "line": "master of Ignies/OpenMU",
     "markerVersion": 1,
     "repository": "https://github.com/Ignies/OpenMU",
     "features": ["s21_maps", "..."]
   }
   ```

2. `MUnique.OpenMU.GameLogic.ForkInfo` in the running server, for plugins: `Name`, `Line`, `MarkerVersion`,
   `Features` and `Has(id)`. Upstream has no such type, so a plugin which must run on both looks it up:
   `Type.GetType("MUnique.OpenMU.GameLogic.ForkInfo, MUnique.OpenMU.GameLogic")` is `null` on upstream.
   The client learns it from the Hidden-Babylon glue plugin, which reads this type and sends it on.

Rules:

- `markerVersion` changes only when a field changes its meaning or is removed. Adding a feature id is not a change
  of the contract.
- Feature ids match `^[a-z][a-z0-9_]{0,39}$` (the installer drops others), are listed once and are never reused
  for something else. A reader treats an unknown id as absent and a missing file or type as "upstream".
- A feature id says the code and the optional data updates are present. Whether a world uses them is its own
  configuration (the applied updates and the world's switches).

## Features

| Id | What the fork carries |
|---|---|
| `s21_maps` | The maps after Season 6 (update "Add the maps of the later seasons") |
| `level_caps` | The level caps of Season 21 (400 / 1350) |
| `s21_gear` | The Season 21 gear a Season 6 class can use |
| `s21_drops` | The per-monster drop lists of the Season 21 maps |
| `s21_event_grades` | The Season 21 event grades by total level (update "Set the event grades and party bonus of Season 21") |
| `party_bonus` | The configurable party experience bonus (plugin "Party experience bonus") |
| `lucky_items` | The lucky items, David's ticket exchange, the jewels of extension and elevation |
| `lucky_refinery_s21` | The refinery rule of Season 21: 60 % at full durability, else 10 %, always a jewel of extension |
| `progression` | The progression core (`src/GameLogic/Progression`): the world's progression setting |
| `fork_protocol` | The Babylon protocol, wire 1: the `21B01` login, the 0xEE handshake (section "Protocol") |
| `wire_w32` | The W32 item format for the Babylon client (section "W32") |
| `class_codes` | The one-byte class code of the Babylon packets (section "Class code"); numbers and helpers in `src/GameLogic/Classes` |
| `s21_items_all` | Every item of the Season 21 data (update "Add every item of Season 21"); what the Season 6 item format can't carry is defined but inactive |
| `classes_s21` | The nine classes of Season 21 with all their evolutions (update "Add the classes of Season 21") |
| `evolutions_45` | The fourth and fifth evolution of the Season 6 classes and their level rules per progression mode |

## Balance

The Season 21 data keeps the original numbers. A computed normalization layer (a world setting choosing
original, normalized or stars) is planned separately; it is meant to read the original rows and scale them at
runtime, so the data and the corrections here stay the source of truth.

## Protocol (wire 1)

Fork worlds speak an extended protocol with the Babylon client; a Season 6 client never sees any of it.

- **Login.** The Babylon client in fork mode sends the version bytes `21B01` (Season 6: `10404`). The game server
  resolves them to `ClientVersion(21, 1, English)` (`GameServer/BabylonClient.cs`, registered in code: a
  `GameClientDefinition` would add endpoints). That client gets every plugin of the Season 6 English client plus
  the ones marked `[MinimumClient(21, 1, Invariant)]` (a test keeps that true).
- **Handshake.** Right after the login succeeded the client sends `C1 EE F0 ForkHello`; the server answers
  `C1 EE F1 ForkWelcome`. Without a welcome within 5 s, or with `ServerWire` 0, the client reconnects in Season 6
  mode. Packets: `src/Network/Packets/Babylon/BabylonPackets.xml` (generated docs: `docs/Packets/Babylon.md`).

```
C1 0C EE F0  ForkHello (client)          C1 10 EE F1  ForkWelcome (server)
  4  u16 LE  ClientWire (1)                4  u16 LE  ServerWire (1)
  6  u32 LE  WantedFeatures                6  u32 LE  ActiveFeatures = wanted AND carried (W32 bit dropped
  10 u16 LE  ClientBuild                              when the W32 serializer is not active)
                                           10 u8      ItemFormat: 0 Season 6 (12 bytes), 1 W32 (32 bytes)
C1 08 EE F2  ForkFeaturesChanged (server)  11 u8      ClassEncoding: 0 Season 6, 1 class code
  4  u32 LE  ActiveFeatures                12 u16 LE  MaximumLevel (the world's normal cap)
                                           14 u16 LE  MaximumStars (0 none, 0xFFFF no limit)
```

- **Feature bits** (`GameLogic/ForkFeatures.cs`, appended only): 0 `wire_w32`, 1 `class_codes`, 2 `classes_s21`,
  3 `evolutions_45`, 4 `enhance_tree`, 5 `ability_core`, 6 `mastery`, 7 `wing_grades`, 8 `sockets_10`,
  9 `pentagram_elements`, 10 `earrings`, 11 `muun`, 12 `gremory`, 13 `ruud`, 14 `mining`, 15 `elite_zones`,
  16 `world_bosses`, 17 `events_s21`, 18 `muquest`, 19 `helper_plus`, 20 `ranking`, 21 `battle_pass`,
  22 `artifacts`, 23 `legendary_craft`, 24-31 reserved. A bit is available when `fork.json` lists the id of the
  same name. `fork_protocol` (no bit) says the fork speaks this protocol at all.
- **Head code 0xEE**, always with a sub code (not 0xFB: the Babylon proxy relays its map pings on C1 FB, and FA-FD are its own), reserved by range so tracks never collide: F0-F7 core, 10-1F
  characters, 20-2F appearance, 30-3F items, 40-4F skill trees, 50-5F Muun, 60-6F Gremory Case, 70-7F currency
  and shops, 80-8F elements and pentagram, 90-9F crafting, A0-AF world, B0-BF events, C0-CF quests, D0-DF social
  and ranking, E0-EF MU Helper Plus. A track adds its packets to `BabylonPackets.xml` in its first commit.
  Handlers: `GameServer/MessageHandler/Babylon/` (`BabylonGroupHandlerPlugIn` and one sub handler per packet).

### Class code

`classCode = base << 3 | evolution` (u8): base 0-15 in Season 21 order (0 DW, 1 DK, 2 FE, 3 MG, 4 DL, 5 SU, 6 RF,
7 GL, 8 RW, 9 SL, 10 GC, 11 LW, 12 LM, 13 IK, 14 AL, 15 CS), evolution 0-4 = 1st to 5th class, 5-7 and bit 7
reserved. The Season 6 classes keep their `CharacterClass.Number` (base << 2 | generation); every other class and
evolution has the number 128 + its code. MG, DL and RF have no 2nd class: their master class is evolution 2.
Helper: `GameServer/RemoteView/Babylon/ClassCodes.cs` (`ToCode`, `FromCode`).

### W32

`GameServer/RemoteView/ItemSerializerW32.cs`, active for the Babylon client (`NeededSpace` 32). Every packet which
sizes its items from `NeededSpace` carries it as it is; the player shop list and buy result go through their
extended packets (`ShowShopItemListW32PlugIn`, `PlayerShopBuyRequestResultW32PlugIn`). Every field Season 6 knows
keeps its Season 6 meaning; the fields of later seasons stay zero until their systems exist.

| Byte | Field |
|---|---|
| 0 | group (0-255) |
| 1-2 | number, u16 LE |
| 3 | item level (0 for Dark Horse and Dark Raven) |
| 4 | durability (Dark Horse, Dark Raven: their level) |
| 5 | flags: 0 skill, 1 luck, 2 excellent bits not zero, 3 ancient, 4 harmony, 5 380 option, 6 sockets, 7 timed |
| 6 | option level (bits 0-3, 0-15), option kind (bits 4-7) |
| 7-8 | u16 LE: bits 0-5 = the low six bits of Season 6 byte 3 (excellent options; wings: options and option kind; Fenrir: its kind), 6-12 mastery options, 13-15 reserved |
| 9 | mastery bonus option: index (bits 0-3), group (bits 4-7) |
| 10 | ancient: set discriminator (bits 0-1), bonus level (bits 2-3) |
| 11 | harmony: level (bits 0-3), option (bits 4-7) |
| 12 | socket bonus option (0xFF: a socket item without bonus) |
| 13-17 | five slots: socket option index over all elements (0-49), 0xFE empty, 0xFF no slot |
| 18-20 | five slot levels, 4 bits each (slot 0 = byte 18 low nibble): sphere level 1-15 |
| 21 | element (bits 0-3), element kind (bits 4-7) |
| 22-27 | kind block (4th/5th wing grades, earring links, Muun, errtel) |
| 28-31 | expiry, u32 LE Unix seconds UTC, 0 = never |
