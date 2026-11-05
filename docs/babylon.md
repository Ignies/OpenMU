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

## Balance

The Season 21 data keeps the original numbers. A computed normalization layer (a world setting choosing
original, normalized or stars) is planned separately; it is meant to read the original rows and scale them at
runtime, so the data and the corrections here stay the source of truth.
