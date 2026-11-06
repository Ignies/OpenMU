# Monster soul converter

The monster soul converter of Season 21. A converter is an item of one of five tiers (14/460-464, sold by Priest James
in the Ruud shop). A character of the tier's total level (level + master level: 400, 800, 1150, 1250, 1450) uses it:
it draws two soul missions of the tier (S21Emu `MonsterSoulConversionMissionInfo.txt`: a monster on its map, 500 souls,
45 % a kill). When all missions are done, the player chooses a reward kind the tier offers; each draw of its item bag
is rolled and the converter is used up.

- `SoulConverterPlugIn` (off by default, the world's switch) holds the tiers (`SoulConverterConfiguration`) and the state
  per character (`CharacterSoulConverter` in `StateFile`, `state/soul_converters.json`): the activations of the week and
  the active converter. Using the item (`SoulConverter1ConsumeHandlerPlugIn` .. `5`) activates it; the item is consumed
  only when the activation works, else the player gets the reason.
- Rules from the data and Webzen's guide: three activations a week (`ActivationsPerWeek`), one active converter at a time,
  the converters of a week expire when the next week starts (`WeekStartsOn`, Thursday 00:00 UTC), party members on the
  same map with the same mission share the soul of a kill (`ShareWithParty`). The kinds: Ruud, jewels, elemental options,
  and at the fifth tier accessories.
- Design (no source): the activations count per character, not per account; the soul of a kill is rolled once and shared.
  Items go to the gremory case first (`Rewards/RewardDelivery`), Ruud to the character.
- The Babylon client opens the window with `C1 EE A8`, claims with `C1 EE A6` and gets `C2 EE A5` and `C1 EE A7`
  (`docs/babylon.md`). A Season 6 client can use a converter and gets its souls, but has no window to claim the reward.
- The data comes with the update "Add the monster soul converter of Season 21" (`Persistence/Initialization/VersionSeasonSix/Season21`).
