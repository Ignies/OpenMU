# Ability Enhancement Core

The card system of the fifth evolution (Season 21), fork feature id `ability_core`. `AbilityCoreFeaturePlugIn` holds it
as its configuration (off by default); the update "Add the Ability Enhancement Core of Season 21" fills it from the S21Emu
data (`Skill/AbilityEnhancementCore`) and turns it on.

## Slots (`AbilityCoreSlots`, `Character.AbilityCore`)

The cards of the core are items in the character's storage `AbilityCore` (migration `AddSeason21AbilityCore`), the slot
is the item slot:

| Slot | Takes | Opens at (Season 21 total level, setting) |
|---|---|---|
| 0 | an extra damage card (nuke, bolt, wide) | the fifth evolution |
| 1-3 | a skill card of the class | 1200, 1300, 1400 (`SkillSlotLevels`) |
| 4-8, 9-13, 14-18 | option cards, a row each: blue, green, purple | 1200, 1250, 1300 (`OptionRowLevels`) |
| 19-21 | the elite card of the row's color | with its row |

Every slot needs the fifth evolution. The levels are read through the world's progression mode like every Season 21
requirement (`AbilityCoreRules`, `EvolutionRules.IsMet`): Original the total level, Normalized level plus master level
against T(R), Stars the power level. The levels and the color rule (`RowsMatchColor`) are from the webzen guides; the data
has no slot rule. A game master opens the slots for a test by the levels of the mode: `/setmasterlevel` (original,
normalized) or `/setresets` (stars, where the world has no master levels and `/setmasterlevel` says so).

A card takes effect while its slot is open and it fits: its kind, the row color, its class (`BaseClasses`), for a skill
card the fourth evolution skill of the skill enhancement tree it needs (`RequiredSkill`, learned or available); an elite
card needs fitting cards in the five option slots of its row and no higher level than the lowest of them. The state packet
names the active slots.

## Effects

A card carries one option as an item option of the type "Ability Card Option" (number = the S21Emu option index, the value
by the card level = the item level). `AbilityCoreService.RefreshAsync` applies the power-ups of the options of the active
cards (an option takes effect for the classes of `AbilityCardOption.BaseClasses`), and gives the fifth evolution skill of a
skill card as an item skill (`ISkillList.AddItemSkillAsync`), taken away with the card. It runs on every map join and
after every successful action. The extra defense adds to the defense, the pentagram defense and attack to the elemental
defense and damage (version 3 of the update). Options without an OpenMU attribute (nuke, bolt and wide skill options, the
fifth wings, mastery, bleeding, poison, triple damage, skill slots, AG recovery, shock defense, splash, and the static
skill card options) are data without effect.

## Actions (`AbilityCoreService.RequestAsync`)

- **Insert**: a card of the inventory into an open, empty slot it fits.
- **Remove**: a card back into the inventory (a given slot or the first free one).
- **Level up**: a card of the inventory or of the core, up to `MaximumCardLevel` (5), for the price of its kind and level
  (`AbilityLevelUP.xml`: other cards of the same item, the lowest levels first, zen and Ability Upgrade Stones 20/239).
- **Reduce**: a card of the inventory into stones (`AbilityReduction.xml`: zen; skill 10, option 1, elite 5 stones).
  Extra damage cards have no level up and no reduction.

## Card crafting (`AbilityCardCrafting`)

At the chaos goblin, while the core is on: Ability Crystals (bound or not) and jewels of bless into a skill card of the
class, an option card of the chosen type in a random color, an elite card of the chosen color, or five Ability Upgrade
Stones (crafting numbers 180-189, prices and rates in docs/babylon.md). A crafted card is level 0 with one card option,
drawn by the option weights. Prices and results are ordinary crafting settings of the chaos goblin.

## View

`IAbilityCoreViewPlugIn`; the Babylon client gets `EE 43`-`45` (docs/babylon.md). A Season 6 client has no view of it.
