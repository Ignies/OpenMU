# Muuns

The Muun system of Season 21 (S8 Part 3 on): small companions in the item group 16 with an option for their owner.

- `MuunFeaturePlugIn` (off by default, the world's switch; `MuunConfiguration`, filled by the update "Add the Muuns of
  Season 21") shows the Muun inventory, the options and the Muun appearance on every map entry, checks the conditions of
  the options every minute and lets equipped Muuns lose health. Talking to Monica (682) opens the exchange.
- `MuunService`: moves between the inventory and the Muun inventory, level up, evolution, the energy converter, health,
  the exchange, riding, and the options on the owner. Only a player whose client shows the Muuns (`IMuunViewPlugIn`, the
  Babylon client) has the system; a Season 6 client never sees a Muun (they are wide items) and gets "off".
- Storage: `Character.MuunInventory` (`ItemStorage`, migration `AddSeason21Muuns`): slot 0 the main Muun (it follows the
  character and is ridden), 1 the sub Muun, 2-101 the bag (the 102 slots of the Season 21 Muun window). The bag takes the
  items of the Muun group: Muuns, evolution stones, the energy converter, wizardry stones.
- Options (`MuunEffects`): the option of the level (1 to rank + 1), or the evolved value, applies while its condition holds
  (`MuunCondition`: hours UTC, weekdays, total level, map) and the Muun has health. The additional option multiplies it
  (x5 to x20) or adds to it for its days (`Item.TimedOptionExpiresAt`, 30 days in the data), counted from the first time
  the Muun enters a Muun inventory, as the guides say ("30 days from the time you obtained the muun").
- Level up: a Muun of level 1 of the same kind raises a Muun by one level (100 %), up to its highest level.
- Evolution: at its highest level, the evolution stone 16/211 which names the Muun (option "Muun Evolution Target"), or
  for a tempest Muun the tempest evolution stone 16/510, turns it into its evolved item; the level and the additional
  option stay.
- Energy converter (16/239): a Muun gives the energy of its rank and level, an evolution stone 2 (MuunExchange.txt); the
  energy is the converter's durability, and at 120 it becomes a wizardry stone (16/240).
- Exchange: Monica takes 2 wizardry stones for the evolution stone of one of six Muuns; it lands in the Muun bag.
- Mounts: a mount (kind 4) in the main slot can be ridden, not in Chaos Castle and Illusion Temple; moving it out of the
  slot or entering such a map dismounts. Riding shows on the appearance only (no speed change).
- Health: the durability of a Muun (255). Designed: an equipped Muun loses 1 every 10 minutes (the guides say it "slowly"
  loses health, without a number); at 0 its option stops; a jewel of life dropped on it restores it.
- Packets: `EE 50` to `EE 59` (`docs/babylon.md`, "Muuns"); the W32 kind block of a Muun holds the end of its additional
  option, the one of an evolution stone its Muun.

Not built: the attack skills of the Muuns (option types 50 and 53), the repair cost option (no attribute), the hero slot
of the guides (a third slot at the highest character level), the riding speed, a Muun inventory for the Season 6 client.
