# Maps of the later seasons

The maps which were added after Season 6, as the Season 21 server data defines
them, as optional updates of a Season 6 database:

* `AddSeason21MapsUpdatePlugIn` adds them (`Season21MapsInitializer`),
* `AddSeason21LevelCapsUpdatePlugIn` sets the level caps of Season 21, which the
  maps depend on: their entry levels and monsters keep the numbers of Season 21,
* `AddSeason21GearUpdatePlugIn` adds the gear of Season 21 (`Season21GearInitializer`).

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
  Raklion.

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
* A map drops what its safezone map drops.
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

Left out, because Season 6 has nothing to hold them: the items of the fourth
and fifth classes and their wings, the 2.5 wings, the event wings, the timed
items, quivers and pentagrams. The wing look for other players needs no server
change for the open source client, whose extended appearance carries the item
group and number of each slot; the appearance of the original Season 6 client
names wings by an enumeration which doesn't know the new ones.
