# Zen sinks

Against the zen inflation of the later maps (money piles follow the kill experience, about 40 times a Season 6 pile on
Kubera Mine; `S21_Parity.md` PAR-023, PAR-024, PAR-117).

- `ZenSinksFeaturePlugIn` (off by default, the world's switch; `ZenSinksConfiguration`), defaults from the Season 21 server
  data (`GameServerInfo - Common.dat`):
  - `PersonalStoreTaxPercent` 1: the seller of a personal store gets the zen price less 1 % (`ZenShopCurrencyExchange`).
  - `MoneyDropPercent` 50: monsters drop half the money (`MoneyAmountDropRate`, applied in `AttackableNpcBase`).

Not built: the 3 % jewel tax of personal stores (a price in jewels moves whole stacks), the zen costs of the later
crafting (imperial, 5th wings, earrings, legendary: with their mixes).
