# Mini games

Every mini game runs in a `MiniGameContext` on its own copy of a map (`MiniGameManager`, keyed by `MiniGameMapKey`).
The copy follows `MiniGameDefinition.MapCreationPolicy`: one shared copy per game level, one per party, or one per player.

## Party instances of Season 21 (fork feature `party_instances`)

`MiniGameType.PartyInstance` (`PartyInstanceContext`) is the instance system of Season 21 (S21Emu `InstanceSystem.txt`):
a copy of a map for each party, with its own monsters and timer. The game level is the content type of the data.

- Entry: `PartyInstances.EnterAsync` (or the Babylon `EE B1` request) goes through `EnterMiniGameAction`, so the usual
  rules apply (level bracket, maximum players, player killers, map requirements). The key is the party master, so the
  members of a party share one copy and another party gets its own; a player without a party gets a copy of its own.
- Timer: a short entry phase (`EnterDuration`, 10 s), then the game runs for `GameDuration`. Members of the party may
  join while it runs.
- Monsters: the spawns of the map copy. Spawns with the trigger `OnceAtEventStart` or `OnceAtWaveStart` make the copy
  clearable: when all of them are dead after the last wave started, the instance ends with success and every player in
  it gets the rewards of the definition (rank 1). A copy without such spawns is only a place; the class quest trial
  spawns its own monsters (`EvolutionQuests`).
- Cleanup: when the last player left the copy, it is disposed at once (not after the exit time), so the party starts a
  new one next time. The end of the time warps the players to the safe zone and frees the map.

| Content type | Instance | Map | Players | Update |
|---|---|---|---|---|
| 2 | Trial of the fourth and fifth class quest | 121 Trial Area, gate 540 | 5 | "Add the party instances of Season 21" |

## Events of Season 21 for the Babylon client

`ISeason21EventViewPlugIn` shows the state, score, result and refused entry of the Season 21 events (packets `EE B0`-`B4`,
docs/babylon.md "Events"). The Season 6 client has no view of them and gets the mini game messages in words; its enter
result plugin sends nothing for these types.

## Doppelganger of Season 21 (fork feature `doppelganger_s21`)

The update "Renew the Doppelganger for Season 21" (S21Emu `Event/DoubleGoerRenewal`) keeps the Season 6 event and
gives it the rooms of the renewal as its monster scalings (`DoppelgangerMonsterScaling`): seven rooms by total level
(up to 399, 799, 899, 1099, 1299, 1499 and above) with one multiplier each for level, health, damage and defense, the
experience of the room for every player when the defense succeeded (`RewardExperience`, as master experience at the
maximum level) and the silver and golden chests of the room (`InterimChestItems`, `FinalChestItems`, bags 78-87 and
452-455, one random item). Every level from 50 may enter; a game needs two players (`MinUserInParty`).

Not ported: the race of up to four parties on parallel lanes of one map (`DoppelGanger_PositionInfo.xml`), which needs
the lanes in the client; each party keeps its own copy and path.
