# Maps of the later seasons

The maps which were added after Season 6, as the Season 21 server data defines
them, as optional updates of a Season 6 database:

* `AddSeason21MapsUpdatePlugIn` adds them (`Season21MapsInitializer`),
* `AddSeason21LevelCapsUpdatePlugIn` sets the level caps of Season 21, which the
  maps depend on: their entry levels and monsters keep the numbers of Season 21,
* `AddSeason21GearUpdatePlugIn` adds the gear of Season 21 (`Season21GearInitializer`),
* `AddSeason21ItemCatalogUpdatePlugIn` defines every other item of the data (`Season21ItemCatalogInitializer`).
* `AddSeason21SkillsUpdatePlugIn` adds the skills, master tree nodes, books and the skill enhancement tree
  (`Season21SkillsInitializer`); it needs the classes and the item catalog.
* `AddSeason21EconomyUpdatePlugIn` adds Priest James and the Ruud shop and switches on the gremory case, Ruud and the
  item expiry (`Season21EconomyInitializer`); `AddSeason21ShopsUpdatePlugIn` sets the NPC shops with their tabs
  (`Season21ShopsInitializer`). Both need the item catalog; see "Economy".
* `AddSeason21MiningUpdatePlugIn` places the mining spots and turns the mining on (`Season21MiningInitializer`); it needs
  the shops (Bolo sells the Pandora Pick). See `GameLogic/Mining/Readme.md`.
* `AddSeason21GoldenInvasionUpdatePlugIn` adds the golden monsters 493-502 (`Season21GoldenInvasionInitializer`, S21Emu
  `Monster.xml` and the item bags 117-126: a Box of Kundun +1 to +5 each) to the Season 6 plugin "Golden Invasion". A
  configuration with the Season 6 roster becomes the one of `Event/InvasionManager.xml` (19 kinds, the Golden Derkon also
  in Elbeland, every four hours from 00:25, 10 minutes); one the operator changed keeps its numbers and timetable and gets
  only the missing kinds and maps. The spawn areas of the data (e.g. the west of Dungeon) are not kept: invaders spawn
  anywhere walkable, as the plugin does. The version of the monsters (Season 5) is our assumption.
  Version 2 adds the Underworld invasion (`UnderworldInvasionPlugIn`, ID 0: the Death King 55 and 110 Death Bones 56 on
  Lorencia or Noria from 00:05) and the Summon invasion (`SummonInvasionPlugIn`, ID 9: the Golden Goblin 652 on Tarkan, Aida,
  Kanturu, Karutan 1/2 or Acheron from 01:25), both every four hours for 10 minutes, with their monster rows, and switches
  them on; one count per monster, so 10 Goblins also on Karutan (data: 5).
* `AddSeason21HuntingRecordUpdatePlugIn` sets the 59 maps and the 64 excluded event monsters of `HuntingRecord.txt` and turns
  the hunting record and the ranking on (`Season21HuntingRecordInitializer`, data `Resources/Season21/WorldSystems.json`
  written by `Generator/season21-world.mjs`). See `GameLogic/HuntingRecord/Readme.md`.
* `AddSeason21SoulConverterUpdatePlugIn` sets the five tiers of the monster soul converter with their missions and reward
  bags and turns it on (`Season21SoulConverterInitializer`); it needs the item catalog. See `GameLogic/SoulConverter/Readme.md`.
* `AddSeason21HelperPlusUpdatePlugIn` sets the hunting grounds and zones of the MU Helper Plus and turns it on
  (`Season21HelperPlusInitializer`); it needs the maps (the grounds use their warps). See `GameLogic/HelperPlus/Readme.md`.
* `AddSeason21SeasonPassUpdatePlugIn` sets the season pass and turns it on (`Season21SeasonPassInitializer`); it needs the
  item catalog (tickets, item rewards). See `GameLogic/SeasonPass/Readme.md`.

They run on a new database too (`RunsOnNewDatabase`), but as they are optional,
a new database leaves them pending. So a Season 6 server only changes when its
operator applies them.

## Data

`Season21MapData` holds one `Season21MapSet` per group of maps which share their
monsters and entrance: the maps, monsters, spawn areas, gates and warp list
entries. The terrain files are embedded as `Resources/Season21/Terrain{n}.att`;
several maps can share one, e.g. Kubera Mine 1-5 all use `Terrain124.att`.

The rows were taken from the server data of a Season 21 (Part 1-2) server
emulator (`MapManager`, `Monster`, `MonsterSetBase`, `Gate` and `Move`) and
checked against the text tables of the Season 21 client (map, monster and move
names). As with every third-party server data, they show what that server does,
not necessarily what the original does. Where they were obviously wrong, they
were corrected:

* misspelt monster names, e.g. "Scotched Warrior" is "Scorched Warrior",
* spawn areas without a walkable tile on the map's terrain are left out,
* a single monster spawns anywhere within its range of its tile, because many
  tiles of the data are not walkable; where only one or two tiles of that range
  are (e.g. Swamp of Darkness), the server picks one of them when its random
  tries miss,
* the gate of Nixie Lake in Raklion is moved to walkable ground of the Season 6
  Raklion,
* where the data contradicts itself, the gates (`Gate.xml`) win over the move
  list (`Move.txt`): Deep Dungeon 1-5 ask 730, 750, 770, 780 and 780, Atlans
  Abyss 2 asks 925 (`Season21Map.EntryLevel` overrides the level of a set),
* Nars asks 400 (Khdoop, muonlinefanz.com), not the 300 of Acheron which `Gate.xml`
  487 repeats; version 3 of the maps update moves an existing database
  (`Season21MapsInitializer.RepairEntryLevels`, levels an operator chose stay),
* Old Kethotum spawns only its Lv195 monsters: the 16 elite spots of its file are
  the Old Crimson Minos which the file of Blaze Kethotum holds on the same
  points, and these points of Blaze Kethotum hold its own Crimson Minos,
* the elites of the whole-map placeholder file of Arenil Temple are left out
  (they belong to an elite zone),
* Kubera Mine uses the newer of the two spawn sets of the data ("Kubera Mine N",
  with the Dead Shoveler),
* the rows of the Scorched Assassin and the Scorched Wizard are swapped back (the
  ranged magic row, whose maximum damage was its defense, is the wizard's).

The versions (`IntroducedIn`) are the ones in which the maps were added
according to the [MU Online history](https://github.com/Khdoop/mu-online-history).

## Placeholder rows

Some rows of the data are placeholders; they are replaced (`Season21PlaceholderRows` keeps the data's numbers, and the
maps and elite zones updates, version 2, repair a database which still has them, unless the operator changed a value):

* Crimson Icarus 859-863 (650,000 health each, weaker than Atlans Abyss): health, damage, defense and defense rate of
  muonlinefanz.com (Crimson Flame's Icarus, 2,422,700 to 2,667,000 health); the attack rate and the level stay. Its
  elites 864/865 get the median factor by which the normals changed (health 3.9, damage 1.6, defense 1.7).
* Under Persona 905: 1,060,000 health instead of 106,000 (its siblings have 1.12-1.15 million, its elite 1.4 million).
* Atlans Abyss 2/3 Medium (Large) Bahamut 834 and Great Bahamut 836 (below the fan table): health, damage and defense of
  muonlinefanz.com (1,815,000 / 2,015,000 health); level, attack and defense rate stay (version 4).
* Season 6 rows S21Emu changed: Phoenix of Darkness 77 75,000 health (95,000), Death Spirit 410 level 118 (119)
  (`ClassicRows`, version 4). The Dark Elf 412 keeps 1,500,000: S21Emu has 1,000,000, muonlinefanz.com 1.5 million.
* Elites equal to or weaker than their normals (Atlans Abyss 3 839/840, Tormenta 945-947, Swamp of Doom 972-974): the
  normal's row times the median elite factor of the data's real elite pairs (health 1.267, damage 1.1, defense 1.059,
  defense rate 1.037, attack rate 1), never below the data's own value; the level stays. No source gives these rows.
* Twisted Karutan 958-960 (`EliteMonster` 0 in the data) are real elites by their numbers and keep them; Aquilas Temple
  (975-980) isn't shipped.

Devias (`Season21ClassicSpawns`, maps update version 5): the 280 monsters of `MonsterSetBase/002 - Devias.xml` (10
spots and 47 single monsters: 40 Yeti, 82 Elite Yeti, 11 Ice Queens) replace the 560 of Season 6 (no Yeti, 235 Elite
Yeti, 75 Ice Queens) while the roster is still the Season 6 one; the NPCs stay.

Town NPCs (`Season21ClassicSpawns.TownNpcs`, maps update version 6) at their `MonsterSetBase` points: Captain Slough 580
(the Acheron guide, passive; no shop or Spirit Map in the data), Maple Spirit 873 and Maple Tree 874 (passive; the maple
leaf event has only its switch in `GameServerInfo - Event.dat`) in Noria; Apostle Devin 406, Moss 492, Lugard 540 and
Jerint 522 also in Lorencia, where Season 21 gathered the event entrances. Wizard Gillard 891 is a merchant with the
shop `Shop/027` (shops update version 2, Lorencia 119/111). The versions of the new NPCs aren't in any source: Season 7
(Acheron) for Captain Slough, Season 21 for the others.

## What is applied

* The monster level is the experience level of the data, as for the other maps.
  Resistances 1 to 7 are ice, poison, lightning, fire, earth, wind and water,
  each `value / 255`.
* Each map requires a total level (`Stats.TotalLevel`). Its gates and its warp
  list entries check the normal level, so they require the total level capped at
  the maximum level: master levels are gained only at the maximum level, so a
  character with the total level always has that level.
* A map drops what its safezone map drops. Each monster drops its list of the
  per-map drop files of the data (`Season21DropGroups`, `Season21DropData`, generated from
  `Item/MonsterDropManager`): one drop group of the monster per drop weight,
  whose chances share the 30 % of a random item drop by their weights. As groups
  of the monster, they aren't filtered by the monster level, which is above any
  drop level here. Items which the configuration doesn't have are left out;
  when the item catalog defines more, it adds them to the groups. A group's
  chance counts its active items; it's set again when items join, unless the
  operator changed it.
* The game servers which host a map's safezone map host the map too.

## What is left out

Columns the configuration has no field for: the displayed monster level, the
magic defense (0 for all), the attack types of skills which aren't implemented,
the item and money rates (the drops come from the drop groups of the map), the
elemental attributes (applied by the elements update, below), the critical and excellent damage resistances, the spawn
element and the gates' maximum level and reset columns.

Content which needs systems which don't exist yet: the event copies of Acheron
and Deventer (Arca War), the mining spots of Kubera Mine, the boss Nix (spawned
by the field bosses update, below), the elite zone flags (no client model)
and the Aquilas Temple (Season 21), whose spawns aren't taken from the data yet.

## Elite zones and field bosses

Two more optional updates, both after the maps:

* `AddSeason21EliteZonesUpdatePlugIn` (`Season21EliteZonesInitializer`, `Season21EliteZoneData`): five zones per map
  at Webzen's entrances (Atlans Abyss 3, Scorched Canyon, Crimson Icarus, Ashen Aida, Blaze Kethotum, Kanturu
  Underground, Ignis, Blood Tarkan, Tormenta, Twisted Karutan, Kardamahal, Swamp of Doom). A zone's area spans its
  entrance and the elite spot of the spawn file (else 6 tiles around the entrance); its elites spawn there and every
  other spawn of them on the map goes. Crimson Icarus keeps 6 elites a zone of its 187 spread ones; Ignis, Kardamahal
  and Swamp of Doom get their elites (914-916, 966-968, 972-974), which the data defines but never spawns, 6 a zone.
  Twisted Karutan's zones are walled in on the server terrain of the data but walkable on the terrain of the Season
  21 client (its tiles carry 0x0800 there): the update opens those 1,347 tiles, and its 45 elites spawn back on them.
  Arenil Temple (no published zones), Old Kethotum (no elites) and Aquilas Temple (not shipped) have none.
  The update also places the Elite Zone Flags of the spawn files (not on Ignis, whose file copies Kanturu
  Underground's) and sets the debuffs (`EliteMonster.txt`) and weekly drop limits (`ItemDropLimit.xml`) when the
  configuration has none.
* `AddSeason21FieldBossesUpdatePlugIn` (`Season21FieldBossesInitializer`, `Season21FieldBossData`): Medusa 561 and
  the Sapi Queens 560 it calls (`InvasionManager.xml` ID 8),
  Lord of Ferea 734 and its crystal orb 735, the God of Darkness 790-794 from `Monster.xml` (experience level as
  everywhere; Medusa and its Sapi Queens keep levels 175 and 141 on their Season 6 map). Each boss gets a spawn area which only the plugin uses
  (`ManuallyForEvent`, so Lord Silvester and Core Magriffy stop spawning as plain monsters and the progression
  normalizes the bosses with their map), and its item bag (`EventItemBag` 106, 168, 191, 293, 309, 310): one drop
  group per section with a chance of 1, so every section drops one item. Items the catalog lacks are left out.
  It places Priest James (725) in Elbeland and Loren Market, who hands out the Spirit Nixie's Shard which Nix's
  statue takes, and fills in the grounds, special skills and entry item of a boss an operator configured without them.

## Evolution quests

The Trial Area (121, `Terrain122.att`) with Sent and Eunice, their town NPCs in Lorencia and Devias and the fourth and
fifth class quests come with the optional update "Add the evolution quests of Season 21"
(`Season21EvolutionQuestData`, `Season21EvolutionQuestsInitializer`); the rules and the trial are in
`GameLogic/EvolutionQuests` (see its readme). Version 2 of the update gives the third evolution quest "Infiltration of Barracks" 20 level up
points (S21Emu `QuestReward.xml` RequireIndex 5; Season 6: 10), so the three steps give 20, 20 and 30.

## Gear

`Season21GearData` holds the items of Season 21 which a Season 6 character
class can use, read from the Season 21 server data (`Item.xml`, `ItemOption.xml`,
`SocketItemType.xml` and the item bags of the boxes). Their names are in
`Season21ItemNames`. Each item copies the Season 6 item of its kind: a ring the
Ring of Ice, a pendant the Pendant of Fire or of Lightning, gear the template
its row names (same group, size class, hand and skill). It takes the template's
slot, options and level tables, and its own damage, speed, rise (half of the
magic power), defense, defense rate, size, skill, requirements and classes. The
template's socket, guardian and ancient options stay with the template; a
socket item gets the seed options of its kind. A lucky item of the data takes
no options. Each item carries the version which introduced it according to the
MU Online history (e.g. eX701 for the mastery socket weapons, eX702 for the
mastery socket sets, Season 8 Part 2 for the new rings and pendants, Season 11
for the blessed archangel weapons and the socket items of the Rage Fighter);
what the history doesn't list (the [Bound] copies, the decoration rings, the
Wings of Conqueror) counts as Season 21 Part 1, the version of the data.

The Ring of Block (13/174) doesn't drop, although the data lets it: the Season 21
client has no row for it, so it couldn't show it. A second run of the update keeps
the drop switch of an existing item.

Left out, because Season 6 has nothing to hold them: the items of the fourth
and fifth classes and their wings, the 2.5 wings, the event wings, the timed
items, quivers and pentagrams. The wing look for other players needs no server
change for the open source client, whose extended appearance carries the item
group and number of each slot; the appearance of the original Season 6 client
names wings by an enumeration which doesn't know the new ones.

## Item catalog

`Resources/Season21/Items.json` holds every item of the Season 21 server data, 4,701 in the groups 0-21, one per
line (`Season21CatalogItem`, loaded by `Season21ItemCatalog`): every column of `Item.xml` (fields left out are 0),
and per item what the option files say: `ItemOption - Settings/ItemOption.xml` (options as type, index and value),
`Socket - Settings/SocketItemType.xml` (sockets), `380 Items - Setting/380ItemType.txt` (guardian option),
`Mastery - Settings/MasteryItem.txt` (mastery group and tier), `Lucky Item - Settings/LuckySystem.xml`,
`SetItem - Settings/SetItemType.xml` (ancient sets) and `ItemValue.txt` (value, highest level). The names are in
`Properties/Season21CatalogItemNames.resx`, by the key of each row.

Generated, never edited by hand:

```
node Generator/season21-items.mjs --data <S21 server Data/Item> --history <mu-online-history source.md> [--log <file>]
```

`--history` is `source.md` of the [MU Online history](https://github.com/Khdoop/mu-online-history). An item takes
the version of the first section of the history which names it, its set ("Nightwing set") or, for mastery items,
its tier ("Mastery Set - Bloodangel"); a section maps to the episode of the same season, the nearest one where
`GameVersion` has none. What the history doesn't name counts as Season 21 Part 1, the version of the data
(`versionFromHistory` is false; the generator prints their number and `--log <file>` writes the list, the Season 6
items included).

`AddSeason21ItemCatalogUpdatePlugIn` defines every row the configuration lacks; defined items, Season 6 or
changed by the operator, stay as they are, so a number which Season 6 uses keeps its Season 6 item (2/22 is the
Mace of the King, not the Bloodangel Scepter). It runs after the gear update, whose items keep the templates
chosen for them by hand:

* gear (groups 0-12, slots 0-7) copies the defined item of the same group and slot whose row matches its kind,
  two-handedness, sub kind and skill best, then the nearest drop level, with `Season21GearInitializer`; it takes
  luck, the option and the excellent options of the template only where `ItemOption.xml` gives it them,
* rings and pendants copy the Season 6 ring or pendant of their options,
* pets get the pet slot, everything else the values of its row only (size, durability or stack, level, value,
  bans, skill, requirements, classes).

The class ranks of the Season 6 classes (1 and 2 any class, 3 the master class) give the qualified classes; ranks
4 and 5 (fourth and fifth class) and the nine later classes stay in the catalog until those classes exist.
No new item drops by the random drops (`DropsFromMonsters`), which would change every Season 6 map; they drop by
the drop lists of the later maps.

An item which the Season 6 item format can't carry (`WaitsFor`) is defined, but inactive, so it doesn't drop, isn't
sold and a game master can't make it: groups 16-21 (`group`), pentagrams (`pentagram`), earrings (`earring`), the
fourth and fifth wings (`wing grade`), errtels (`errtel`), muuns (`muun`), quivers (`quiver`), timed items
(`expiry`) and seed spheres above level 5 (`sphere level`). They wait for the wide item format.

Kept in the catalog only, as the configuration has no place for them yet: the mastery options, the ancient sets
of the new items, the options of the special types (10-15) other than the fixed jewelry bonuses, the resistances,
the elemental and legendary columns and the buy prices.

## Skills

`Generator/season21-skills.mjs` transcribes the skill data of the Season 21 server data into
`Resources/Season21/Skills.json` and `Properties/Season21SkillNames.resx`:

    node season21-skills.mjs --data <S21 server Data folder>

* `Skill/Skill.xml` (973 skills), `Skill/MasterSkillTree.xml` (1054 nodes of the 16 classes),
  `Skill/Formula.txt` (section SkillTree, the values of the nodes by their level),
* `Skill/EnhanceSkillTree/EnhanceSkillTree.xml` (2548 nodes), `EnhanceSkillTreePassive.xml` (128 passives) and
  `EnhanceSkillTreeMagicField.xml`. The data has the tree twice; the copy in the folder (S21.1-2) wins over
  `Skill/EnhanceSkillTree.xml` (S20.1-1): it differs in two requirements of the dark wizard, which the newer copy
  points at the nodes beside them,
* `Effect.txt` (names and groups of the effects), `Character/CharacterSkillInit.xml` (the first skills of a class),
* the books: `Skill/SkillRequire.txt` names the 22 which need a master skill; the others are matched to their skill by
  name (`Item.xml` has no skill on a book). 104 books teach a skill; the unmatched ones are Season 6 books, which
  keep the skill of their Season 6 data, and a few without a skill row (Marble Burst, Unleash Marble).

`Season21SkillsInitializer` maps the rows onto the configuration:

* A skill which the configuration has keeps every value; it only gets the classes after Season 6 which the data
  names (e.g. Poison for the rune wizard). The 598 skills it doesn't have are added: damage, mana, AG, range,
  required level, energy and command as the data has them. A class column says from which evolution on (1 first
  to 5 fifth) a class may use the skill; every later evolution of the class may use it too.
* Use type 3, 7, 9, 10 and 11 (master and tree passives) are passive boosts. A skill of kind 0 or 7 hits: with a
  radius it hits every target around its target (`EffectRange` = radius), else one. Kind 1 (buff) and 2 (debuff)
  with an effect which has a duration in the configuration work as buffs and hits; their effect is the configured one.
* The effects which the configuration doesn't have are added with their name and group, without power-ups or
  duration: a buff or debuff with such an effect is kept as a skill of type `Other` (the server doesn't apply it
  yet), and an attack doesn't link it, because it would apply an effect without duration. Kinds 3-6 (removals,
  summons, movements, heals) are `Other` too. They wait for their effect data.
* The damage type follows the stat the data specializes the damage by (energy: wizardry, the others: physical), else
  the class (dark wizard, summoner and the wizard classes of Season 21: wizardry).
* A master tree node of an added skill gets a `MasterSkillDefinition`: the root of its group, the rank of its slot
  (the client tree has 3 roots of 4 x 9 slots), the minimum and maximum level, the formula of `Formula.txt` (variable
  `level`), the skill it replaces and the skills it requires. The Season 6 nodes are kept as they are, so the Season 6
  client's tree stays right; the Season 21 nodes of the Season 6 skills only add the new classes.
* A book of the item catalog which teaches no skill yet gets its skill. Version 3 of the update adds the skills which
  `SkillRequire.txt` asks for to the configuration of the plugin "Learnables" (`SkillBookRequirement`: the skill the
  character has, the master skill and its points, 10 for every row); such a book teaches only when they are met.
* The skill enhancement tree is stored as the configuration of `EnhanceTreeFeaturePlugIn` (GameLogic `EnhanceTree`),
  which the update turns on.

Not taken: the delay of a skill, the element imprint and the attack patterns (no field and no system yet), the
damage reduction table, the Ability Enhancement Core (track T, later).

## Item options of the wide item format

`Season21ItemOptionsInitializer`, four optional updates which run again without change:

* "Activate the items of the wide item format" (`ActivateSeason21WideItemsUpdatePlugIn`): the catalog rows with a
  `waitsFor` become active; those of the groups 0-15 get the option definition "Wide Item Format", so only the
  Babylon client gets them (`GameLogic/WideItems`).
* "Add the mastery options" (`AddSeason21MasteryOptionsUpdatePlugIn`): the mastery items of the catalog
  (`MasteryItem.txt`) get the mastery options of their group and a mastery bonus option (`MasteryOption.xml`); the
  all stats bonus reaches the stats through a relationship per class (`Stats.AllStatsBonus`).
* "Add the grade options of the fourth and fifth wings" (`AddSeason21WingGradesUpdatePlugIn`): `ItemGradeOption.xml`
  by item kind (76 fourth, 106 fifth); wings without a slot get the wing slot.
* "Add the item mixes of Season 21" (`AddSeason21ItemMixesUpdatePlugIn`): the configuration of the plugin "Item rules
  of Season 21" (grade rates, the mastery mixes of `Season21MixData`, `Resources/Season21/MasteryMixes.json` from
  `Generator/season21-mixes.mjs --data <S21Emu Data/Item>`), and at the chaos goblin the mastery combination, the fourth
  wings combinations and the wing core enhancement. Version 3 adds the archangel upgrade as recipes of the same mix
  (`ArchangelMixes`, ids from 9001): the Archangel's Hammer, and the 14 Divine weapons of `ArchangelUpgradeMix.xml` +15
  with the +16 option, the hammer and a bundle of 30 Bless, Soul, Life and Chaos to the Blessed weapon +0 (excellent
  options and luck kept, 100 %). The ingredients are of the Webzen guides; an operator's configuration keeps its recipes
  and gets these appended once. Version 4 adds the monster wings (2.5) of `GoblinCombination.xml` mix 56 the same way
  (`MonsterWingMixes`, ids from 9101): a second wing +9 with the +4 option, a Kundun boss part (14/176-179), a Creation
  and a Chaos, 60 %, 100,000 Zen. The data picks the result in code; each second wing gives the monster wing of its
  class (`Item.xml` classes; the Magic Gladiator's Wings of Darkness the Wings of Chaos).
  Version 6 adds the socket upgrade (`SocketUpgradeMixes`, ids from 9201) of
  `SocketSystemCombination - Settings/SocketSystemCombinationMixUpgrade.xml`: each of its 118 socket items +11 to +15
  with two or more sockets, all empty (`EmptySocketsMin`), a Socket Upgrade Note (14/364), 5 Bless, 5 Soul and a Chaos
  give the next socket item +0, 80 %, 10,000,000 Zen; skill and luck 20 % each (`SkillPercent`, `LuckPercent`), 1 to 3
  sockets by 30/45/25 % (`SocketWeights`; the 4/23 row 1 to 5). A failed upgrade keeps the item (fail type 1).
* "Add the sphere levels 6 to 10" (`AddSeason21SphereLevelsUpdatePlugIn`): socket option levels 6-10, the seed spheres
  12/311-340 with their element's options and mounting, and the seed sphere enhancement (crafting numbers 100-104 at
  the seed researcher).

The values and the defaults which the data doesn't give are listed in `docs/babylon.md`, "Wide items".

## Economy

`Season21Economy` reads `Resources/Season21/Economy.json`, generated by `Generator/season21-economy.mjs --data <Data>`
from `Shop/` with `ShopManager.txt` (the shop file index to the NPC number), `ShopRuud/RuudShop.txt`,
`EventItemBagManager.txt` with `EventItemBag/` (the Ruud of the bags of a monster class) and the names of the Ruud boxes
in `Item/Item.xml`.

* Shops: 29 shops, 1772 rows. `TabInfo` 1-5 become the tabs 0-4; the tab 0 replaces the merchant store, which every
  client shows. The data has no positions, so the items are placed into the 8 x 15 grid in their order; an item which
  doesn't fit is left out. A tab whose items already equal the data stays, so the update is idempotent. The durations of
  the data (7 days on some potions and arrows) aren't applied.
* Ruud shop: section 1 is for every class, sections 2-17 for the base classes 0-15 (the class code order); 200 rows. Rows
  of items which the configuration doesn't have (or has inactive) are kept, but neither shown nor sold.
* Ruud of monsters: the lowest minimum and the highest maximum of the Ruud ranges of a bag (64 monsters).
* Chaos machine (`AddSeason21ItemMixesUpdatePlugIn` version 2): the +10 to +15 combinations succeed at 60, 60, 50, 50,
  40 and 40 % (`GameServerInfo - ChaosMix.dat` `PlusItemLevelMixRate1`-`6`); a rate which isn't the Season 6 one stays.
* Third wings (version 5): the combination 39 costs 15,000,000 zen at any rate (`GoblinCombination.xml` mix 39) in
  place of 200,000 per percent (8,000,000 at 40 %); a price which isn't the Season 6 one stays.
* Priest James (725) stands where the data places him: Elbeland 32/239 and Loren Market 215/56.
* Jewels (version 2): Chaos, Bless, Soul, Life and Creation drop from monsters of level 12, 25, 25, 46 and 46 up to 150
  (`Item/ItemDrop.xml`); above 150 the Season 21 maps give them from boxes, events and their drop lists. A jewel which
  has a maximum drop level already keeps both levels.
* Zen: the data drops money by the kill experience on every map (`MapManager.txt` `ZenAmountRate` 100 on all 121 maps),
  halved (`MoneyAmountDropRate` 50, the zen sinks plugin); there is no per-map table or cap to apply.
* Level rewards (`Character/ItemCompensationLevel.xml`): the item index is group x 512 + number; the class columns map to the
  base class codes (KM is the light wizard, LM the lemuria mage), all 16 set means every class. The data's
  `SendToGremory`, `Duration` and dates aren't applied: every reward goes through the gremory case.
* Event contribution (`Event/EventMapPointIntegration.xml`): boxes and points per event; the special monsters are the
  Blood Castle gate (131) and statue (132), the Imperial Guardian bosses (504-511) and the Doppelganger ice walker (531).
  The update "Add the level rewards and event contribution boxes of Season 21" (`Season21RewardsInitializer`) needs the
  economy update.

## Lucky sets

`AddSeason21LuckySetsUpdatePlugIn` (`Season21LuckySets`) gives the lucky armors their sets of `Item/SetItem - Settings`:
`SetItemType.xml` names one set per lucky number (groups 7-11), `SetItemOption.xml` the options. 29 sets (Moros 62 ... Elio
409; the pieces the configuration has, so the later ones need the item catalog). The sets always apply (no ancient
discriminator), from two different pieces the first n - 1 options, all on the full set. Two option rows exist: the
first sets 30 health, 25 skill damage, 20 defense, 10 % double damage, full 10 % critical and excellent chance and 10
critical and excellent damage; the second sets 50 health, 40 defense, 15 % double damage, 15 % critical chance, full 15 %
excellent chance, 20 critical and excellent damage and 5 % to ignore the defense (the four-piece Amis and Trite list the
fourth as a full option, the same in the end). Left out: the luck, life option and level rolls of the ticket exchange
(`LuckySystem.xml` `Option0`-`5` = 10 each, meaning not given).

## Elements, pentagrams and earrings

`Generator/season21-elements.mjs --data <S21Emu Data>` writes `Resources/Season21/Elements.json` from Item.xml
(the pentagrams, slot 236), `Pentagram - Settings` (PentagramOption, PentagramJewelOption, PentagramJewelMixRate),
`ElementalItemRefinemen - Settings` (ElementalUpgrade, ElementalItemRefinemenMixRate), Monster.xml (the element
columns), Skill.xml (`ElementImprintBonus`) and EarringItemOption.txt. `Season21ElementsInitializer` maps it:

- `AddSeason21ElementsUpdatePlugIn` (depends on the item catalog): the element stats, the pentagram slot type (236),
  25 pentagrams (element option, 5 sockets, base stats), 13 errtels (element, rank options with 11 level values),
  the element columns of every monster of the configuration which has a row (857 rows), the refinery craftings at
  the chaos goblin (140-164) and the configuration of the elements plugin, which it turns on.
- `AddSeason21EarringsUpdatePlugIn` (depends on the item catalog and the mastery options, for the all stats bonus):
  the earring slot types (237 right, 238 left), 16 earrings (base stats, a first option and two more by 20 %), and
  8 link sets of a left and a right earring of one tier.

Decisions (design where no source gives a number): the errtel option stats by their index (rank 1 the flat value of
the kind, rank 2 the five "against element" options, rank 3 PvP and "in raids" = PvM); the earring options by their
index after Webzen's guide of the first earrings (28-31 without effect); the earring base stats; the packed jewels
for the jewels x30 / x10 of the upgrades; success 100 % for the mastery upgrades (the data gives no rate). The
pentagram options, errtel set options, radiance effects and the earring enhancement are data only (see
`GameLogic/Elements/Readme.md`).

## Muuns

`Season21MuunData` reads `Resources/Season21/Muuns.json`, generated by `Generator/season21-muuns.mjs --data <Data>` from
`Item/Muun - Settings/` (MuunInfo.xml, MuunOption.xml, MuunAddOption.xml, MuunExchange.txt) and Monica's line of
`Monster/MonsterSetBase/051 - Elbeland.xml`. The folder `Muun - Settings/` beside `Item/` is an older copy (August 2025,
386 rows with other option groups); the one under `Item/` (October 2025) wins, as plan_s21_full_port.md says.

* Muuns: 386 rows; the item number is the index minus 16 x 512. Type 1 normal, 4 mount (Tibetton, Repen, Pon, Griffs), 5
  hero; sub type 0 normal, 1 premium, 2 tempest. The highest level is the rank + 1 (rank 1 to 4: level 2 to 5, the
  grades O to OOOO of the guides). 178 Muuns evolve into another item, which is marked evolved. Index 8657 (16/465) has no
  item in the catalog and stays without effect.
* Options: 78 rows; value 0 is the evolved value, 1-5 the levels; condition 1 applies before the evolution, condition 2
  after it. Condition 7 ("per 5 hours played") applies always: the play time isn't counted.
* Additional options: 10 rows; their days start when the Muun first enters a Muun inventory. `SetPeriod 1` rows (a fixed
  2013 window in the data) get no days.
* Exchange: an evolution stone gives 2 energy, a Muun the energy of its rank (1-6) plus its level; the six offers of
  section 2 pay 2 wizardry stones (16/240) for the evolution stone (16/211) of Branch, Hook, Chestnut Fairy, Repen, Pon and
  Griffs (section 3 has a seventh bag, Tibetton, which no offer uses). The energy for a wizardry stone, 120, is from the
  item guide of the Muun Energy Converter (mu.lv); the data has no number.
* Monica (682) stands in Elbeland at 24/229, facing south west. The update "Add the Muuns of Season 21"
  (`Season21MuunsInitializer`) needs the activation of the wide items; it also adds the option "Muun Evolution Target"
  to the evolution stones 16/211 and 16/510.

## Ability Enhancement Core

`Season21AbilityCoreData` reads `Resources/Season21/AbilityCore.json`, generated by `Generator/season21-ability.mjs
--data <Data> --names <ParticleOptionText.txt>` from `Skill/AbilityEnhancementCore/` (AbilityOption, AbilityAddOption,
AbilityParticleBook, AbilityParticleSkill, AbilityLevelUP, AbilityReduction) and the section 15 "Skilltree 5th" of
`Skill/Formula.txt`. The option names are the client's (trustMu `Lang.mpr`, `Lang/Eng/ParticleOptionText.txt`); the
server data has none.

* Cards: 45 items of group 20. 24 skill cards (one or two per class, `AbilityParticleSkill.xml`: the fifth evolution skill,
  the fourth evolution skill it needs, the static option 0-4 with its formula); 15 option cards (blue, green, purple,
  types 1-5); 3 elite cards (one per color); 3 extra damage cards (nuke, bolt, wide, 20/235-237). The options of a card
  and their classes come from `AbilityOption.xml` and `AbilityParticleBook.xml` (a bit per class in the Season 21 order);
  a skill card carries one of HP, MP or AG (`AbilityAddOption.xml`, the chance in millionths as the option weight 3/6/1).
* Values: every `%d` of a formula is the card level; the generator evaluates the levels 0 to 5. Options of the client type
  1 are rates in percent and are stored as factors (2 % = 0.02).
* Item options: one definition per card (`0x1900` + item number) with an option of the type "Ability Card Option" per
  option (number = the static option index, `LevelType.ItemLevel`, a value per card level). 46 of the 87 options have an
  OpenMU attribute (`Season21AbilityCoreInitializer.Effects`); "every 20 levels" ones follow the total level. The nuke,
  bolt and wide options, the fifth wings, mastery, bleeding, poison, splash and the static skill card options have no
  attribute yet: they are data without effect.
* Prices: `AbilityLevelUP.xml` (levels 0-4 per kind: cards, zen, Ability Upgrade Stones 20/239) and
  `AbilityReduction.xml` (zen, stones per kind). `Unk1` of the reduction and `RISection/RIType` beyond the stone are not
  read.
* The update "Add the Ability Enhancement Core of Season 21" adds the item options and fills and turns on the plugin
  "Ability Enhancement Core (Season 21)" (`src/GameLogic/AbilityCore/Readme.md`).

## World systems

`Season21WorldSystemsData` reads `Resources/Season21/WorldSystems.json`, generated by
`Generator/season21-world.mjs --data <S21Emu Data>` from `HuntingRecord.txt`, `MonsterSoulConverter/` with the item bags
`EventItemBag/` 288-355, `Character/MuHelperPlus.xml` and `Event/BattlePass/BattlePass.xml`. Each system has its update:

- Hunting record: the maps and the excluded monsters (`Season21HuntingRecordInitializer`).
- Soul converter: the activations a week, and per tier the item, the level of its comment, the mission count and missions,
  and the reward bags (`EventItemBag/` 288-290 and 344-355 by kind, 291 the accessories of the fifth tier). A bag row
  becomes a draw with the chance of its section (the lower of `DropRate` and `SectionRate`); its durability is the count.
- Helper plus: the grounds with their warp (`MoveReqIndex`) and the zones with map, element and hunting point
  (`HuntingCordX/Y`); the rectangles, buff, channel and gate columns are left out (a move only needs the hunting point).
- Season pass: the switches, the ticket experience, the schedule, the level experience, the daily and special missions
  (kind `None`), the rewards (a buff for `Type` 0, an item with its count for 1), the reward lists of the passes and the
  buff values; the Max HP buff (411) gets the maximum health, the other buffs no attribute.
