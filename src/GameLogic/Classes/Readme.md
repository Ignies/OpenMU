# Classes

The class model of fork worlds: sixteen base classes (the seven of Season 6 and the nine of Season 21) with up to five
evolutions each. Fork feature ids `class_codes`, `classes_s21`, `evolutions_45`.

## Class code (`ClassCodes`)

`code = base << 3 | evolution`, a byte: base 0-15 in the Season 21 order (`BaseClassCode`: DW, DK, FE, MG, DL, SU, RF,
GL, RW, SL, GC, LW, LM, IK, AL, CS), evolution 0-4 for the first to the fifth (`ClassEvolution`), 5-7 and bit 7 reserved.
Magic gladiator, dark lord, rage fighter and grow lancer have no second evolution.

`CharacterClass.Number` stays the database key and the code is computed from it, nothing is stored twice:

- The 18 Season 6 classes keep their numbers (base * 4 + generation), so no stored character changes.
- Every other class and evolution is numbered `128 + code` (fourth and fifth evolutions of the Season 6 classes, and
  the nine new classes), which never collides with Season 6's 0-27.

A Season 6 client has 5 bits for the class. `GetSeason6Number` gives it the third evolution for a fourth or fifth one
(the appearance serializer uses it); the classes after Season 6 have no Season 6 number, so the Season 6 character list
leaves their characters out. Clients of the Season 21 protocol get the code.

## Evolution rules (`EvolutionRules`)

The first three evolutions keep their quests and levels. Season 21 asks for a total level (level plus master level) of
800 for the fourth and 1200 for the fifth (S21Emu `Quest/Quest.xml`). The progression mode reads that total like a map
requirement:

| Mode | Fourth | Fifth |
|---|---|---|
| Original (or progression off) | level + master level 800 | 1200 |
| Normalized | level + master level T(800) = 335 | T(1200) = 465 |
| Stars | power level E = level + stars' worth, 335 | 465 (needs a star, the level cap is 400) |

`EvolveCharacterAction` moves a character along `NextGenerationClass`, checking the rule for the evolutions which have
one; the game master command `/evolve` (`EvolveChatCommandPlugIn`, off by default) skips it.

## Data

The classes are added by the optional update `AddSeason21ClassesUpdatePlugIn`
(`Persistence/Initialization/CharacterClasses/CharacterClassInitialization.Season21.cs`, `Season21ClassData.cs`):

- The fourth and fifth evolution of each Season 6 class use the formulas of its third and may use its items and skills.
- The nine new classes take base stats, health, mana and points per level from the S21Emu data
  (`Character/DefaultClassInfo.txt`); their damage, defense, rate and speed formulas come from the Season 6 class of the
  same kind (`Season21ClassArchetype`), as the S21Emu data keeps those in its binary. The Crusader row is marked
  unfinished in the source (its four growth values equal the dark knight's).
- Every first evolution can get created without a level (S21Emu `CreateType = 0`). An operator who sets
  `LevelRequirementByCreation` on a class without a Season 6 unlock flag gets it checked by the server
  (`CreateCharacterAction`): another character of the account has the level, or the account has unlocked the class.

## Seams for later tracks

- Skills (S1, S2): a class gets its skills through `Skill.QualifiedCharacters`, the start skills of S21Emu
  `Character/CharacterSkillInit.xml` through the character creation plugins.
- Stat specialization (S21Emu `Character/StatsSpecialize.txt`) is open: the data has the thresholds, not the curve.
- Evolution quests (Q) call `EvolveCharacterAction.TryEvolve(player, true)`.
- Trees (T) read `CharacterClass.GetEvolution()` for the enhancement tree of the fourth evolution.
