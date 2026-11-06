# Gremory case

The reward storage of Season 21 (S9 Part 2 on): every character has a case, and its account has one shared by its
characters. Rewards wait there until the player claims them into the inventory, for 7 days by default.

- `GremoryCaseFeaturePlugIn` (off by default, the world's switch; `GremoryCaseConfiguration`: capacities of 50, the
  7 days, whether mini game item rewards go into it) shows both cases to the player on every map entry.
- `GremoryCaseService` adds (`TryAddAsync`), claims (`ClaimAsync`), lists and removes expired rewards. A reward only goes
  into a case when the plugin is on and the player's client shows the case (`IGremoryCaseViewPlugIn`, the Babylon client):
  a Season 6 client, a full case or an off switch give the reward the way it took before.
- The claim deadline is `Item.ExpiresAt`; the item expiry (`../ItemExpiry`) removes rewards nobody claimed. A claimed
  item doesn't expire.
- Sources wired: the item rewards of every mini game (`MiniGameRewardService`: Blood Castle, Devil Square, Chaos Castle,
  Illusion Temple, Doppelganger, ...), the game master command `/gremgif <character> <case: 1 account, 2 character>
  <group> <number> [level]` (Season 21's command). The field bosses keep their ground drops, as in Season 21.
- Storage: `Character.GremoryCase` and `Account.GremoryCase` (`ItemStorage`, migration `AddSeason21Economy`).
- Packets: `C2 EE 60` to `C1 EE 65` (`docs/babylon.md`, "Economy").

- Further sources: the level rewards and the event contribution Mastery Boxes (`../Rewards`).

Not built: the reward source text per item (the client shows the case without it).
