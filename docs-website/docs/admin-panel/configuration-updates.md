---
title: Configuration updates
sidebar_position: 3
description: Apply the configuration data updates which ship with new OpenMU versions.
---

# Configuration updates

**Navigation:** *Updates* — route `/config-updates`

OpenMU is under active development, and new versions regularly bring changes to
the **game configuration data**: a corrected monster stat, a new drop item group,
a new plugin configuration, a fixed quest.

Your database keeps the configuration you initialized (and possibly customized),
so those changes cannot simply be applied by updating the software. Instead they
come as *configuration updates*, which you apply here.

The navigation menu shows a badge with the number of available updates, so you
notice them after an upgrade.

## Applying updates

The page lists every update which has not been applied to your database yet, with
its description. An update can also reappear with a new version (shown as
`v1 → v2`) when a previously installed update was extended — applying it brings
your database to the current state without duplicating anything.

* **Mandatory updates** are always applied and cannot be deselected — the server
  requires them.
* **Optional updates** can be selected or deselected individually, which lets you
  keep your own customizations where an update would overwrite them.

After the run, each update is marked as applied or, when something went wrong,
as failed.

:::warning[A restart is required]
The updates take effect after a restart of the server process. Until then, the
running server still works with the configuration it loaded at startup.
:::

Some updates are required before the game servers can run. For example, a game
server doesn't start without the item price definitions, which the update *Add
item price definitions* adds to databases from before they existed. Otherwise
it would sell special items like jewels at the price of equipment. The same goes
for the item stat profiles with the item formulas, which the update *Add item
stat profiles* adds. The game server logs the reason and stays stopped, while
the admin panel keeps running, so you can apply the update here. Then use
**Reload configuration and restart all game servers** on the
[Servers page](servers.md), or restart the server process.

## At startup

The all-in-one server applies the pending mandatory updates itself when it
starts, right after the database schema update, so the game servers don't start
without the data they require. It lists the updates in the console and handles
them like the schema update:

* With *Auto update schema* enabled in the
  [System configuration](game-configuration.md#system), it applies them
  automatically.
* Otherwise, it asks whether to apply them. If you decline, or the console can't
  be read, the server doesn't start.

The start leaves out the optional updates, so they never prevent it; they stay
listed here for you to decide. It also leaves out a mandatory update which
depends on an optional update you haven't applied, or on an update which isn't
available, e.g. a disabled one. In a distributed deployment, the processes don't
apply updates; apply them here.

## On a new database

A new database already contains the changes of OpenMU's own updates, because the
initialization creates the current configuration. It therefore marks them as
applied, and this page starts empty.

Plugins can bring configuration updates too, for example one which adds the
plugin's own item. Their changes are not part of OpenMU's initialization, so such
an update declares `RunsOnNewDatabase`, and a new database applies it right away,
after the updates it depends on. On an existing database it is listed here like
any other update.

An optional update of that kind is not applied on a new database: it is listed
here as pending, as on an existing database, so you decide whether to apply it.
The same goes for the updates which would be applied and depend on it. Their
dependencies are checked when you apply them, so an optional plugin update with a
missing dependency doesn't stop the creation of a new database.

## Maps of the later seasons

A Season 6 server can add the maps which came after Season 6, up to Season 20,
and the gear of Season 21 with three optional updates. A new database leaves them pending, so a server
changes only when you apply them:

* **Add the maps of the later seasons (Season 21 data)** adds Acheron (with
  Deventer, Urk Mountain and Nars), Ferea, Nixie Lake, Deep Dungeon 1-5, the
  Swamp of Darkness, Kubera Mine 1-5, Scorched Canyon, Crimson Icarus, Arenil
  Temple, Ashen Aida, Old and Blaze Kethotum, Kanturu Underground, Ignis
  Vulcanus, Blood Tarkan, Tormenta Island, Twisted Karutan, the Kardamahal
  Underground Temple, the Swamp of Doom and Atlans Abyss 1-3, with their
  monsters, spawns, gates, entrances on the existing maps and warp list entries.
  Each map requires the total level (level and master level) of Season 21, for
  example 850 for Kubera Mine. The gates and the warp list check the normal level,
  which is capped at the maximum level. The game servers which host a map's
  safezone map host the new map too. The maps keep the entry levels and
  monsters of Season 21, so the update depends on the level caps of Season 21:
  keep **Set the level caps of Season 21** selected (it is by default), and both
  are applied in that order.
* **Set the level caps of Season 21** raises the maximum master level to 1350
  and continues the master experience curve: as in Season 6 up to master level
  400, and twice its step above.
* **Add the gear of Season 21 (Season 21 data)** adds the items of Season 21
  which the Season 6 character classes can use: rings and pendants, socket
  weapons and shields, the blessed archangel weapons, socket armor sets, the
  *[Bound]* copies of Season 6 gear and wings, and the Wings of Conqueror. Each
  item takes the options and level tables of the Season 6 item of its kind, with
  the values of Season 21. Monsters drop the ones the data lets them drop, and
  the red and blue chocolate boxes hold its rings. Each item carries the season
  which introduced it (Season 21 Part 1 where the history doesn't say), so
  Content activation can switch the gear off by season too.

Afterwards, every value can be changed in the game configuration: the warp costs
and level requirements in the warp list, the entry level in the requirements of
each map, the monsters in their definitions. To switch the maps off again, use
[Content activation](game-configuration.md#content-activation): they are marked
with the season which introduced them.

Older clients don't know these maps. They are meant for clients which do, for
example a client of a later season or a custom one.

## Preconditions

If the database is not installed yet, or a schema update is pending, this page
refers you to the [Setup page](setup.md) first — configuration updates are
applied on top of a current schema.

## Good practice

* Take a backup of your PostgreSQL database before applying updates to a server
  which has players on it.
* Read the descriptions. If an update touches something you customized (drop
  rates, monster stats), you may want to deselect it and merge the change
  yourself in the [game configuration](game-configuration.md).
* Apply updates during a maintenance window, since a restart is needed anyway.
