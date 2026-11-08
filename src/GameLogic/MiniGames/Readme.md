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
Loren Market and the zone are linked on foot (version 2: 532 at Loren Market's east edge to 533, 534 back to 535).
Sent (764) and Eunice (939) in the towns now take a character to the zone (gates 537 and 691), and Cent and Eunice there
into the trial instance of its party (stored configuration of "Evolution quests"; worlds without the update keep the
direct way). The Portal of Dimension enters the labyrinth (`Season21EventsConfiguration.Entrances`, the Season 6 client
has no event window).

The labyrinth is map 115 (`Terrain116.att`), a solo instance (`OnePerPlayer`, total level 400): 41 floors of the data,
each the mission monsters (Great Sword 754 x2, Soldier 755 x3, Lancer 756 x3 of Kundun) and by 20 % the goblin (757,
2,000 points), with the floor's health, damage and defense multipliers, five minutes a floor; a cleared floor gives
50 points per floor number, the Ruud of the first seven floors (30 to 400) and at the floors 10, 20 and 30 one item of the
lower, medium and higher Maze of Dimensions box (bags 240-242); a floor which runs out ends the run.
Not ported: Elena's Letter. The rooms of a floor are the next section.
A run starts when the client of its runner loaded map 115 (`StartsWhenEntrantsAreOnTheMap`, at most 55 s), not after the
10 s entering phase: the first floor places the runner in its entrance room, and a client still on the loader was placed
on a blocked tile and sent back to the Instance Zone.

#### Rooms of the labyrinth (fork feature `labyrinth_rooms`)

Update "Add the rooms of the Labyrinth of Dimensions of Season 21" (S21Emu `LabyrinthOfDimension.xml` and
`LabyrinthOfDimension_Path.xml`), plugin "Labyrinth of Dimensions rooms (Season 21)" (`LabyrinthRoomsPlugIn`, off by
default, the update switches it on once). While it's on and the client draws rooms (a `ILabyrinthRoomsViewPlugIn`, the
Babylon client), a run plays its floors through rooms (`LabyrinthRun`); other clients play the floors as before.

- A floor plays one of the 30 paths by chance, a board of 10 x 10 rooms, and starts in its entrance room. Each room shows
  an area (16 x 12 tiles) of one of 38 areas of other maps. Only the current room exists: entering it loads its area as
  the terrain of the run's copy of map 115 (everything else blocked, no safe zone) and moves the character to the side it
  came through (the first y is the top, the first x the left; the data doesn't say).
- A room has a portal on each side: closed, open, or opening with its mission. The client asks to move (`EE B8`); the
  answer is `EE B9`, then the board (`EE B6`) and the mission (`EE B7`) of the new room.
- Mission rooms (kind 3) have one goal of the data: kill 15, kill the eight monsters within 15 minutes, 1,500,000 damage,
  60 s of the MU Helper, or kill them within 15 minutes without a health (or mana) potion; the monsters (Great Sword,
  Soldier, Lancer of Kundun) come back while the mission isn't done. A forbidden potion or the end of the time starts
  it again; leaving the room does too. The main mission room needs all of its goals (kills, damage, helper time); its
  end clears the floor, with the clear points, Ruud and boxes of the floor (and the league). A kill gives the points of the
  floor, monsters have the floor's multipliers.
- A room opened for the first time gets the goblin (757, 2,000 points) by 20 % while fewer than 10 rooms are open.
- The minutes of the timed goals are the data's (15) and longer than the five minutes of a floor; operators can lower
  them. The experience goal (kind 7, unused by the data) counts as met.

#### Leagues of the labyrinth (fork feature `labyrinth_league`)

Update "Add the leagues of the Labyrinth of Dimensions of Season 21" (S21Emu `LabyrinthOfDimension_League.xml`), plugin
"Labyrinth of Dimensions league (Season 21)" (`LabyrinthLeaguePlugIn`, off by default, the update switches it on once).
While it's on, `Season21RoundsContext` scores the labyrinth through it:

- A cleared floor gives its first points (50 per floor) the first time it's cleared on the day (UTC) and its repeat points
  (25 per floor) after that; the first floor of the day adds 1,000, and a clear within 20 minutes adds up to 250
  (`FastFinish - FastFinish * (seconds * 100 / 1200) / 100`). Kills keep their points (goblin 2,000).
- Every cleared floor gives one Maze of Dimensions box (14/403-405) by the chances of the floor, instead of the round's
  random item; the update lets the boxes be dropped open into the items of bags 240-242.
- The points of the run also count for the week: the schedule lists the days of the month (weeks from the 1st, 8th, 15th
  and 22nd, the reward day on the 29th; the 30th and 31st count nothing). A character's first points of a week put it into
  the first group of its tier with fewer than five characters. When the next week starts, each group is ranked by points
  (a tie goes to the earlier one): ranks 1-2 go up a tier, rank 5 down (Bronze and Legend are the ends).
- On the reward day, entering the labyrinth gives Silver 2, Gold 5 and Legend 10 Higher boxes (14/405), once. A new
  month starts every character in Bronze.
- The state is kept in `state/labyrinth_league.json` (`CharacterStateStore`). The Babylon client gets `EE B5` on entering
  and after the run (docs/babylon.md, "Events").

Not ported: Webzen's weekly progression on Tuesday 00:00 UTC (the S21Emu schedule by day of month is the data) and the top
100 rewards (no data).

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
Not ported: the obelisks and free PvP of Global, and the entry by Sir Lesnar. The obelisks are the Arca War's
(`src/GameLogic/ArcaWar/Readme.md`), which plays on the plain maps 92 and 96.

### Imperial Guardian rework of Season 18 (fork feature `imperial_guardian_s18`)

Update "Apply the Imperial Guardian rework of Season 18" (S21Emu `Event/ImperialGuardian/ImperialGuardianRenewal.xml`):
every day from total level 1000, and at the completion one reward chosen by its chance
(`ImperialGuardianEventDefinition.CompletionRewards`): four (20 %) or two (40 %) Artifact Upgrade Stones (20/94) or a
Spider Artifact Fragment (20/95, 40 %), into the gremory case when it's on. The event already was a party instance with
its four maps, zones and day rotation. Not ported: the monster level 145 and its multipliers of the data (OpenMU scales
the monsters by the players' level instead).

### Crywolf renewal of Season 21 (fork feature `crywolf_s21`)

Update "Apply the Crywolf renewal of Season 21" (webzen guide 425, S21-1 Update 3; S21Emu `Event/Crywolf.dat` for the
timings): the army of the event takes the element of the week (`CrywolfEventDefinition.WeeklyElements`: fire, water,
earth, wind, darkness, changing every seven days from `ElementRotationStart`, a Thursday 00:00), Balgass appears 6:40
into the battle, the rewards of the contractors and heroes go into the gremory case or the inventory
(`RewardsIntoGremoryCase`, `CrywolfRewards`), and the states last 300, 60, 120, 900, 15 and 10 seconds. Without the
update the event stays Season 6. Version 2 of the update makes Balgass (349) immune to debuffs (`Stats.IsImmuneToDebuffs`:
a monster with it takes no magic effect, since all of a monster's effects come from its attackers; one check in
`MagicEffectsList.AddEffectAsync`) and opens the altars on Monday 9:30: the altars can be contracted in the ready state,
so the event starts on Monday at 9:30 minus the two notifications (9:24), unless the operator changed the Season 6
schedule. Not ported: the auto move of the Event Map window (client).

### Chaos Castle Survival of the Fittest (fork feature `chaos_castle_survival`)

Update "Add the Chaos Castle Survival of Season 21" (S21Emu `Event/ChaosCastleSurvival.dat`, webzen guide 161): map 97
(`Terrain98.att`, gate 443) with Chaos Castle 15 and 16 (644, 645) at the 99 places of the data, spawned by the game; a
Chaos Castle (`MiniGameType.ChaosCastleSurvival`, run by `ChaosCastleContext` with its stages, rules and PvP halving) for
5 to 70 players from level 280, 5 minutes to enter, 10 to play, 4 to leave. `ChaosCastleSurvivalStartPlugIn` opens it at
1:25, 4:35, 5:00, 7:00 and every two hours from 9:00 (21:04) on the days of the tournament.

The plugin "Chaos Castle Survival of the Fittest (Season 21)" (`ChaosCastleSurvivalPlugIn`, server time) makes the games of
a month a tournament: try-outs on the 8th to 10th, semi-finals on the 11th and 12th, the final on the 13th at 1:25 only
(the days of the data). A game counts 1 point per killed monster, 2 per killed player and 3 for the win; the try-out
points add up, the best 20 of them may enter the semi-finals, the best 10 of those the final (`MayEnterAsync` refuses the
others), and the winner of the final gets the Conqueror's Badge (13/276) into the gremory case or the inventory. The
points are kept in `state/chaos_castle_survival.json`.

Not ported: the Hall of Fame, the Battle Core exchange of the event items and the `ExperienceTable` of the data (no
meaning given).

### Illusion Temple League (fork feature `illusion_temple_league`)

Update "Add the Illusion Temple League of Season 21" (S21Emu `Event/IlusionTempleLeague`): on maps 98 and 99 (maps update
10) the Cursed Statue 658 and the ten Captured Stone Statues 659-668 of `IllusionTemple_League_NPC.xml`, spawned by the
match; a league temple on each (`MiniGameType.IllusionTempleLeague`, levels 1 and 2) for 2 to 10 players from total level
400 (a bracket above the maximum level, so the progression mode maps it), entered at the waiting area (gates 444/445), 5
minutes to enter, 15 to play, 1 to leave (`Time` of the data). `IllusionTempleLeagueStartPlugIn` opens them 5 minutes
before 13:00 to 22:00 (the schedule of the data without its test row).

The match is `IllusionTempleContext` with the renewed rules (Webzen guide 129, muonlinefanz): the teams are the two guilds
(the first to enter are the allied forces) and keep their parties; the Cursed Statue gives the Cursed Hollow (the relic
14/64), its carrier seals it into a free stone for a point, a player of the other team unseals a stone (after
`StoneSealProtection`) for the hollow, which takes the point away; one hollow is in play at a time (an unsealed stone
replaces a standing statue); after a seal the statue comes back after 5 seconds with the chance
`100 - occupied stones * StatueSpawnRateReductionPerPoint` percent, tried again every 5 seconds. 7 points end the match;
the winner is decided by occupied stones, kills, then the level sum (S21Emu). The Season 6 skills and skill points stay.

The plugin "Illusion Temple League (Season 21)" (`IllusionTempleLeaguePlugIn`, server time) makes the matches a monthly
league: eliminations on the 3rd to 9th (5 matches a guild, Webzen; a player limit is a setting), their stones, kills and
level sums add up; the best four play the semi-finals on the 11th at 14:55 (slots of `RankTeam`: first and fourth in
temple 1, second and third in temple 2), the winners the final on the 12th at 14:55 in temple 1; a draw of the final is
not kept, so it can be played again. `MayEnterAsync` lets in a party of at least 5 members of one guild (Webzen; S21Emu's
`MinUserInParty` has no value), two guilds a temple, half of the players a guild. The players of the champion in the final
get the Wings of Conqueror (12/268) once when they enter the world from the reward day (13th) to the end of the month.
`DamageReduction` scales the PvP hits in the league (`IPlayerDamageFactorPlugIn`). The state is kept in
`state/illusion_temple_league_guilds.json` and `_players.json` (guilds by their persistent id).

The Babylon client gets `IIllusionTempleLeagueViewPlugIn`: the league of the guild (`EE BE`, on entering the world on a
league day, at the start and the end of a match) and the stones with the place of the hollow (`EE BF`, on each change);
layouts in `docs/babylon.md`.

Settings without a source (ours): `MaximumPlayerEntries` 0 (none), `DamageReduction` 0, `StatueSpawnRateReductionPerPoint`
0, `StoneSealProtection` 10 seconds. Not ported: the title of the champion (no title system), the event point rewards of
`EventMapPointIntegration.xml`, the `StageTypeSettings` (`PlayDuration`, `SpawnMonster`; the data has no rows and maps
98/99 no monsters) and the S21 Mait NPC entrance (the client enters with `EE B1`).

### Daily entry limits

`Season21EventsConfiguration.DailyEntryLimits` caps how often a character enters an event a day (UTC); the event rules
update sets the numbers of S21Emu `Event/EventEntryLimit.dat`: 15 for Blood Castle, Chaos Castle, Devil Square,
Doppelganger, Imperial Guardian, Illusion Temple and the Boss Battle, 10 for the Labyrinth. `EnterMiniGameAction` refuses
an entry above the limit and counts a successful one (`EventEntryCounter`). The counts are kept per character and day in
`Season21EventsConfiguration.EntryLimitStateFile` (default `state/event_entries.json`, the state folder the installer
mounts), shared by every game server of the process, so a restart keeps them; a new day starts the counts of a
character again. Game servers in separate processes need the same state folder.

### Not ported (no data)

- Arca War: plan in the S21 hub notes ("[E2] Arca War").
