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

## Rounds events of Season 21 (`Season21/`)

`Season21RoundsContext` plays a `RoundsEventDefinition` of the plugin "Events of Season 21" (`Season21EventsFeaturePlugIn`,
one configuration for all of these events): rounds one after the other, each spawning its monsters (and its bonus
monsters, by chance) at the spawn points of their kind. A round ends when its monsters are dead or its time is over;
a kill gives its points to the killer, a cleared round its clear points, Ruud and items to every player alive. At the
end the players are ranked by their points (a party counts together when the event says so), get the rewards of the
definition for their rank (`MiniGameObjective.Rank` for the contribution boxes) and the result (`EE B3`). A game which
isn't shared starts right after its entry phase. Scheduled events open by a `MiniGameStartBasePlugIn`
(`Season21EventStartConfiguration`), off by default; the first application of the event's update switches it on.

### Tormented Square (fork feature `tormented_square`)

Update "Add the Tormented Square of Season 21" (S21Emu `Event/TormentedSquare`): map 102 (`Terrain103.att`, gate 474),
up to 20 players, 2 minutes to enter; fifteen rounds of the data (130 monsters, 50 in the boss rounds 5, 10 and 15),
points per kill 300 to 200,000, round points 3,000 to 45,000, ten golden monsters (654-656) in every round; one minute
a round, 10 s pause (20 s after a boss round); a party is a team. Timetable 11:44, 17:30, 19:00, 20:30, 22:00, 23:30
(`TormentedSquareStartPlugIn`). Not ported: the monthly tournament stages (eliminations, semi-finals, final) and their
rewards, which the data names but doesn't list; the definition has no item rewards until the operator adds them.

### Instance Zone and Labyrinth of Dimensions (fork features `instance_zone`, `labyrinth`)

Update "Add the Instance Zone and the Labyrinth of Dimensions of Season 21": map 114 (`Terrain115.att`) with Cent
(765), Eunice (940), Jerint (522) and the Portal of Dimension (758) at their S21Emu places, gates 537, 691, 680 and 533.
Sent (764) and Eunice (939) in the towns now take a character to the zone (gates 537 and 691), and Cent and Eunice there
into the trial instance of its party (stored configuration of "Evolution quests"; worlds without the update keep the
direct way). The Portal of Dimension enters the labyrinth (`Season21EventsConfiguration.Entrances`, the Season 6 client
has no event window).

The labyrinth is map 115 (`Terrain116.att`), a solo instance (`OnePerPlayer`, total level 400): 41 floors of the data,
each the mission monsters (Great Sword 754 x2, Soldier 755 x3, Lancer 756 x3 of Kundun) and by 20 % the goblin (757,
2,000 points), with the floor's health, damage and defense multipliers, five minutes a floor; a cleared floor gives
50 points per floor number, the Ruud of the first seven floors (30 to 400) and at the floors 10, 20 and 30 one item of the
lower, medium and higher Maze of Dimensions box (bags 240-242); a floor which runs out ends the run.
Not ported: the 10 x 10 rooms of a floor which the server opens along `LabyrinthOfDimension_Path.xml` (the run is played
in the first room), Elena's Letter, the daily floor reset and the weekly leagues.

### Boss Battle Together (fork feature `boss_battle`)

Update "Add the Boss Battle Together of Season 21" (S21Emu `Event/BossBattleEvent.xml`): map 139 (`Terrain140.att`,
gate 681), up to 100 players from total level 800 for 10,000,000 zen, 5 minutes to enter and 5 to fight
(`BossBattleContext`). One of the five bosses (copies 919-924 of Illusion of Kundun, Medusa, Selupan, Nix and the God of
Darkness) spawns at 124/149 with 80,000,000 health and 2,000,000 more for every three players. The plugin
"Boss Battle damage" counts each player's damage on it (`EE B2` once a second). At the end, 1,000,000 damage gives a
Jewel of Harmony Box (20/78) and a Group Boss Battle Bonus Box (20/110), the most damage and the last hit two more bonus
boxes each, into the gremory case when it's on (`RewardDelivery`). Timetable 00:05 (`BossBattleStartPlugIn`).
Not ported: the 4th class and ticket requirement of Global (S21Emu has neither), the 145,690,000 health of Global (the
data keeps 80,000,000).

### Protector of Acheron (fork feature `acheron_guardian`)

Update "Add the Protector of Acheron of Season 21" (S21Emu `Event/AcheronGuardian.dat`): the event copies of Acheron
(map 92, `Terrain93.att`) and Deventer (map 96, `Terrain97.att`), game level 1 and 2, from total level 450, 30 minutes.
A rounds event of 30 one-minute waves, each ten monsters in every area of the data (Alkmaar 89/41, Ubaid 188/101,
Deventer 162/54) from the monsters of that area; a kill is 10 points, 100 points give an item of the Protection of
Acheron bag (164). Timetable 4:29, 12:00, 23:06 (`AcheronGuardianStartPlugIn`).
Not ported: the obelisks and free PvP of Global (the data has no obelisk monster), and the entry by Sir Lesnar.

### Imperial Guardian rework of Season 18 (fork feature `imperial_guardian_s18`)

Update "Apply the Imperial Guardian rework of Season 18" (S21Emu `Event/ImperialGuardian/ImperialGuardianRenewal.xml`):
every day from total level 1000, and at the completion one reward chosen by its chance
(`ImperialGuardianEventDefinition.CompletionRewards`): four (20 %) or two (40 %) Artifact Upgrade Stones (20/94) or a
Spider Artifact Fragment (20/95, 40 %), into the gremory case when it's on. The event already was a party instance with
its four maps, zones and day rotation. Not ported: the monster level 145 and its multipliers of the data (OpenMU scales
the monsters by the players' level instead).

### Not ported (no data)

- Arca War: S21Emu `ArcaBattle.xml` has the schedule and trophies but no obelisk monsters, guild registration rules or
  capture logic the client could show; logged, not built.
- Chaos Castle Survival of the Fittest: `ChaosCastleSurvival.dat` has the schedule and stages only; map 97 has no
  stage or ranking data. Logged, not built.
- The S21 Crywolf renewal: see `src/GameLogic/Crywolf` (element of the week, timings, gremory rewards).
