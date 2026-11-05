# Maps of the later seasons

The maps which were added after Season 6, as the Season 21 server data defines
them, as optional updates of a Season 6 database:

* `AddSeason21MapsUpdatePlugIn` adds them (`Season21MapsInitializer`),
* `AddSeason21LevelCapsUpdatePlugIn` sets the level caps of Season 21, which the
  maps depend on: their entry levels and monsters keep the numbers of Season 21,
* `AddSeason21GearUpdatePlugIn` adds the gear of Season 21 (`Season21GearInitializer`),
* `AddSeason21ItemCatalogUpdatePlugIn` defines every other item of the data (`Season21ItemCatalogInitializer`).

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
  tiles of the data are not walkable,
* the gate of Nixie Lake in Raklion is moved to walkable ground of the Season 6
  Raklion,
* where the data contradicts itself, the gates (`Gate.xml`) win over the move
  list (`Move.txt`): Deep Dungeon 1-5 ask 730, 750, 770, 780 and 780, Atlans
  Abyss 2 asks 925 (`Season21Map.EntryLevel` overrides the level of a set),
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
elemental attributes, the critical and excellent damage resistances, the spawn
element and the gates' maximum level and reset columns.

Content which needs systems which don't exist yet: the event copies of Acheron
and Deventer (Arca War), the mining spots of Kubera Mine, the boss Nix (it's
defined, but the data spawns it by an event), the elite zone flag of Scorched
Canyon and the Aquilas Temple (Season 21), whose spawns aren't taken from the data yet.

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
