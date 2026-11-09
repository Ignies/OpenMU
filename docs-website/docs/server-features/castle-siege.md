# Castle Siege mini-map and guild commands

During the battle (`Start` state), the guild master of each participating
alliance's master guild receives mini-map updates every three seconds while on
the Castle Siege map. Ordinary guild members and masters of allied member guilds
do not receive these tactical snapshots.

Updates show up to 1,000 players on the commander's own siege side, including
allied guilds, together with living castle gates and Guardian Statues. Players
on other maps and opposing sides are excluded. Updates stop when the battle
ends or the commander leaves the siege map.

The server pushes these updates automatically. The existing client protocol
has no mini-map subscription request or acknowledgment, so opening the UI does
not require a new request packet.

An alliance master can select one of seven command groups and place an attack,
defend, or wait order at a target coordinate. The server relays the selected
group and command to players on the same siege side and map, including the
commander. The group selects a numbered marker slot; it does not select a siege
side. Attempts are limited to one per second per player, including unauthorized
attempts, to keep repeated requests from flooding the siege's shared lock and
other players' connections.

The client must support the Castle Siege commander interface and the player
position (`B6`), NPC position (`BB`), and guild command (`B2:1D`) packets. Within
each update, the player packet replaces the position layer and the NPC packet
adds structures afterward. The commander position is already known locally;
the existing position packet has no separate commander-identity field.

## Player kills during the battle

During the battle, players of different siege sides may attack and kill each
other without becoming outlaws, and the attacks don't start the self-defense.
The attacking sides are enemies of each other as well as of the defenders.
This applies only on the Castle Siege map and only to players with a siege
side. Killing a player of the own side, or any player outside the battle, is
still a normal player kill.

## Gate and statue list

The list of castle gates and Guardian Statues, with their health, upgrade
levels and positions, is only answered while the Senior's window is open, and
only shows the structures to the guild master and assistant master of the
castle owner guild. Other players get an empty list.

## Friendly fire during the battle

During the battle, players of the same siege side can't damage each other,
including with area skills and pets. This applies only on the Castle Siege map
and only to players with a siege side.

## Registration rules

A guild master registers the guild (or its alliance) when the guild has
`RegisterMinMembers` members and the master's level plus master level reaches
`RegisterMinLevel`. Season 6 data asks for 20 members and level 200. The update
"Season 21 event rules" (version 3) sets the Season 21 values of S21Emu
(`GameServerInfo - Event.dat`: CastleSiegeMinMemberToReg=3,
CastleSiegeMasterMinLevelToReg=400), unless an operator changed them. The level
is a Season 21 total level: with the progression in the normalized or stars mode
it is mapped like the other Season 21 thresholds (the evolutions, the map
entries), so a level 400 master of the original mode is a level 190 master of
the default normalized mode.

## One siege per world

The game servers of a world (one game configuration) share one siege. The
first game server which runs its periodic tasks owns the siege state: the
schedule, the registrations, the marks, the selected guilds and the battle.
The other game servers find the same state for their players, so a guild
registered on one server is registered on all of them, and a restart clears or
loads the stored registrations once. When the owning game server stops running
the siege for 30 seconds (stopped or removed), the next game server which runs
its periodic tasks takes it over from the stored state. The siege's
notifications (registration open, the countdown, the battle) and the tax changes
reach the players of every game server of the world.

The battle (gates, statues, crown, life stones, machines) takes place on the
owning game server's instance of the Valley of Loren. To have every player on
that one battlefield, host the Valley of Loren on one game server only (remove
it from the maps of the other server configurations): the players of the other
game servers then enter the hosting server's instance (see the server settings
on shared maps).

## Battle end and cycle end

At `End` the alliance which holds the castle (the crown captor, else the
defender) becomes the stored owner; a new owner starts with no taxes, tribute
or hunt zone. Every participant who was on the battlefield for
`ParticipantRewardMinSeconds` gets the reward item, also on another game
server of the world (queued for the next login when the inventory is full or
the player is offline), the winning side's guilds
get their guild score, the life stones die and the machines despawn. Five
minutes later `EndCycle` clears the registrations, marks, selected guilds,
sides, castle NPCs and a game master's crown test (`/siege crown`), and opens
the next registration. A game master who forces a registration state after the
guild selection (`/siege register` after `/siege end`) gets the same clean
start.

## Restarting after a missed cycle end

When the server wasn't running at the end of a cycle (`EndCycle`), it notices
on the next start: if the selected guilds of a battle are still stored while
the schedule is already before the guild selection (`Notify`) of the next
cycle, the old registrations, marks and selected guilds are cleared, as the
cycle end would have done. A cycle which ended without selecting guilds leaves
no trace, so its registrations stay.

## Teleporting over gates

While a castle gate is closed, players can't use the Teleport or Teleport Ally
skill to cross it. The gate blocks a teleport when the straight line between
the start and the target passes through its area. An opened or destroyed gate
doesn't block teleports. The castle walls are ordinary terrain and are not
covered by this rule.
