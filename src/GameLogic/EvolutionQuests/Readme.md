# Evolution quests (fourth and fifth class)

Fork feature id `evolution_quests` (bit 24). The quests are data, added by the optional update "Add the evolution
quests of Season 21" (`AddSeason21EvolutionQuestsUpdatePlugIn`, `Persistence/Initialization/VersionSeasonSix/Season21/
Season21EvolutionQuest*.cs`); this folder holds the rules and the trial.

## Chain

A chain is the quests of one NPC, group and class whose last quest has the reward `QuestRewardType.CharacterEvolution`
(`EvolutionQuestRules`). Its quests start one after the other (the one after the last finished quest of the group), the
first only when the character meets the level rule of the evolution (`EvolutionRules`: Original total level 800 / 1200,
Normalized level plus master level 335 / 465, Stars power level 335 / 465). `QuestStartAction` and the quest list of
the NPC (`GetAvailableQuestsOfOpenedNpc`) apply it; quests of other groups are not affected. The evolution reward runs
`EvolveCharacterAction.TryEvolve(player, true)`; `QuestCompletionAction` refuses the completion before taking anything
if the character can't evolve.

Season 21 data (S21Emu `Quest/Quest.xml` 7-12, `QuestObjective.txt`, `QuestReward.xml`), group 21, for every class:

| Quest | NPC | Kills | Reward |
|---|---|---|---|
| 7 Eligibility Test | Sent (766, Trial Area) | Sent 763 x1 | 20 points |
| 8 New Battle (1) | Sent | Deep Dungeon Skeleton Warrior 767, Larva 770, Cyclops 771 x5 each | 30 points |
| 9 New Battle (2) | Sent | Sent 763 x1 | 50 points, Scroll of Gray Oblivion, fourth evolution |
| 10 New Power Nuke | Eunice (941, Trial Area) | Eunice 937 x1 | [Extra DMG] Nuke |
| 11 New Power Bolt | Eunice | Eunice 937 x1 | [Extra DMG] Bolt |
| 12 New Power AoE | Eunice | Eunice 937 x1 | [Extra DMG] Wide, 200 points, fifth evolution |

## Trial (`EvolutionQuestFeaturePlugIn`)

- Sent (764) in Lorencia 131/147 and Eunice (939) in Devias 193/11 take a character whose next class is a fourth
  (fifth) evolution and who meets its level rule to the Trial Area (map 121), else they say why.
- Starting a step on the Trial Area, or asking for it again while it's active, or entering the map with it active,
  spawns the step's monsters for the player (`EvolutionTrial`, `EvolutionQuestConfiguration.Steps`, the S21Emu
  `QuestInstanceManager.xml` positions, each within 3 tiles). They respawn until the trial ends: the time limit (10
  minutes, 5 for quest 8 as its "time" condition), the player leaves the map, completes the quest or starts another.
- A kill of a trial monster counts for the party members of the killer on the map, too.
- Experience hold (`HoldExperienceUntilFourthEvolution`, on by default): a character whose next class is a fourth
  evolution with a quest, and who meets its level rule, gains no experience until it evolved (a blue message once per
  session). It only applies to classes which have such a quest, so a world without the update is not affected.
- Babylon client: `C1 EE C4 EvolutionQuestState` on every map entry, completion and when the hold starts;
  `C1 EE 13 ClassEvolved` to the player and its observers (docs/babylon.md). The Season 6 client uses the quest
  packets it knows (F6) and shows the fourth and fifth evolution as the third.

## Party instance

With the update "Add the party instances of Season 21" (feature `party_instances`) Sent and Eunice take the character into
the copy of the Trial Area of its party (`MiniGameType.PartyInstance`, game level 2, up to 5 players,
`src/GameLogic/MiniGames/Readme.md`): each party has its own monsters, and the copy goes away when the party left.
Without the update the Trial Area stays one shared map, where each player's steps spawn their own monsters.

## Stand-ins

- The S21 damage and defense multipliers of the steps (quest 9: x1.3 / x1.2), the party damage rules and the giant
  mount quests 13-14 are not applied. Sent 763 has no experience level in the data and takes 800; Eunice 937 / 938
  are 10-health placeholders in the data and are kept.
