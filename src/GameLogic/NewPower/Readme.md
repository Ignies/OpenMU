# New Power (fifth class: Nuke, Bolt, Wide)

Fork feature id `new_power`. `NewPowerFeaturePlugIn` is the switch (off by default, `IDisabledByDefault`); the Babylon
client's Admin > Systems turns it on. The data comes with the mandatory update "Add the New Power of Season 21"
(`AddSeason21NewPowerUpdatePlugIn`, new and installed databases) where the Ability Enhancement Core is installed, and
with version 5 of the core's update where the core comes later (`Season21NewPowerInitializer`). A Season 6 world
without the core stays as it is.

## What it does

- **Kind.** The extra damage card in core slot 0 gives its skill as an item skill (`AbilityCardDefinition.Skill`):
  20/235 "[Extra Damage] Nuke" gives 2086 "Nuke(Activated)", 236 Bolt 2087, 237 AoE 2088 (S21Emu `Skill.xml`
  :1037-1039; the card's option 80/81/82 names the kind). The kind of that skill works; without a card, the kind of an
  active effect 421-423 "Extra Nuke/Bolt/AoE Damage" (S21Emu `Effect.txt` :405-407, client groups 334-336) works.
  Only one card fits slot 0, so one kind at a time (Webzen guide 340 and the Season 19 event page: one of the three
  cards in the extra slot).
- **Trigger.** A hit of a skill of the skill enhancement tree (a fourth class skill, `EnhanceTreeNode.Skill`) or of a
  card skill (`AbilityCoreService.IsCardSkill`): "Extra DMG activates when using Upgrade Skills (4th) and Ability Skills
  (5th)" (Webzen guide 340, https://muonline.webzen.com/en/gameinfo/guide/detail/340). Pick: once per cast, as a
  500 ms cooldown per player (`CooldownMilliseconds`), since an area cast hits many targets in one go; no source gives
  a chance or cooldown.
- **Targets.** Nuke 1, Bolt 2, Wide 10 at most (Webzen guide 340: "Maximum of 1 / 2 / 10 Target(s)"). The others are
  the nearest living monsters around the hit one, not players or summons. Pick: Bolt within 2 tiles, Wide within 3
  (no source gives a range).
- **Damage.** `ExtraDamage x DamageRate + AttackIncrease` of the kind (`NewPowerStats`), dealt like a bleeding hit
  (`ApplyBleedingDamageAsync`: no defense, no miss). Sources: the card value 100/200/480/940/1580/2400 by card level
  (S21Emu `AbilityOption.xml` option 80-82, `AbilityParticleBook.xml`), "Nuke/Bolt/Wide ATK Increase" (options 29-31).
  Webzen guide 340 ranks the kinds "Greater / Medium / Lesser" extra damage but gives no number. Pick: the card value
  counts 1, 2/3 and 1/3 (`DamageRate`), the 3:2:1 of every per-kind value of the data (ATK increase 30/20/10, critical
  rate 1.875/1.25/0.625, the wing core's 120/80/40); and no defense applies, as the values are far below a fifth class
  monster's defense.
- **Rates.** Options 68-70 (critical), 71-73 (excellent), 74-76 (double), 77-79 (triple) of the kind (Webzen guide 343
  names them). Picks: excellent x1.2 plus the excellent damage bonus, else critical plus the critical damage bonus;
  triple before double, as option 60.
- **Look.** Each target hit gets a skill animation of the kind's skill (caster the player, target the monster); the
  Babylon client draws it (`versions/shared/skills/common/new_power`). A Season 6 client shows nothing.
- **Fifth class quests.** The trial steps of the quests 10-12 last 60 s (S21Emu `QuestInstanceManager.xml`
  `Time="60000"`; Webzen guide 338 "within 1 minute") and give the step's effect 421/422/423 for that time
  (`EvolutionTrialStep.PowerEffect`; guide 338: each quest gives a temporary "Extra Nuke/Bolt/Wide DMG" buff) with
  `TrialExtraDamage` 50 (S21Emu `Effect.txt` first value; its second, 10, is not read).

## Not done

- WingCore option 19-21 ("Extra Nuke damage increased" 120, Bolt 80, Wide 40) and 22 ("Extra DMG reduction" 50), and the
  Legendary bonus option 4-6: the fork has no wing core or legendary bonus options yet. Their effect is a power-up on
  `NukeAttackIncrease` (...), one row each, when those options exist.
- "Extra DMG DEF" (guide 340): no attribute.
- Damage colours: the strike shows as a plain hit; the rates still multiply it.
