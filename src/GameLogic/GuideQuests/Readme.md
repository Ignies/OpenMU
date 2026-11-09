# Guide quests (quest guide of Season 21)

Fork feature id `muquest` (bit 18). The steps are data, set by the optional update "Add the guide quests of Season 21"
(`AddSeason21GuideQuestsUpdatePlugIn`, `Persistence/Initialization/VersionSeasonSix/Season21/Season21GuideQuestsInitializer.cs`,
`Resources/Season21/GuideQuests.json` from `Generator/season21-guide-quests.mjs` over S21Emu `QuestGuide/QuestGuide.txt`);
the update turns the plugin "Guide quests (Season 21)" (`GuideQuestPlugIn`) on, later applications keep the operator's
steps and switch.

## Chain

- 186 steps in four chapters, from "Move to Lorencia" at level 1 to "3rd Class Upgrade Quest Completed" at 400. A step
  names the base classes which get it (the town steps of Lorencia, Noria and Elbeland are per class, the class quest steps
  leave out the classes without a second evolution), so every class has 164. A character goes through the steps of its
  class in the order of the configuration; the next one is the one after the last completed.
- A step has no NPC: the client asks to accept it (`C1 EE C1`, action 1) when the character's level reaches its
  minimum level, and to complete it (action 2) when its objective is met. Only the step the client shows is accepted
  or completed, so a late click does nothing.
- Objectives (`GuideQuestObjective`, the kinds of the data): 1 collect (items in the inventory, a stack counts its
  pieces; the completion takes them), 2 hunt (kills of the monster number by the character itself, up to the count),
  3 reach a level, 4 finish a Season 6 class quest (the last finished quest of group 0 has at least the number: 1 second
  class, 2 hero status, 6 third class; a character which has the class already counts), 5 go to the spot (within
  `ArrivalDistance` tiles, 5, on the map).
- Rewards: experience (`AddExperienceAsync`, the normal experience path), money and items (`RewardDelivery`, inventory
  first, then the gremory case). Reward items the item catalog doesn't know are dropped by the update.

## State

Per character in `StateFile` (`state/guide_quests.json`, `CharacterStateStore`): the last completed step, the accepted
step and its progress (kills, or 1 when the spot was reached). Accepting and completing write at once, kills at most
every `SaveInterval`. Collect, level and class quest progress is read live.

## Client

Only the Babylon client: `C1 EE C0 GuideQuestState` on every map entry, level up, counted kill, arrival and as the answer
to a request (docs/babylon.md "Quests"). The Season 6 client has no guide window and gets nothing.

## Stand-ins

- The data has two reward numbers per step; the first is taken as experience and the second as money (a level step gives
  1.6 million and 100,000 at level 290). Not confirmed by a source.
- The teleport of the S21 guide window (the gate of the data) is not offered; the gate is sent for the client to show.
- Party kills don't count; the item level and option columns of the data (always 0) are not read.
