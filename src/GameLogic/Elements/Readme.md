# Elements

The elemental layer of Season 21 (feature `pentagram_elements`): every character with a pentagram and every
monster of the Season 21 data has an element, an elemental damage and an elemental defense, and every hit carries
an elemental part beside the normal damage. The protocol side is in `docs/babylon.md`, "Elements, pentagrams and
earrings".

- `ElementsFeaturePlugIn` (off by default, the world's switch) holds the rules (`ElementsConfiguration`): the
  element wheel, the PvP multiplier, the minimum hit chance, the skill imprints and the errtel tables (removal,
  level up, rank up). The update "Add the elements, pentagrams and errtels" fills them and turns it on.
- `ElementalDamageCalculator`: the elemental part of a hit. `AttackableExtensions.CalculateDamageAsync` adds it to
  the hit (also when the normal hit misses: the elemental hit has its own roll) and records it in
  `HitInfo.ElementalDamage`; the plugin shows it to the Babylon clients involved (`IElementalDamageViewPlugIn`).
- `Pentagrams`: the rules of pentagrams and errtels (the socket of an errtel kind, the rank options, the level).
  `PlayerActions/Items/ErrtelSocketAction` puts an errtel into a pentagram or takes it out.
- Craftings (`PlayerActions/Craftings`): `ElementalUpgradeCrafting` (pentagram / errtel to its mastery item),
  `ErrtelLevelUpCrafting`, `ErrtelRankUpCrafting`.

## Elemental damage

```
damage  = random(min, max) + bonus + PvP or PvM bonus + bonus against the defender's element
damage *= (1 + increase) x wheel(attacker, defender) x imprint % x (1.3 against a player)
defense = (elemental defense + PvP or PvM bonus + bonus against the attacker's element) x (1 + increase)
result  = max(0, damage - defense), if attack rate against defense rate hits
```

Webzen gives the wheel and the 30 % in PvP (EX700 guide, https://muonline.webzen.com/en/events/EX700Guide/pop_attribute);
the order of the terms is a design, since no source gives the formula. A defender without element takes 120 % from
every element. The hit chance follows the normal hit: `1 - defenseRate / attackRate`, at least 3 %; a defender
without elemental defense rate is always hit.

The monster element 6 of Monster.xml ("random") picks one of the five by the monster's object id, so it stays while
the monster lives and changes with its next spawn. The element patterns (`ElementalPattern`) of the data are not
used: they belong to events which rotate their element.

## Pentagrams and errtels

- A pentagram's element is its `Element` option; its stats are base power-ups (elemental damage min / max, defense,
  attack and defense rate).
- An errtel has an element and up to five rank options (option type `ErrtelRank`, the sub option type is the rank,
  the link level the level 0-10). The kind of an errtel, and so its socket, comes from its option numbers, which the
  Season 21 data groups by kind in blocks of 50 (anger 0-21, blessing 50-71, integrity 100-121, divinity 150-171,
  radiance 200-224; the beginner errtels 250-253).
- Inserted, the errtel's rank options move into the pentagram as links with the socket as their index, so the
  pentagram's power-ups include them; taken out, a new errtel of the definition the options belong to gets them back.
- Not done yet: the pentagram options (PentagramOption.xml, bonuses for errtel combinations), the errtel set
  options of the mastery errtels (ErrtelMastery.xml), the radiance effects of rank 2 and 3 (absorb, bastion,
  bleeding, paralyzing, binding, punish, blinding), the elemental critical damage, the expansion scroll of the fifth
  socket, the pentagram enhancement levels.
