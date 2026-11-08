# Arca War of Season 21 (fork feature `arca_war`)

Update "Add the Arca War of Season 21" (S21Emu `Event/ArcaBattle.xml`, `Monster/Monster.xml`, `Effect.txt`, `Define.lua`,
and the webzen guide of the Arca War for what the data doesn't have), plugin "Arca War (Season 21)" (`ArcaWarPlugIn`, off
by default, the update switches it on once; version 2 adds the effect 187 and Sir Lesnar, and an existing configuration
gets the new settings with their defaults). One `ArcaWarContext` per game context runs the war on the plain maps 92 and
96 (Acheron and Deventer of the Arca War, which only the war uses; the Protector of Acheron plays on copies of them).

## Stages

Daily at 10:45 (server time, every day of the week): the guild masters register their guilds (1 minute, at most 6, first
come, each paying `RegistrationFee`, 10 Signs of Lord (14/21 level 3) of the webzen guide; a stack counts by its
durability, and a master without them gets `NotEnoughSignsOfLord`), then the members of the registered guilds (1 minute, at most 20 a guild with the master), a minute of waiting, 30
minutes of war, 10 minutes to leave; then everyone left on the maps goes to the safe zone. A guild with fewer members than
`MinimumGuildMembers` (1) drops out before the war; with fewer guilds than `MinimumGuilds` (1) the war is cancelled. The
guilds go in turn into the three spawn groups (two on Acheron, one on Deventer); the start moves every registered member
online to a random spawn area of its group, and a member who comes later enters with the request "enter". A death sends
the member to the town of Acheron (the safe zone map of 92 and 96), and back in the world it's slower for 90 s (effect 184,
`DeathPenaltySpeedFactor` 0.5, no data).

A game master opens the registration at once with `/arcawar` (`ArcaWarChatCommandPlugIn`); each further `/arcawar`
ends the current stage at the next tick of the war.

## Obelisks

Three obelisks (the Fire Tower, monster 598, a destructible): Alkmaar 89/41 and Ubaid 188/101 on Acheron, Deventer 162/54,
with 500,000 health, 300,000 once a guild holds one, back 20 s after it fell. Each war gives each obelisk one of the five
elements at random (webzen; `ArcaObeliskDefinition.Element` fixes one). The shield of an obelisk is down for a guild
whose members stand on all three of its auras (within `AuraRange`, 1) for `CaptureSeconds` (5); both are settings, the
data has the auras but no rule. Only that guild can hurt it, and only while it keeps the auras; the member whose hit
destroys it takes it for the guild (50 points). The holder can't hurt its own obelisk.

## Fighting, points and rewards

During the war, on the maps: members of one guild can't hurt each other (`FriendlyFire` off), members of other guilds fight
without the player killer penalty, players outside the war can't fight the ones in it. The damage between members of
other guilds is reduced by `OppositeTeamDamageReduction` and, with friendly fire, within a guild by
`SelfTeamDamageReduction` (percent, S21Emu `DamageReduction` has the keys but no values, so both are 0; the plugin
point `IPlayerDamageFactorPlugIn` scales the hit). Points (webzen): 5 a monster of the
groups, 10 a member of another guild, 50 an obelisk. A monster and the first kill of each enemy give the Trophies of Battle
(13/147). The monster groups (592-596 twice on Acheron, 611-615 on Deventer) are filled up to ten every ten minutes, each
monster at a random spot of its group.

At the end every participant online gets the experience of its points (0: 300,000, 11: 500,000, 51: 700,000,
101: 1,000,000, 301: 2,000,000, 501: 3,000,000), and the guilds holding obelisks keep the buff of each element for 7 days,
also on members who come online later (`state/arca_war.json`): fire 179 +25 skill damage, water 180 +50 defense, earth 181
+5 % critical damage chance, wind 182 +30 damage, darkness 183 10 % damage reduction (values of `Effect.txt` by effect
number, elements of `Define.lua`; the webzen guide pairs earth with damage and wind with the critical chance).

## Acheron reward and trophies

The members of a guild holding obelisks have, while they are on a map of `BonusMaps` (Acheron, 91; webzen: "only in
Acheron"), the effect 187 (Arca Battle Reward 6) until their holding ends: `BonusExperiencePercent` 20 on
`BonusExperienceRate` and `BonusDropPercent` 10 on `ItemDropRateBonus` (values 1 and 2 of `Effect.txt`; the Russian
guide says 20 % drop). The drop bonus raises the chance of every chance based drop group of a kill
(`DefaultDropGenerator`). It comes on entering the map (or at the end of the war, on login) and goes on leaving it.

Sir Lesnar (584) stands in Lorencia, Devias and Noria (S21Emu `MonsterSetBase`) with the window `NpcDialog`, and
exchanges 1 to 100 Trophies of Battle at once (`TrophiesOfBattleCrafting`, mix 53, only while the plugin is on): the
rate is 50 % for the first trophy of each ten plus 5 % for each further one (`TrophiesOfBattleMixRate` 50,
`TrophiesOfBattleMixRateAdd` 5 of `GameServerInfo - ChaosMix.dat`, read with the tiers of the guide), and a success
gives one random item of the bag of the ten (bags 153-162, the result items whose `Reference` is the tier; counts as
durability). A failure takes the trophies.

## Not ported

The ranking of the guilds by registered Signs of Lord of webzen (the war takes the first six which pay), the
registration of a Season 6 client, which has no packet for it (it gets the stages as golden messages), and its dialog
of Sir Lesnar.

## Babylon packets

`C1 EE BA ArcaWarRequest` (client), `BB ArcaWarRequestResponse`, `BC ArcaWarStageState`, `BD ArcaWarObelisks`
(docs/babylon.md, "Events").
