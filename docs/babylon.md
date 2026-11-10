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
| `classes_s21` | The nine classes of Season 21 with all their evolutions and the stat formulas of the S21Emu character calculation (update "Add the classes of Season 21", version 2) |
| `evolutions_45` | The fourth and fifth evolution of the Season 6 classes and their level rules per progression mode |
| `s21_skills` | The skills of Season 21: the nine new classes, the fourth and fifth evolution skills, the master tree nodes Season 21 adds, the books (update "Add the skills of Season 21") |
| `enhance_tree` | The skill enhancement tree of the fourth evolution and its points per progression mode (`src/GameLogic/EnhanceTree`) |
| `evolution_quests` | The fourth and fifth class quests, the Trial Area and the experience hold at the fourth evolution (update "Add the evolution quests of Season 21", `src/GameLogic/EvolutionQuests`) |
| `muquest` | The quest guide: 186 steps (164 for each class) from level 1 to the third class (update "Add the guide quests of Season 21", plugin "Guide quests (Season 21)", `src/GameLogic/GuideQuests`) |
| `gens` | The gens families Duprian and Vanert of upstream's `src/GameLogic/Gens` (plugin "Gens system"), named so the Babylon client and Admin > Systems can switch the Gens window: join from level 50 at the stewards 543 and 544, contribution from kills in the battle zones, 14 ranks (8 by ranking position), the monthly jewelry case rewards of ranks 1 to 8 (S21Emu `GameServerInfo - Common.dat` GensSystem*, `EventItemBagManager.txt` 102-105) |
| `elite_zones` | The five elemental elite zones of the maps from Atlans Abyss 3 and Scorched Canyon on, with their elites inside (update "Add the elite zones of Season 21", plugin "Elite zones", `src/GameLogic/EliteZones`) |
| `world_bosses` | The field bosses Medusa, Lord Silvester, Core Magriffy, Lord of Ferea, Nix and the God of Darkness with the shared 18 to 24 hour respawn, stages, arenas, item bags and `/fieldboss` (update "Add the field bosses of Season 21", plugin "Field bosses", `src/GameLogic/FieldBosses`) |
| `wide_items` | Per-client item visibility: what only the W32 format carries never reaches a Season 6 client; the waiting items of the catalog activated for fork clients (update "Activate the items of the wide item format", section "Wide items") |
| `mastery` | The mastery options and mastery bonus options of the 890 mastery items (update "Add the mastery options") |
| `wing_grades` | The grade options of the fourth and fifth wings (update "Add the grade options of the fourth and fifth wings") |
| `sockets_10` | The sphere levels 6 to 10 and the seed sphere enhancement (update "Add the sphere levels 6 to 10") |
| `item_expiry` | Timed items: `Item.ExpiresAt`, removed by the plugin "Item expiry" from the inventory, vault and gremory cases (`src/GameLogic/ItemExpiry`) |
| `gremory` | The gremory case of Season 21: the character and account reward storage, claimed into the inventory within 7 days; mini game item rewards and `/gremgif` go into it (plugin "Gremory case", `src/GameLogic/GremoryCase`) |
| `ruud` | Ruud (`Character.Ruud`), the Ruud shop of Priest James with the 200 rows of Season 21, the Ruud of 64 monsters and 10 Ruud boxes, `/setruud` (update "Add the gremory case, Ruud and the Ruud shop of Season 21", plugin "Ruud", `src/GameLogic/Ruud`) |
| `npc_shop_tabs` | Merchant stores with up to five tabs (`MonsterDefinition.MerchantStoreTabs`; the tab 0 is the Season 6 store) |
| `s21_shops` | The 29 NPC shops of Season 21 with their tabs, 1772 rows (update "Set the shops of Season 21") |
| `zen_sinks` | The zen sinks of Season 21: 1 % tax on the zen sales of personal stores, monsters drop 50 % of the money (plugin "Zen sinks of Season 21") |
| `s21_mixes` | The mastery item mixes, the fourth wings combination and the wing core enhancement, the grade rates (update "Add the item mixes of Season 21", plugin "Item rules of Season 21") |
| `level_rewards` | The level rewards of Season 21: a Muun at every 50 total levels from 950 to 1750, into the gremory case (update "Add the level rewards and event contribution boxes of Season 21", plugin "Level rewards of Season 21", `src/GameLogic/Rewards`) |
| `event_contribution` | The event contribution points of Season 21 (blood castle, chaos castle, devil square, illusion temple, imperial guardian, doppelganger) and the Mastery Boxes they give at the end of an event, into the gremory case (same update, plugin "Event contribution boxes of Season 21") |
| `pentagram_elements` | The elements: elemental damage and defense of characters and monsters, pentagrams, errtels and the elemental refinery (update "Add the elements, pentagrams and errtels", plugin "Elements (Season 21)", section "Elements, pentagrams and earrings") |
| `earrings` | The earrings with their slots, options and links (update "Add the earrings", same section) |
| `muun` | The Muuns of Season 21: the Muun inventory with two equipped Muuns, their options on the owner, level up, evolution, the energy converter, Monica's exchange and the mounts (update "Add the Muuns of Season 21", plugin "Muuns of Season 21", `src/GameLogic/Muuns`) |
| `mining` | The Pandora mining: 51 spots in the safe zones of six maps, five stages, Jewels of Bless (update "Add the mining of Season 21", plugin "Mining (Season 21)", `src/GameLogic/Mining`) |
| `golden_invasions_s21` | The golden invasion of Season 21: the ten later golden monsters 493-502 with their Box of Kundun, the Golden Derkon in Elbeland, 19 kinds every four hours from 00:25 for 10 minutes (update "Add the golden invasion of Season 21"; the Season 6 plugin "Golden Invasion" runs it) |
| `ranking` | The hunting record (time, kills, experience and damage per character, map and day on the 59 hunting maps, kept 7 days) and the in-game ranking by total level and by the kills and hunting experience of the week (update "Add the hunting record of Season 21", plugin "Hunting record and ranking (Season 21)", `src/GameLogic/HuntingRecord`) |
| `soul_converter` | The monster soul converter: five tiers (items 14/460-464, total level 400 to 1450), two soul missions drawn from the tier, souls absorbed by kills and shared in a party, the reward kinds Ruud, jewels, elemental options and accessories, three a week, expiring on Thursday (update "Add the monster soul converter of Season 21", plugin "Monster soul converter (Season 21)", `src/GameLogic/SoulConverter`) |
| `helper_plus` | The MU Helper Plus: the rules of the helper for the Babylon client (the plus mode from total level 400, alone or as party leader, its ranges and cost in percent of the helper cost) and the moves of the plus mode into the 330 hunting zones of the 31 hunting grounds, with the level and zen of their warps (update "Add the MU Helper Plus of Season 21", plugin "MU Helper Plus (Season 21)", `src/GameLogic/HelperPlus`) |
| `party_instances` | The party instances of Season 21: a copy of a map for each party with its own monsters and timer (`MiniGameType.PartyInstance`); the trial of the fourth and fifth class quest runs in one (update "Add the party instances of Season 21", `src/GameLogic/MiniGames/Readme.md`) |
| `doppelganger_s21` | The Doppelganger of Season 21: seven rooms by total level with their multipliers, experience and chests, two players at least (update "Renew the Doppelganger for Season 21", `src/GameLogic/MiniGames/Readme.md`) |
| `tormented_square` | The Tormented Square of Season 21: map 102, fifteen rounds of monsters, points and ranks, by its timetable (update "Add the Tormented Square of Season 21", plugins "Events of Season 21" and "Tormented Square start"; event type 9 of the event packets) |
| `instance_zone` | The Instance Zone of Season 21 (map 114): the entrances of the fourth and fifth class trials and of the labyrinth (update "Add the Instance Zone and the Labyrinth of Dimensions of Season 21") |
| `labyrinth` | The Labyrinth of Dimensions of Season 21 (map 115): a solo instance of 41 floors with Ruud and boxes (same update; event type 10) |
| `labyrinth_league` | The scores and weekly leagues of the labyrinth: floor points by the daily record, first play and fast finish, a Maze of Dimensions box per floor, groups of five per tier (Bronze, Silver, Gold, Legend), two up and the fifth down every week, the tier reward on the 29th (update "Add the leagues of the Labyrinth of Dimensions of Season 21", plugin "Labyrinth of Dimensions league (Season 21)", off by default; `C1 EE B5 LabyrinthLeague`) |
| `labyrinth_rooms` | The 10 x 10 rooms of the labyrinth floors: 30 paths of rooms which show areas of 38 other maps, portals between them, mission rooms (kills, kills in time, damage, MU Helper time, no health or mana potion) which open their mission portals, the main mission which clears the floor, the goblin in newly opened rooms (update "Add the rooms of the Labyrinth of Dimensions of Season 21", plugin "Labyrinth of Dimensions rooms (Season 21)", off by default; only a Babylon client plays them; `C1 EE B6`-`B9`) |
| `arca_war` | The Arca War of Season 21 (maps 92 and 96): guild masters register (1 minute), then members (1 minute), a 30-minute guild war over three obelisks (Fire Tower 598, 500,000 health, 300,000 once held, back after 20 s) whose shield drops for a guild standing on its three auras; 5 points a monster, 10 an enemy, 50 an obelisk, experience by points, Trophies of Battle, a week's buff of the element and the reward of Acheron (effect 187, +20 % experience, +10 % item drops there) for the guilds holding obelisks at the end, a fee of 10 Signs of Lord, a team damage reduction setting and the trophy exchange at Sir Lesnar (update "Add the Arca War of Season 21" version 2, plugin "Arca War (Season 21)", off by default; `C1 EE BA`-`BD`, `src/GameLogic/ArcaWar/Readme.md`) |
| `chaos_castle_survival` | The Chaos Castle Survival of the Fittest of Season 21 (map 97, gate 443): a Chaos Castle for up to 70 players from level 280 whose games are the try-outs (8th-10th), semi-finals (11th-12th) and final (13th, 1:25) of a monthly tournament; 1 point a monster, 2 a player, 3 the win; the best 20 and 10 go on; the Conqueror's Badge for the champion (update "Add the Chaos Castle Survival of Season 21", plugins "Chaos Castle Survival of the Fittest (Season 21)" and "Chaos Castle Survival start", off by default; event type 13 of the event packets, the castle itself shows the Chaos Castle states) |
| `boss_battle` | The Boss Battle Together of Season 21 (map 139): one of five bosses for everybody, rewards for damage, top damage and the last hit (update "Add the Boss Battle Together of Season 21"; event type 11) |
| `acheron_guardian` | The Protector of Acheron of Season 21 (maps 92 and 96): 30 waves of monsters, a bag reward for 100 points (update "Add the Protector of Acheron of Season 21"; event type 12) |
| `imperial_guardian_s18` | The Imperial Guardian rework of Season 18: from total level 1000, artifact rewards at the completion (update "Apply the Imperial Guardian rework of Season 18") |
| `crywolf_s21` | The Crywolf renewal of Season 21: the army takes the element of the week, Balgass at 6:40, the rewards into the gremory case, the Season 21 timings (update "Apply the Crywolf renewal of Season 21") |
| `battle_pass` | The season pass (MU Pass): the monthly seasons of the data repeated every year, 20 levels, 25 daily missions and one special mission (their kind set by the operator), the free, gold and premium pass with their rewards (buffs until the season ends, items into the gremory case), the Exp, Gold and Premium Pass Tickets (update "Add the season pass of Season 21", plugin "Season pass (Season 21)", `src/GameLogic/SeasonPass`) |
| `stats_specialize` | The stat specialization of Season 21: a stat of a class raises an option by the percent of its section (S21Emu `Character/StatsSpecialize.txt`; plugin "Stat specialization (Season 21)", off by default, `src/GameLogic/Classes`). No packet: the raised values reach the client through the Season 6 stat packets |
| `creatable_classes` | `C1 EE 14 CreatableClasses` after the class codes of the list: every class the creation offers with the state of the server's creation rule (`GameLogic/Classes/ClassCreationRule.cs`) and its creation level |
| `level_penalties` | The damage and experience penalties of Season 21 below a monster's level + 600 (S21Emu `DamagePenalty.lua`, `ExperienciePenalty.lua`; plugin "Level penalties (Season 21)", off by default, levels mapped through the progression mode, `src/GameLogic/Progression`). No packet |
| `ability_core` | The Ability Enhancement Core of the fifth evolution: 45 skill, option, elite and extra damage cards in 22 slots (`Character.AbilityCore`, migration `AddSeason21AbilityCore`), their options as item options by card level, the fifth evolution skills of the skill cards, level up and reduction, card crafting at the chaos goblin (update "Add the Ability Enhancement Core of Season 21", plugin "Ability Enhancement Core (Season 21)", off by default; slot levels mapped through the progression mode; `src/GameLogic/AbilityCore`; `EE 43`-`45`) |
| `illusion_temple_league` | The Illusion Temple League of Season 21 (maps 98 and 99, event type 14 of `EE B1`, level 1 or 2): the renewed temple (the Cursed Statue 658 gives the Cursed Hollow, its carrier seals it into one of the ten Captured Stone Statues 659-668 for a point, the other team unseals a stone to take it back, 7 points or the most at the end win) for the parties of two guilds (5 members of one guild, total level 400 through the progression mode); eliminations on the 3rd-9th (5 matches a guild), ranked by occupied stones, kills and level sum; the best four play the semi-finals on the 11th (first against fourth, second against third), their winners the final on the 12th; the players of the champion in the final get the Wings of Conqueror (12/268) from the 13th (update "Add the Illusion Temple League of Season 21", plugins "Illusion Temple League (Season 21)" and "Illusion Temple League start", off by default; `src/GameLogic/MiniGames/Readme.md`; `EE BE`-`BF`) |
| `s21_bag_items` | The uses of the items of group 19 (update "Add the uses of the Season 21 items of group 19"): the Pandora Box (the Pandora Pick, fireworks), Red and Blue Card (potions, keys, jewels, blessings), Premium Muun Box (179 Muuns at level 1) and Red Card [Elemental] (one pentagram, one errtel or seed sphere, one excellent armor piece and 50-250 Ruud) open by use (`C1 26` item use; S21Emu `EventItemBag` 294-297, 308; one item per bag section next to the player; needs "Item rules of Season 21"); David's lucky exchange takes the [Lvl. 0] tickets 19/4-13 like 13/135-144 at level 0 with the option rolled (`LuckyTicketRolls.RollLevelOfLevelZeroTickets`); the [Bound][Hero] Pentagram Chaos Combination Talisman 19/16 and the Elemental Talisman of Chaos 14/190 keep the pentagram or errtel of a failed elemental upgrade, level up or rank up, the Elemental Talisman of Luck 14/189 adds 10 %; the Blessing of Eunice 19/17 gives 50 % more experience for 30 minutes to the fifth evolution (effect 447, sent to the Babylon client as `EE 32 MagicEffectStatusWide`) |

## Game master commands

Chat commands of the fork for testing its systems with one or two clients (minimum status `GameMaster`).

| Command | What it does |
|---|---|
| `/siege <register\|mark\|notify\|ready\|start\|end>` | Forces the Castle Siege state at its next tick, like Force State of the admin panel (not persisted: a restart starts at the guild registration again) |
| `/siege crown` | Crown test on or off: the two switches are waived, so one attacker alone at the Crown captures it (crown, life stone and ruins with one client); a restart or the cycle end (EndCycle) ends it |
| `/labyrinth [floor]` | Outside the Labyrinth of Dimensions: enters a run of its own without ticket, level or daily limit checks, at the floor when given (1-41); inside: clears the current floor (its rewards count) and goes on with the given floor or the next one, without the pause |
| `/itleague <elim\|semi\|final\|reward\|off>` | Plays the Illusion Temple League stage on any day (`IllusionTempleLeaguePlugIn.ForcedStage`, not persisted) and force-starts the league temples (not for `reward`); while forced the temples open at every time of the timetable; `off` gives the stages back to the days of the month |
| `/item ... sck= seeds= errtels= opts= muun=` | The Season 21 options of `/item` (all optional; with named arguments the group and number are named too: `/item group=12 number=200 errtels=0:3:5 opts=element:1`). `sck=<n>` sockets (default the maximum of the item); `seeds=<index>[:<level>],...` the seeds of the sockets in order, the index over all elements as the W32 item carries it (0-49), sphere level 1-10 (default 1); `errtels=<socket>:<ranks>[:<level>],...` errtels in the sockets of a pentagram (socket 0 anger to 4 radiance, 1-5 ranks of the first errtel of that kind, level 0-10), its pentagram options follow; `opts=<kind>:<number>[:<level>],...` options of the item by kind `mastery`, `bonus` (mastery bonus), `element`, `grade` (wing grade) or `earring`; `muun=<days>` starts the additional option of a Muun. An entry the item can't have is named and nothing is made. Lucky and bound are properties of the item definition (lucky item set, `IsBoundToCharacter`), not of the item: there is no flag to set |
| `/startts`, `/startbb`, `/startag` | Force-start the Tormented Square, the Boss Battle Together and the Protector of Acheron like `/startbc` (`StartMiniGameEventChatCommandPlugInBase`): a running one is stopped, the entrance opens now; refused on a game server where the event's start plugin is off |
| `/getruud [character]` | Shows the Ruud of a character, the own without a name (`/setruud <character> <amount>` sets it) |

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
- `C1 EE 14 CreatableClasses` (server): count at 4, then per class 4 bytes: class code (u8), state (u8), creation
  level (u16 LE, 0 for none). Sent right after `EE 12`, so with every character list; lists every class with
  `CanGetCreated`, by code. State, by `ClassCreationRule` (the rule the server enforces on a creation): 0 creatable,
  1 needs another character of the account at the level (refused), 2 needs the Season 6 unlock flag (the client
  checks it, as in the original; the unlock comes with the level or a card), 3 unavailable in this world (inactive
  class or home map; refused). A client without it (an older fork) keeps the Season 6 unlock flags. After a delete
  the list is not sent again; a client asks for it (`C1 F3 00`) to refresh the states.

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
- `C2 EE 43 AbilityCoreState` (server): open slots (u32 LE, bit n the slot n) at 5, active slots (u32 LE, the slots whose
  card takes effect) at 9, count at 13, then per card its slot (u8) and 32 bytes of item data (W32; the item level is the
  card level, its option the card option). Sent on every map join of a world with the core and after every successful
  core action. Slots: 0 extra damage, 1-3 skill, 4-18 option (three rows of five: blue, green, purple), 19-21 elite.
- `C1 EE 44 AbilityCoreRequest` (client, 7): action at 4 (0 insert: inventory slot into core slot; 1 remove: core slot
  into the inventory slot, 0xFF the first free one; 2 level up: the card of the inventory slot, or of the core slot when
  the inventory slot is 0xFF; 3 reduce: the card of the inventory slot), core slot at 5, inventory slot at 6.
- `C1 EE 45 AbilityCoreResponse` (server, 40): result at 4 (0 done, 1 off, 2 not a fifth evolution, 3 no item, 4 not a
  card or no level up / reduction for it, 5 slot locked, 6 wrong slot: kind, row color or class, 7 slot occupied,
  8 requirement missing: the fourth evolution skill of a skill card or the five option cards of an elite's row,
  9 inventory full, 10 highest level, 11 not enough cards, 12 not enough zen, 13 not enough stones, 14 elite level above
  the lowest option card of its row), action at 5, core slot at 6, inventory slot at 7, the card after the action (the
  stones after a reduction) at 8 (W32, 0xFF none). The inventory side gets the Season 6 item removed / added / upgraded
  packets. A card option of the item is the item option type "Ability Card Option" (number = the S21Emu option index,
  the names of the client table ParticleOptionText).
- Card crafting (chaos goblin, update "Add the Ability Enhancement Core of Season 21" version 2, only while the core is
  on): the client puts the crystals (20/164 or bound 20/238, mixed) and the jewels of bless (14/13) into the mix box and
  sends the mix request `C1 86` with the crafting number; all at 100 % (Webzen guide 343, S21Emu ChaosMix.dat):

  | Crafting number | Mix | Result |
  |---|---|---|
  | 180 | 50 crystals, 10 bless, 100 M zen | a skill card of the class (random among them), one HP/MP/AG option |
  | 181-185 | 5 crystals, 1 bless, 10 M zen | an option card of type 1-5 in a random color, one random option |
  | 186-188 | 25 crystals, 5 bless, 50 M zen | the blue / green / purple elite card, one random option |
  | 189 | 25 crystals, 5 bless, 50 M zen | 5 Ability Upgrade Stones (20/239) |

  The card is level 0; its option is drawn by the option weights (skill cards 3 / 6 / 1 from AbilityAddOption.xml).
  The amounts are exact (more is "too many items"). The same items fit several numbers, so the number is required.
- 47 is kept for an effect status with a 16-bit effect number (the Season 6 status has a byte; no effect above 255 is
  applied yet).

### Quests (range C0-CF)

The evolution quests use the quest packets of Season 6 (F6: list of the NPC, start, progress, completion), group 21,
numbers 7-12 (S21Emu `Quest/Quest.xml`). On top of them, for the Babylon client:

- `C1 EE C4 EvolutionQuestState` (server, 10): next evolution at 4 (3 fourth, 4 fifth, 0xFF none), rule met at 5
  (bool), experience held at 6 (bool), level key at 7 (1 level + master level, 2 power level, 3 Season 21 total),
  required level (u16 LE) at 8. Sent on every map entry, after a completed quest and when the hold starts.
- `C1 EE 13 ClassEvolved` (server, 8, characters range): player id (u16 BE) at 4, class code at 6, reason at 7
  (0 quest, 1 game master, 2 item). Sent to the player and every player who sees it.

The guide quests (feature `muquest`, update "Add the guide quests of Season 21", `src/GameLogic/GuideQuests`) have no
NPC and no Season 6 packet; only the Babylon client sees them:

- `C1 EE C0 GuideQuestState` (server, 84 + 4 per item): status at 4 (0 none, 1 offered, 2 active, 3 ready, 4 level
  too low, 5 finished), result at 5 (0 none, 1 accepted, 2 completed, 3 not ready, 4 level too low), step number at 6,
  index at 8, count at 10 (u16 LE each), chapter at 12, objective at 13 (1 collect, 2 hunt, 3 level, 4 class quest,
  5 move), minimum level (u16) at 14, target (u32) at 16, item number (u16) at 20, required (u16) at 22, progress
  (u16) at 24, map (u16) at 26, x at 28, y at 29, gate (u16) at 30, experience (u32) at 32, money (u32) at 36, name
  (40 bytes UTF-8) at 40, item count at 80, then from 84 per item group, count, number (u16). Sent on every map
  entry, level up, kill or arrival that moves the step, and as the answer to a request.
- `C1 EE C1 GuideQuestRequest` (client, 8): action at 4 (0 show, 1 accept, 2 complete), the step the client shows
  (u16 LE) at 6.

World packets (range A0-AF). A Season 6 client gets none of them; it sees the bosses and the announcements through the
Season 6 packets (golden notices for a spawn, a kill and a lost battle, blue notices at the arena).

- `C2 EE A0 EliteZones` (server): map number (u16 LE) at 5, count at 7, then per zone 8 bytes: element (1 fire, 2 water,
  3 earth, 4 wind, 5 darkness), entrance x, y, area x1, y1, x2, y2, can enter (0/1). Sent when the character enters a map
  with zones.
- `C1 EE A1 FieldBossState` (server, 14): map number (u16 LE) at 4, boss number (u16 LE, the last stage) at 6, status at 8
  (0 waiting, 1 alive, 2 in a fight, 3 defeated), health percent of the current stage at 9, seconds left (u32 LE) at 10:
  to the spawn, to the end of the battle or to the move out of the arena (0 unknown). Sent on entering the boss map and
  on every change (spawn, stage, each tenth of health, kill, lost battle, back to waiting).
- `C1 EE A2 MiningRequest` (client, 7): spot object id (u16 BE) at 4, action at 6 (0 mine the next stage, 1 stop and collect).
- `C1 EE A3 MiningResult` (server, 12): spot object id (u16 BE) at 4, result at 6 (0 opened, 1 stage mined, 2 failed and the pending
  jewels lost, 3 collected, 4 no pickaxe, 5 pickaxe worn, 6 no spot in reach, 7 another player mines it, 8 off), stage at 7,
  jewels (u16 LE) at 8, jackpot (u16 LE) at 10. Sent when the character talks to a spot (result 0) and after each request.
- A4 stays kept for the invasion notice.
- `C2 EE A5 SoulConverterState` (server): tier at 5 (0 none), activations left this week at 6, expiry (u32 LE, UTC seconds, 0 none)
  at 7, count at 11, then per mission 8 bytes: map, monster, souls, required souls (u16 LE each). Sent on activation, on
  every absorbed soul, after a claim, on entering a map with an active converter and on `C1 EE A8 SoulConverterStateRequest`.
- `C1 EE A6 SoulConverterRewardRequest` (client, 5): reward kind at 4 (0 Ruud, 1 jewels, 2 elemental options, 3 accessory).
  Answered by `C1 EE A7 SoulConverterResult` (6): result at 4 (0 rewarded, 1 missions not done, 2 no converter, 3 kind not
  offered, 4 off, 5 not delivered), reward kind at 5.

Social and ranking packets (range D0-DF):

- `C1 EE D3 RankingRequest` (client, 6): kind at 4 (0 total level, 1 monster kills of the week, 2 hunting experience of the
  week), page from 0 at 5. Answered by `C2 EE D0 RankingPage`: kind at 5, page at 6, page count at 7, own rank (u16 LE, 0
  not ranked) at 8, count at 10, then per entry 17 bytes: rank (u16 LE), name (10), class code, value (u32 LE).
- `C1 EE D4 HuntingRecordRequest` (client, 6): map number (u16 LE, 0xFFFF the current map) at 4. Answered by
  `C2 EE D1 HuntingRecordList`: map number (u16 LE) at 5, count at 7, then per day, newest first, 28 bytes: day start
  (u32 LE, UTC seconds), seconds on the map, kills (u32 LE each), experience and damage (u64 LE each).
- `C2 EE D2 SeasonPassState` (server): season (u16 LE, 0 none runs) at 5, end (u32 LE, UTC seconds) at 7, level at 11,
  experience (u32 LE) at 12, passes at 16 (bit 0 free, 1 gold, 2 premium), claimed levels of the free, gold and premium pass
  (u32 LE each, bit 0 = level 1) at 17, 21 and 25, count at 29, then per mission 6 bytes: index (bit 7 a special mission),
  completions, progress (u32 LE). Sent on entering a map, on every gain, after a claim and on `C1 EE D7 SeasonPassStateRequest`.
- `C1 EE D5 SeasonPassClaim` (client, 6): pass at 4 (0 free, 1 gold, 2 premium), level at 5. Answered by
  `C1 EE D6 SeasonPassClaimResult` (7): pass, level, result at 6 (0 claimed, 1 level not reached, 2 already claimed, 3 pass
  not owned, 4 no reward, 5 not delivered, 6 off).

MU Helper Plus packets (range E0-EF):

- `C1 EE E0 HelperPlusRules` (server, 13): flags at 4 (bit 0 the map has hunting zones, bit 1 the character may use the plus
  mode, bit 2 the plus mode is on), plus mode total level (u16 LE) at 5, hunting and pick-up range of the helper at 7 and 8,
  of the plus mode at 9 and 10, plus mode cost in percent (u16 LE) at 11. Sent on every map entry and after `C1 EE E3`.
- `C1 EE E3 HelperPlusModeRequest` (client, 5): plus mode on (bool) at 4.
- `C1 EE E1 HelperPlusMoveRequest` (client, 6): zone index (u16 LE, the `Index` of a `MuHelperPlus.xml` zone) at 4. Answered
  by `C1 EE E2 HelperPlusMoveResult` (7): zone index at 4, result at 6 (0 moved, 1 unknown zone, 2 level too low, 3 not
  enough money, 4 plus mode off, 5 off, 6 can not move now).

### Muuns (range 50-5F)

The Muun system (`src/GameLogic/Muuns`, plugin "Muuns of Season 21", off by default). A Season 6 client gets none of these
packets and has no Muun system: Muuns are wide items, so it never sees one (test `MuunPacketsTest.OnlyTheBabylonClientGetsTheMuunView`).
Storages in the packets: 0 the inventory, 1 the Muun inventory (slot 0 main Muun, 1 sub Muun, 2-101 bag, 102 hero slot). Results: 0 done,
1 off, 2 no item, 3 wrong material, 4 highest level, 5 level too low, 6 no evolution, 7 not enough items, 8 not at the
exchange NPC, 9 no free slot, 10 riding not allowed here, 11 no mount, 12 slot occupied, 13 not a Muun, 14 the same Muun is equipped already, 15 slot locked (the hero slot before its total level, 1750 by default, or a Muun which is no hero Muun). Item data is 32
bytes in the connection's item format (W32); 0xFF for no item.

- `C2 EE 50 MuunInventoryList` (server): count at 5, then per item 33 bytes: slot (u8), item data (32). Sent on every map entry.
- `C1 EE 51 MuunItemMove` (client, 8): from storage at 4, from slot at 5, to storage at 6, to slot at 7. Only a Muun goes into
  slot 0 or 1, only items of the group 16 into the Muun inventory. Answered by `C1 EE 52 MuunItemMoveResponse` (server, 41):
  result at 4, the four fields at 5-8, item data at its new place at 9. The inventory side also gets the Season 6 item removed /
  item added packets.
- `C1 EE 53 MuunRideRequest` (client, 5): ride (bool) at 4. Only a mount (kind 4) of the main slot, not in Chaos Castle or
  Illusion Temple.
- `C1 EE 54 MuunItemUse` (client, 9): action at 4 (1 level up: a Muun of level 1 of the same kind; 2 evolve: its evolution stone,
  at its highest level; 3 charge energy: a Muun or an evolution stone on a Muun energy converter; 4 restore: a jewel of life),
  source storage and slot at 5-6, target storage and slot at 7-8. The source item is used up.
- `C1 EE 55 MuunActionResponse` (server, 40): action at 4 (1-4 as above, 4 also when an equipped Muun lost health, 5 exchange,
  6 ride), result at 5, storage at 6, slot at 7, item data of the changed or created item at 8.
- `C1 EE 56 MuunAppearance` (server, 11): player id (u16 BE) at 4, item group of the main Muun at 6 (0xFF none), number (u16 LE)
  at 7, level at 9, riding (bool) at 10. Sent with the appearance of a player who comes into view, and to the player and
  everyone who sees it when its main Muun changes or it mounts or dismounts.
- `C2 EE 57 MuunOptions` (server): count at 5, then per equipped Muun 5 bytes: slot, option type (MuunOption.xml OptionType),
  value (u16 LE, with the additional option), flags (bit 0 applies now, bit 1 raised by the additional option). Sent on map
  entry and when an option changes.
- `C2 EE 58 MuunExchangeList` (server): count at 5, then per offer 10 bytes: index, price group, number (u16 LE), count, result
  group, number (u16 LE), the Muun of the evolution stone (u16 LE, 0xFFFF none). Sent when the character talks to Monica (682).
- `C1 EE 59 MuunExchangeRequest` (client, 5): offer index at 4. Answered by EE 55 (action 5); the stone lands in the Muun bag.
- `C1 EE 5A MuunSkillUsed` (server, 13): player id (u16 BE) at 4, monster id (u16 BE) at 6, skill level at 8, damage (u32 LE) at 9.
  Sent to the owner and everyone who sees it when a Muun with an attack skill option hits the nearest monster (every
  10 s by default, less the cooldown reduction of its additional option).

### Events (range B0-BF)

The events and party instances of Season 21 share five packets, and a sixth that asks a state (the B range is full, so
it sits in the world range: `C1 07 EE A9 EventStateRequest` (client), event type u16 LE at 4, game level at 6; the server
answers `EventState` for that level: open with the seconds until the entrance closes, playing with 0, or closed with the
seconds until the next start (0 when the event has no start plugin or timetable there); a level the event lacks gets no
answer, `EventStateRequestAction`); the event type is the server's `MiniGameType`
(8 party instance, 9 Tormented Square, 10 Labyrinth of Dimensions, 11 Boss Battle Together, 12 Protector of Acheron,
13 Chaos Castle Survival, 14 Illusion Temple League). A Season 6 client gets none of them: it is told in words.

```
C1 0C EE B0  EventState (server)          C1 07 EE B1  EventEnterRequest (client)
  4  u16 LE  event type                     4  u16 LE  event type (8 and up; the Season 6 events keep their packets)
  6  u8      game level                     6  u8      game level
  7  u8      0 open, 1 closed, 2 playing, 3 ended
  8  u32 LE  seconds to the next state    C1 07 EE B4  EventEnterResult (server)
                                             4  u16 LE  event type
C1 14 EE B2  EventScore (server)             6  u8      0 success, 1 failed, 2 not open, 3 level too high,
  4  u16 LE  event type                                 4 level too low, 5 full, 6 not enough money, 7 player killer
  6  u8      round / floor / stage
  8  u32 LE  score (damage in the boss battle)  C1 14 EE B3  EventResult (server)
  12 u16 LE  rank (0 unranked)                   4  u16 LE  event type, 6 bool success
  14 u16 LE  players                             8  u32 LE  score, 12 u16 LE rank
  16 u32 LE  seconds left                        16 u32 LE  experience of the reward
```

A class quest trial step (event type 8) sends `EventScore` with the quest number (7-12) as the round and the step's time
left: when it starts, when it's asked again and with 0 seconds when it ended (the original's "Quest Survival Time").

```
C1 0E EE B5  LabyrinthLeague (server, on entering the labyrinth and after the run; fork id labyrinth_league)
  4  u8      tier: 0 Bronze, 1 Silver, 2 Gold, 3 Legend
  5  u8      rank in the group of the week (0 no points this week)
  6  u8      characters in the group
  7  u8      day: 0 no league day, 1 first day of a week, 2 play day, 3 reward day
  8  u32 LE  points of the week
  12 u16 LE  floors cleared today
```

```
C1 2C EE B6  LabyrinthRoomBoard (server; fork id labyrinth_rooms)    C1 05 EE B8  LabyrinthRoomMoveRequest (client)
  4  u8      path (1-30)                                             4  u8      0 top, 1 bottom, 2 left, 3 right
  5  u8      room: row * 10 + column
  6  u16 LE  map whose area the room shows                         C1 06 EE B9  LabyrinthRoomMoveResult (server)
  8  u8 x4   start x, start y, end x, end y of the area              4  u8      direction
  12 u8      room kind: 1 entrance, 2 normal, 3 mission, 4 main      5  u8      0 moved, 1 no portal or room,
  13 u8      area number (EventMaps ID)                                         2 mission not done, 3 not in rooms
  14 u8 x4   portals top, bottom, left, right:
             0 closed, 1 open, 2 opens with the mission
  18 13 B    opened rooms, bit (i % 8) of byte (i / 8)
  31 13 B    rooms whose mission is done

C1 08+12n EE B7  LabyrinthRoomMission (server)
  4  u8      room kind, 5 u8 mission index, 6 bool done, 7 u8 goal count n
  8  n x 12  goal: 0 u8 kind (1 kill, 2 kill the room's monsters in time, 3 damage, 4 helper seconds,
             5 / 6 like 2 without health / mana potions, 7 experience), 4 u32 LE target (monsters, minutes,
             damage, seconds), 8 u32 LE progress (kills, damage or seconds since the mission (re)started)
```

The board comes when a floor starts (in its entrance room), after each move and when a mission is done; the mission
with every room and on progress (damage and helper time at most once a second). The client draws the area of the map as
the room; the server keeps only that area walkable on the run's copy of map 115 and moves the character to the side of
the room it came through. A client without these packets plays the labyrinth floors without rooms.

```
C1 05 EE BA  ArcaWarRequest (client; fork id arca_war)        C1 06 EE BB  ArcaWarRequestResponse (server)
  4  u8      0 register the guild (master),                     4  u8      request kind
             1 register as a member, 2 enter the war             5  u8      0 success, 1 not open, 2 no guild,
                                                                            3 not the master, 4 already registered,
C1 10 EE BC  ArcaWarStageState (server)                                     5 too many guilds, 6 guild not registered,
  4  u8      0 closed, 1 master registration, 2 member                      7 too many members, 8 not registered,
             registration, 3 waiting, 4 war, 5 closing                      9 not enough Signs of Lord
  5  u8      registered guilds, 6 bool registered
  8  u32 LE  seconds to the next stage, 12 u32 LE points

C1 08+24n EE BD  ArcaWarObelisks (server, once a second to the players on maps 92 and 96 during the war)
  4  u8      obelisk count n
  8  n x 24  0 u8 index, 1 u8 element (1 fire, 2 water, 3 earth, 4 wind, 5 darkness), 2 u16 LE map, 4 u8 x, 5 u8 y,
             6 u8 health %, 7 u8 auras the viewer's guild stands on (bit i), 8 bool shield down for the viewer's guild,
             12 u32 LE holder guild id (0 none), 16 8 B holder guild name
```

The state comes to everyone at each stage change, after a request and on entering the world while the war isn't closed;
the Season 6 client gets the stage changes as golden messages and can't register (no packet). Monster 598 is a
destructible the client draws as the obelisk; its shield (`IsOpen`) is down only for the guild on all three auras.

```
C1 14 EE BE  IllusionTempleLeagueState (server; fork id illusion_temple_league)
  4  u8      stage of the day: 0 none, 1 eliminations, 2 semi-finals, 3 final, 4 reward day
  5  u8      rank of the guild in the eliminations (0 none)
  6  u8      elimination matches of the guild, 7 u8 the most a guild may play (0 no limit)
  8  u16 LE  occupied stones, 10 u16 LE kills, 12 u32 LE level sums of the eliminations
  16 u8      0 none, 1 semi-final in temple 1, 2 semi-final in temple 2, 3 finalist, 4 champion
  17 u8      elimination matches of the character, 18 u8 the most a character may play (0 no limit)

C1 08+4n EE BF  IllusionTempleStones (server)
  4  u8      stone count n (10), 5 u8 own team (1 allied, 2 illusion forces)
  6  u8      Cursed Hollow: 0 in the Cursed Statue, 1 carried, 2 on the ground, 3 none (the statue comes back)
  8  n x 4   0 u8 index (0-9, NPC 659-668), 1 u8 holder (0 none, 1 allied, 2 illusion), 2 u8 x, 3 u8 y
```

The league state comes when a character of a guild enters the world on a league day, when a league match starts and
after it ended; the stones at the battle start and whenever a stone is sealed or unsealed or the hollow changes hands.
The league temples are entered with `EE B1` (type 14, level 1 or 2); inside, the Season 6 Illusion Temple packets (`BF`)
carry the time, the score, the skills and the result as in the old temple, with the occupied stones as the score. A
Season 6 client gets the rules as golden messages (no stones view).

Item rewards go into the gremory case where Season 21 does (and the plugin is on), else into the inventory.

Sir Lesnar (NPC 584 in Lorencia 114/132, Devias 240/53, Noria 183/126) has the window `NpcDialog`: the client draws
its own dialog, with the requests of `EE BA` and the exchange of the Trophies of Battle (13/147), which is the standard
mix: the client moves 1 to 100 trophies into the mix box (`C3 24` ItemMoveRequest) and sends `C1 05 86` with mix 53; the
result is the standard `C1 86` answer. The rate is 50 % for the first trophy of each ten plus 5 % for each further one;
the item comes from the bag of the ten (S21Emu bags 153-162). The registration fee takes 10 Signs of Lord (14/21
level 3) from the master's inventory; a master without them gets result 9.

### Economy (ranges 30-3F, 60-7F)

The gremory case, Ruud, the merchant store tabs and the item expiry (`src/GameLogic/GremoryCase`, `Ruud`, `ItemExpiry`,
`ZenSinks`). A Season 6 client gets none of these packets: its rewards keep their Season 6 way (inventory or ground), Ruud
reaches it as a message, and a merchant shows it the first tab only, which is the Season 6 store (test
`EconomyPacketsTest.OnlyTheBabylonClientGetsTheEconomyViews`). Item data is 32 bytes in the connection's item format (W32);
W32 carries the expiry at bytes 28-31 and flag bit 7.

- `C1 EE 31 ItemExpired` (server, 9): storage at 4 (0 inventory, 1 vault, 2 character case, 3 account case), slot at 5, item
  group at 6, item number (u16 LE) at 7. The removal itself follows the usual packet (item removed, case item expired).
- `C1 EE 32 MagicEffectStatusWide` (server, 13): active at 4, object id (u16 BE) at 5, effect number (u16 LE) at 7, remaining
  seconds (u32 LE, 0 when removed or the duration isn't sent) at 9. A magic effect numbered past 255 (a later season's
  effect, S21Emu `Effect.txt`, such as the Blessing of Eunice 447), sent where Season 6 sends `C1 07` (plugin "Babylon:
  magic effects past a byte"); a Season 6 client gets no packet for it, as its byte would name another effect. OpenMU's
  own invisible effects 200-255 stay unsent (`MagicEffectsList`).
- `C2 EE 60 GremoryCaseList` (server): kind at 5 (0 character, 1 account), capacity at 6, count at 7, then per reward 37 bytes:
  slot (u8), expiry (u32 LE, Unix seconds UTC), item data (32). Sent for both cases when the character enters a map and on
  `C1 EE 65 GremoryCaseListRequest` (client, 4).
- `C1 EE 61 GremoryCaseItemAdded` (server, 42): kind at 4, slot at 5, expiry (u32 LE) at 6, item data at 10.
- `C1 EE 62 GremoryCaseClaim` (client, 6): kind at 4, slot at 5. Answered by `C1 EE 63 GremoryCaseClaimResponse` (server, 7):
  kind at 4, slot at 5, result at 6 (0 claimed, 1 inventory full, 2 no such reward, 3 expired, 4 off). A claimed item appears
  in the inventory by the Season 6 packet and doesn't expire anymore.
- `C1 EE 64 GremoryCaseItemExpired` (server, 6): kind at 4, slot at 5.
- `C1 EE 70 RuudChanged` (server, 12): Ruud (u32 LE) at 4, change (i32 LE) at 8, 0 when it's just shown. Sent on map entry,
  when the Ruud shop opens and on every change.
- `C2 EE 71 RuudShopList` (server): count at 5, then per item 12 bytes: index (u16 LE, the purchase refers to it), group,
  number (u16 LE), level, count (0 the default durability), flags (bit 0 may get luck, bit 1 may get the option), price in
  Ruud (u32 LE). Sent when the character talks to the Ruud NPC (Priest James, 725), with the items of its class.
- `C1 EE 72 RuudShopBuy` (client, 6): index (u16 LE) at 4. Answered by `C1 EE 73 RuudShopBuyResponse` (server, 11): result
  at 4 (0 bought, 1 not enough Ruud, 2 inventory full, 3 no such item for the class, 4 shop closed), index at 5, Ruud at 7.
- `C1 EE 74 MerchantStoreTabs` (server, 6): tab count at 4, active tab at 5. Sent after the store item list of a merchant
  with more than one tab, and before the store item list (Season 6 `C2 31`) of a selected tab.
- `C1 EE 75 MerchantStoreTabSelect` (client, 5): tab at 4. The server answers with EE 74 and the store item list of the tab;
  purchases (Season 6 `C1 32`) then refer to the slots of that tab (OpenMU issue 653: the tab 0 is the Season 6 store).

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
| 7-8 | u16 LE: bits 0-5 = the low six bits of Season 6 byte 3 (excellent options; wings: options and option kind; Fenrir: its kind), bits 6-12 = the mastery options by their number (6-12), 13-15 reserved |
| 9 | mastery bonus option: level 1-3 (bits 0-3), group (bits 4-7: 0 weapon, 1 armor); 0 = none |
| 10 | ancient: set discriminator (bits 0-1), bonus level (bits 2-3) |
| 11 | harmony: level (bits 0-3), option (bits 4-7) |
| 12 | socket bonus option (0xFF: a socket item without bonus) |
| 13-17 | five slots: socket option index over all elements (0-49), 0xFE empty, 0xFF no slot |
| 18-20 | five slot levels, 4 bits each (slot 0 = byte 18 low nibble): sphere level 1-10 (1-15 representable) |
| 21 | element (bits 0-3), element kind (bits 4-7), see "Elements, pentagrams and earrings" |
| 22-27 | kind block. Fourth and fifth wings: three grade options, each option number (u8, 0-10, 0xFF none) and grade (u8, 0-3); errtels and earrings: see "Elements, pentagrams and earrings". Muun: the end of its additional option at 22-25 (u32 LE, Unix seconds UTC, 0 not started); its level is byte 3, its health byte 4; its play time (minutes, for the play time condition) is server side only (`Item.PetExperience`). Evolution stone: the Muun it evolves at 22-23 (u16 LE, item group 16). Muun energy converter: its energy is the durability (byte 4). The others stay zero |
| 28-31 | expiry, u32 LE Unix seconds UTC, 0 = never |

## Wide items

`GameLogic/WideItems/WideItemRules.cs` decides what only the W32 format carries (feature `wide_items`):

- an item definition of the item groups above 15 or with a number above 511, or one marked with the option
  definition "Wide Item Format" (option type `Season21ItemOptionTypes.WideFormat`: Muuns, earrings, pentagrams,
  errtels, the fourth and fifth wings, timed items, quivers, seed spheres of levels 6-10);
- an item with a mastery, mastery bonus or wing grade option, or a socket option above sphere level 5.

The item serializer of a connection tells what it carries (`IItemFormatViewPlugIn.CarriesWideItems`: W32 yes, every
Season 6 or older format no); `player.CanSee(item)` combines both. A player without a client view (a bot) sees every
item. A Season 6 client never gets a wide item, and gets no stand-in for it:

| Place | Season 6 client |
|---|---|
| Inventory, vault, NPC shop, chaos machine, player shop list | the item is left out of the list |
| Item appeared, moved, upgraded, bought | nothing is sent |
| Ground | the drop is left out (no packet when nothing is left); a pick-up by its id is refused |
| Trade | the partner can't put it into the trade ("can't be traded") and never sees it |
| NPC shop, player shop buy | refused as an unknown item |
| Crafting result | the item data is 0xFF ("no item") |
| Other players' gear | the slot is empty in the appearance; an equip change reads as "removed" |
| Monster drops | a killer whose client can't carry them never gets a wide item or a wide option rolled |
| Admin "give item" | refused with the reason |

The marketplace bridge of Hidden-Babylon (`openmu/plugins`, outside this repository) uses the same rule:
`WideItemRules.IsWideOnly(item)` before it offers an item to a Season 6 world's site.

The update "Activate the items of the wide item format" sets the 1,187 waiting items of the catalog active and marks
those of the groups 0-15. They drop by the drop lists of the later maps (for fork clients only), can be sold by a
merchant an operator gives them to, and can be given by a game master to a Babylon client.

### Options (W32 only)

- **Mastery** (`mastery`, update version 2): one mastery option per item (always) and a bonus option by 25 %.

  | Tier | Weapon: damage per 20 levels (option 6) | Weapon: damage (option 7) |
  |---|---|---|
  | Bloodangel | +1.5 | +42 |
  | Darkangel | +1.6 | +45 |
  | Holyangel | +1.8 | +51 |
  | Soul | +2.0 | +57 |
  | Blue Eye | +2.2 | +62 |
  | Silver Heart | +2.4 | +68 |
  | Manticore | +2.6 | +74 |
  | Brilliant | +2.8 | +80 |
  | Apocalypse | +3.0 | +85 |

  Sources: Webzen guides "1st" to "9th Mastery Weapons" (muonline.webzen.com/en/gameinfo/guide/detail/131-136, 261,
  294, 330); muonlinefanz.com "Mastery Weapons" agrees for Bloodangel, Darkangel and Holyangel. Both add to the attack
  and the wizardry (`Stats.BaseDamageBonus`).
  Armors, every tier: health + 165, mana + 165, damage decrease 45 (flat, new `Stats.DamageDecreaseAbsolute`, taken
  off every received hit after the defense), zen + 45 %. Sources: S21Emu MasteryOptionRate.xml (165 / 165 / 45 / 45 %)
  and muonlinefanz.com item db (Bloodangel and Holyangel armors: 165 / 165 / 45, zen 42 %); the older Webzen
  "1st Mastery Armors" page (Season 14) lists 300 / 180 / 195 / 50 %, which the later data replaced; the S21 data wins.
  Option 10 (defense + total level / 20) is a designed number: no source gives its value. Elemental defense (11) and
  obsidian (12) wait for the element system.
  Bonus options: weapons all stats + 10 / 25 / 40 (S21Emu MasteryOption.xml and the Webzen mastery bonus guide;
  muonlinefanz.com says + 20 for the middle one), armors damage decrease 25 / 50 / 75 (S21Emu, muonlinefanz.com).
  Version 1 had one weapon option set for all tiers (+30, + level / 10) and the damage decrease as defense; version 2
  removes that set from the weapons and retargets the armor options.
- **Wing grades** (`wing_grades`): eleven options (ItemGradeOption.xml): full mana / health recovery, damage return,
  ignore defense, attack speed, excellent damage, double damage chance, strength, vitality, energy, agility. Fourth
  wings up to three of them in grades 0-3 (level = grade + 1), fifth wings at their single grade.
- **Who wears the fourth and fifth wings** (update "wing grades", version 2): the class ranks of Item.xml; rank r is
  the class from its evolution r on, for the Season 6 classes and the classes of Season 21. Example: Wings of Flame God
  (12/415) for the Dark Knight from the fourth evolution.
- **Grade rates**: a grade option gets its grade by the weights 947500 / 30000 / 15000 / 7500 (grades 0-3, S21Emu
  ItemGradeOption.xml RateList; configuration of the plugin "Item rules of Season 21"), on drops and on the fourth
  wings combination. The first grade option always comes, up to two more by 50 % each (designed: no source gives the
  count).
- **Sphere levels 6-10** (`sockets_10`): every socket option gets the levels 6-10, its Season 6 level 5 value scaled by
  the Season 21 values of SocketItemOption.xml (a multiplier scales its bonus part). The seed spheres 12/311-340
  (level 6-10) can be mounted; the seed researcher enhances a seed sphere of level 5-9 with 7 / 8 / 9 / 10 / 11 sphere
  upgrade runes (12/401) and 25 / 28 / 31 / 34 / 37 million zen (SocketSystemCombinationMix.xml) to the next level.
  These are the crafting numbers 100-104 of the Season 6 mix request (`C1 86`); a client which can't carry the result
  gets "incorrect mix items".

### Mixes (update "Add the item mixes of Season 21", chaos goblin)

| Crafting number | Mix | Source |
|---|---|---|
| 110 | Mastery item combination: 2,476 mixes (unsealing, tier upgrades, mastery jewelry), found by the items; success rate, zen and what the result keeps per mix | S21Emu MasteryItemMix.xml (`Generator/season21-mixes.mjs` -> `Resources/Season21/MasteryMixes.json`); the 6 mixes with a random result bag are left out |
| 110 | Socket item upgrade: 118 mixes (ids 9201+), a socket item +11 to +15 with two or more sockets, all empty, a Socket Upgrade Note (14/364), 5 Bless, 5 Soul and a Chaos give the next socket item +0; 80 %, 10 M zen, skill and luck 20 % each, 1 to 3 sockets (4/23: 1 to 5); a failure keeps the item | S21Emu SocketSystemCombinationMixUpgrade.xml (`Season21MixData.SocketUpgradeMixes`, code mixes of the item mixes update) |
| 111 | Fourth wings: Garuda's Feather, Garuda's Flame, Golden Crest x50, jewels of bless, soul, chaos and creation x30 each (as packed jewels x30), a higher magic stone; 40 %, 20 M zen | S21Emu GoblinCombination.xml mix 507 |
| 112 | Fourth wings: Lv.4 Wing Relic and a higher magic stone; 24 %, 20 M zen | GoblinCombination.xml mix 2010 |
| 120-134 | Wing core enhancement +1 to +15: the core, 1-5 wing core upgrade stones, 1-8 M zen, 100 % | WingCore.xml MixLevelUP kind 107 |

The fourth wings combination gives the wings of the player's class (none: "class too low") with rolled grade options.
Keep bits of a mastery mix: 1 excellent and mastery options, 2 luck, 4 set options, 8 new mastery options, 16 a new
option (+4 to +16), 128 everything (CustomMix.xml). Designed, without a source: a failed mastery mix keeps the main
material and uses up the others; only one higher magic stone counts; a failed core enhancement keeps the core. The S6
mix request `C1 86` with the crafting number starts a mix; the mix of the items is also found without it.

No new packet is needed: every option rides in the W32 item, and every packet which carries an item sizes it by the
serializer. Season 6 clients keep every packet byte for byte (the visibility tests and the Season 6 serializer tests).

## Elements, pentagrams and earrings

Features `pentagram_elements` (update "Add the elements, pentagrams and errtels", plugin "Elements (Season 21)",
`src/GameLogic/Elements`) and `earrings` (update "Add the earrings"). Pentagrams, errtels and earrings are wide
items: a Season 6 client never gets one, and its own fights only meet the elements of the monsters (below).

- **Slots.** Pentagram 236, right earring 237, left earring 238 (Item.xml `Slot`), after the personal store
  (204-235), so no stored bag, extension or store slot shifts. `InventoryConstants.ExtraEquippedSlots` and
  `IsEquippedSlot`; the inventory holds them as a storage of three single slots. The inventory list sends them
  with their slot like any other item.
- **Element.** A character's element is its pentagram's (option type `Element`, 1 fire, 2 water, 3 earth, 4 wind,
  5 darkness, rolled when it drops); a monster's comes from Monster.xml `ElementalAttribute` (6: one of the five by
  its object id, new with each spawn).
- **Elemental damage.** Every hit of an attacker with an element carries an elemental part: its own success roll
  (elemental attack rate against elemental defense rate, shaped like the normal hit), damage between minimum and
  maximum plus the bonuses, times the increase, the element wheel, the skill's imprint (Skill.xml
  `ElementImprintBonus`) and 1.3 against a player, minus the defender's elemental defense with its bonuses. The
  elemental part is a part of the hit's damage (Season 6 clients see the sum); `C1 EE 84 ElementalDamage` shows it
  to the attacker and the defender. Wheel (attacker rows; Webzen EX700 guide, wind against wind taken as 100 %):

  | | fire | water | earth | wind | darkness | none |
  |---|---|---|---|---|---|---|
  | fire | 100 | 80 | 90 | 90 | 120 | 120 |
  | water | 120 | 100 | 80 | 80 | 110 | 120 |
  | earth | 110 | 120 | 100 | 100 | 90 | 120 |
  | wind | 90 | 110 | 120 | 100 | 80 | 120 |
  | darkness | 80 | 90 | 110 | 110 | 100 | 120 |

- **Pentagrams** (25, Item.xml slot 236): `DamageMin/Max` the elemental damage, `Defense` the elemental defense,
  `ElementalAttack` / `ElementalDefense` the rates. 1-4 sockets when they drop; the Expansion Scroll of Radiance
  slot (14/316, used on the pentagram) opens the fifth. Socket 0 anger, 1 blessing, 2 integrity, 3 divinity,
  4 radiance. The pentagram options (PentagramOption.xml) are active while the sockets hold the errtels they need
  (rank and level per socket); the mastery errtels have a set (fighting spirit, passionate, 50 %) whose bonuses
  need 2 and 4 errtels of the set in a mastery pentagram.
- **Elemental critical, dodge, radiance.** A critical elemental hit deals the maximum elemental damage (design: like
  the normal critical hit; no source gives a multiplier); a perfect dodge takes none; punish adds 6-16 % of the
  target's maximum health to 30 % of the critical hits (at most 250,000, muonlinefanz.com); absorb shield / life give
  1-11 % of the elemental damage back at 30 %. Bastion, bleeding, paralyzing, binding and blinding stay data.
- **Errtels** (13): an element and rank options (PentagramJewelOption.xml, values of the levels 0-10): rank 1 rolled
  when it drops, the next ranks by the rank up. `C1 EE 81 ErrtelSocketRequest` puts an errtel into the socket of its
  kind of a pentagram of its element (the errtel is used up, its options move into the pentagram) or takes it out
  (PentagramJewelMixRate.txt chances by level and rank; on failure the errtel is gone); `C1 EE 83 PentagramResult`
  answers, `C1 EE 86` / `C1 EE 85 PentagramErrtels` list the errtels of a pentagram's sockets.
- **Refinery** (chaos goblin, crafting numbers 140-164 of the mix request `C1 86`): 140-153 the upgrades to the
  mastery pentagrams and errtels (ElementalUpgrade.xml: emblem, bless and soul x30, creation x10 as packed jewels;
  keeps element, sockets, errtels; a pentagram from +11 at 80 / 85 / 90 / 95 / 100 %, an errtel from level 9 at
  100 %), 154-163 the errtel level up from level 0-9 (elemental runes 12/288 by rank,
  money by level), 164 the rank up (chance by the level of the highest rank).
- **Earrings** (16): basic options "ATK/Wizardry/Curse min - max" and "Elemental DEF" (Item.xml, the same numbers in
  Webzen's guides and muonlinefanz.com), one random option and two more by 20 % each (EarringItemOption.txt 70 / 20 /
  10 %; muonline.net says 20 / 40 / 40 %), and a link set per tier: enemy defense ignore, all stats and, from Blood
  on, mastery damage. Options 28-30 are mastery attack/wizardry and mastery excellent / critical damage (Webzen 5th
  and 7th earrings; the other tiers interpolated). Enhancement +0 to +15 at the chaos goblin (crafting numbers
  165-179): 60 % to +7, 80 % above, 1-5 lapidary stones (13/510) and 1-8 million zen (EarringItemOption.txt section 4;
  muonline.net: 55 stones, 67.5 million in total); from +7 a failure resets the earring to +0 (Webzen guide 150).
  What a level adds is published nowhere; the level is kept for it.

W32 fields:

| Item | Byte 21 | Slots 13-17 / levels 18-20 | Kind block 22-27 |
|---|---|---|---|
| pentagram | element, kind 1 | per socket: rank count of its errtel (1-5), 0xFE empty, 0xFF no socket; level of the errtel | zero |
| errtel | element, kind 2-6 (anger to radiance) | per rank: option number (0xFF none); its level | rank count, socket |
| earring | zero | none | option count, five option numbers (0xFF none) |

Packets (80-8F):

```
C1 08 EE 81  ErrtelSocketRequest (client)   C1 08 EE 83  PentagramResult (server)
  4  u8  mode: 0 insert, 1 take out            4  u8  mode
  5  u8  pentagram slot                        5  u8  0 success, 1 failed (errtel gone), 2 refused
  6  u8  errtel slot (insert)                  6  u8  pentagram slot
  7  u8  socket 0-4                            7  u8  socket
C1 1B EE 80  ElementInfo (server)           C1 04 EE 87  ElementInfoRequest (client)
  4 u8 element, 5/9 u32 LE damage min/max, 13 defense, 17 attack rate, 21 defense rate (u32 LE),
  25 u8 critical %, 26 u8 dodge %; answers EE 87 and follows every errtel socket change
C1 0B EE 84  ElementalDamage (server)       C1 05 EE 86  PentagramErrtelsRequest (client)
  4  u16 BE  target id                         4  u8  pentagram slot
  6  u8      element                         C1 .. EE 85  PentagramErrtels (server)
  7  u32 LE  damage                            4 u8 pentagram slot, 5 u8 count, then 14 bytes per errtel:
                                               socket u8, errtel number u16 LE, rank count u8, 5 x (option, level)
```
