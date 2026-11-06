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
| `s21_skills` | The skills of Season 21: the nine new classes, the fourth and fifth evolution skills, the master tree nodes Season 21 adds, the books (update "Add the skills of Season 21") |
| `enhance_tree` | The skill enhancement tree of the fourth evolution and its points per progression mode (`src/GameLogic/EnhanceTree`) |
| `evolution_quests` | The fourth and fifth class quests, the Trial Area and the experience hold at the fourth evolution (update "Add the evolution quests of Season 21", `src/GameLogic/EvolutionQuests`) |
| `elite_zones` | The five elemental elite zones of the maps from Atlans Abyss 3 and Scorched Canyon on, with their elites inside (update "Add the elite zones of Season 21", plugin "Elite zones", `src/GameLogic/EliteZones`) |
| `world_bosses` | The field bosses Medusa, Lord Silvester, Core Magriffy, Lord of Ferea, Nix and the God of Darkness with the shared 18 to 24 hour respawn, stages, arenas, item bags and `/fieldboss` (update "Add the field bosses of Season 21", plugin "Field bosses", `src/GameLogic/FieldBosses`) |

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
  22 `artifacts`, 23 `legendary_craft`, 24 `evolution_quests`, 25-31 reserved. A bit is available when `fork.json`
  lists the id of the same name. `fork_protocol` (no bit) says the fork speaks this protocol at all.
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
Helper: `GameServer/RemoteView/Babylon/ClassCodes.cs` (`ToCode`, `FromCode`), on top of `GameLogic/Classes/ClassCodes.cs`.

Character packets (characters range 10-1F):

- `C1 EE 10 CreateCharacterWide` (client, 15): name (10) at 4, class code at 14. Creates any class the world has;
  failures answer with the Season 6 creation failure.
- `C1 EE 11 CharacterCreatedWide` (server, 16): name at 4, slot at 14, class code at 15.
- `C1 EE 12 CharacterClassCodes` (server): count at 4, then (slot, class code) pairs; sent right after the
  Season 6 character list, which a Babylon client gets with every character (a Season 6 client gets no
  character of a new class, and a 4th or 5th evolution as its 3rd).

Appearance packets (range 20-2F):

- `C2 EE 20 PlayerAppearanceWide` (server): player id (u16 BE) at 5, class code at 7, item count at 8, then per item
  slot (u8) + 32 bytes of item data in the connection's item format (W32). Sent right after the Season 6 packet
  which adds the player to the view (normal and transformed).
- `C1 EE 21 AppearanceChangedWide` (server, 40): player id at 4, slot at 6, class code at 7, 32 bytes item data at 8
  (0xFF when the slot got empty). Replaces the Season 6 appearance change for the Babylon client, whose glow byte
  would overwrite a part of the W32 item number.

Skill packets (range 40-4F). A Season 6 client keeps every Season 6 skill packet; the use packets (targeted, area)
carry the skill number in 16 bits and stay as they are for both clients.

- `C2 EE 40 EnhanceTreeState` (server): free points (u16 LE) at 5, spent points at 7, count at 9, then per learned tree
  skill its number (u16 LE) and level (u8). Sent after the skill list to a fourth or fifth evolution while the tree is on.
- `C1 EE 41 EnhanceTreeAddPoint` (client, 6): skill number (u16 LE) at 4.
- `C1 EE 42 EnhanceTreeAddPointResult` (server, 10): result at 4 (0 added, 1 no tree, 2 unknown node, 3 requirement
  missing, 4 not enough points, 5 maximum reached), skill number at 5, level at 7, free points at 8.
- `C2 EE 46 MasterSkillListWide` (server): free master points (u16 LE) at 5, count at 7, then per skill its number
  (u16 LE), level (u8) and the display values of the level and the next one (floats). Replaces the Season 6 master
  list, which names a slot of the Season 6 client tree that the classes after Season 6 don't have.
- `C2 EE 48 SkillListWide` (server): count (u16 LE) at 5, then per skill its list index (u16 LE), number (u16 LE) and
  level (u8). Replaces the Season 6 list, whose byte count in a C1 packet holds at most 62 skills. Single skills are
  added and removed with the Season 6 packets.
- 43-45 are kept for the Ability Enhancement Core, 47 for an effect status with a 16-bit effect number (the Season 6
  status has a byte; no effect above 255 is applied yet).

### Quests (range C0-CF)

The evolution quests use the quest packets of Season 6 (F6: list of the NPC, start, progress, completion), group 21,
numbers 7-12 (S21Emu `Quest/Quest.xml`). On top of them, for the Babylon client:

- `C1 EE C4 EvolutionQuestState` (server, 10): next evolution at 4 (3 fourth, 4 fifth, 0xFF none), rule met at 5
  (bool), experience held at 6 (bool), level key at 7 (1 level + master level, 2 power level, 3 Season 21 total),
  required level (u16 LE) at 8. Sent on every map entry, after a completed quest and when the hold starts.
- `C1 EE 13 ClassEvolved` (server, 8, characters range): player id (u16 BE) at 4, class code at 6, reason at 7
  (0 quest, 1 game master, 2 item). Sent to the player and every player who sees it.

World packets (range A0-AF). A Season 6 client gets none of them; it sees the bosses and the announcements through the
Season 6 packets (golden notices for a spawn, a kill and a lost battle, blue notices at the arena).

- `C2 EE A0 EliteZones` (server): map number (u16 LE) at 5, count at 7, then per zone 8 bytes: element (1 fire, 2 water,
  3 earth, 4 wind, 5 darkness), entrance x, y, area x1, y1, x2, y2, can enter (0/1). Sent when the character enters a map
  with zones.
- `C1 EE A1 FieldBossState` (server, 14): map number (u16 LE) at 4, boss number (u16 LE, the last stage) at 6, status at 8
  (0 waiting, 1 alive, 2 in a fight, 3 defeated), health percent of the current stage at 9, seconds left (u32 LE) at 10:
  to the spawn, to the end of the battle or to the move out of the arena (0 unknown). Sent on entering the boss map and
  on every change (spawn, stage, each tenth of health, kill, lost battle, back to waiting).
- A2-A4 stay kept for mining and the invasion notice.

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
