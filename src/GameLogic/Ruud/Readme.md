# Ruud

The second currency of Season 21 (S10 Part 2 on), kept per character (`Character.Ruud`, migration `AddSeason21Economy`).

- `RuudFeaturePlugIn` (off by default, the world's switch; `RuudConfiguration`) opens the Ruud shop when a character
  talks to its NPC (Priest James, 725, in Elbeland and Loren Market) and shows the Ruud on every map entry.
- The shop sells the items of its rows (`RuudShopEntry`) which have no class or the base class of the character
  (`Classes/ClassCodes`); the index of a row in the configuration is how the client refers to it.
  `PlayerActions/Items/BuyRuudShopItemAction` checks the NPC, the class, the item definition (active), the Ruud and
  the inventory, and pays; the list leaves out rows whose item the configuration lacks or has inactive. Luck and the additional option come by their chance; the option level is random up to
  `OptionLevel` (the third number of the Season 21 row, taken as that).
- Sources: the killers of the monsters of `MonsterRewards` (64 bosses and event monsters of the Season 21 item bags),
  the Ruud boxes (`PlayerActions/ItemConsumeActions/RuudBox*ConsumeHandlerPlugIn`, 10 boxes from 10 to 10,000; the amount
  is configured in `BoxRewards`), the game master command `/setruud <character> <amount>`.
- `RuudExtensions` adds (up to `MaximumRuud`), takes and sets Ruud. A client without the Ruud view (Season 6) gets a
  message for gained Ruud and can't open the shop.
- With Ruud on, the field bosses leave Priest James to the Ruud shop (`IsShopNpc`), which sells their entry item, the
  Spirit Nixie's Shard (14/390, five for 2,500 Ruud); with Ruud off he hands it out for zen as before.
- The data comes with the update "Add the gremory case, Ruud and the Ruud shop of Season 21"
  (`Persistence/Initialization/VersionSeasonSix/Season21`).
- Packets: `C1 EE 70` to `C1 EE 73` (`docs/babylon.md`, "Economy").

The mastery chain: the shop sells the sealed Bloodangel pieces and weapons per class and the Spirits, Souls and Anvils
of every upgrade step (Darkangel to Imperial). The upgrade mixes themselves are crafting of the mastery track.
Not built: Ruud as a personal store currency, the Monster Soul Converter quests (the converters are sold).
