# Zen sinks

Against the zen inflation of the later maps (money piles follow the kill experience, about 40 times a Season 6 pile on
Kubera Mine; `S21_Parity.md` PAR-023, PAR-024, PAR-117).

- `ZenSinksFeaturePlugIn` (off by default, the world's switch; `ZenSinksConfiguration`), defaults from the Season 21 server
  data (`GameServerInfo - Common.dat`):
  - `PersonalStoreTaxPercent` 1: the seller of a personal store gets the zen price less 1 % (`ZenShopCurrencyExchange`).
  - `MoneyDropPercent` 50: monsters drop half the money (`MoneyAmountDropRate`, applied in `AttackableNpcBase`).
    The data keeps the pile on the kill experience on every map (`MapManager.txt` `ZenAmountRate` 100 everywhere), so
    the halving is the whole Season 21 rule; a lower percentage is the operator's balance choice.

  - `PersonalStoreJewelTaxPercent` 3: of a price in jewels the seller gets 3 % less (`PersonalShopJewelCommisionRate`,
    `ItemShopCurrencyExchange`); the buyer pays the whole price and the tax pieces are deleted. Bundles aren't split, so
    the tax is the most pieces up to 3 % (rounded down) which some of the paid items make up exactly.

Not built: the zen costs of the later
crafting (imperial, 5th wings, earrings, legendary: with their mixes).
