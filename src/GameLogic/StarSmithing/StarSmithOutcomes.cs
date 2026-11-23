// <copyright file="StarSmithOutcomes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A seal's outcome (Readme.md "PR 6 contract", "Outcomes"): one outcome by weight among those the forge's rarity
/// reaches (none in reach: the lowest tier), each boost rolled in its range, multiplied by the seal's magnitude factor,
/// capped per item, and only the tier's boost slots kept. Its own dice from the attempt's seed, so a replay agrees.
/// </summary>
public static class StarSmithOutcomes
{
    /// <summary>Mixed into the attempt's seed, so the outcome's dice never repeat the sky's.</summary>
    private const long SeedSalt = 0x0B00_57EA_15EE_D001;

    /// <summary>
    /// Rolls a recipe's outcome.
    /// </summary>
    /// <param name="recipe">The sealed recipe.</param>
    /// <param name="rarity">The forge's rarity.</param>
    /// <param name="magnitudeFactor">The seal's magnitude factor.</param>
    /// <param name="seed">The attempt's seed.</param>
    /// <param name="balance">The balance tables.</param>
    /// <returns>The roll, or null when the recipe has no outcome.</returns>
    public static StarSmithRoll? Roll(StarSmithRecipe recipe, StarSmithRarity rarity, double magnitudeFactor, long seed, StarSmithBalance balance)
    {
        var table = recipe.Outcomes;
        if (table.Count == 0)
        {
            return null;
        }

        var dice = new StarSmithDice(seed ^ SeedSalt);
        var reach = Enumerable.Range(0, table.Count).Where(i => table[i].Tier <= rarity && table[i].Weight > 0).ToList();
        if (reach.Count == 0)
        {
            reach = [Enumerable.Range(0, table.Count).MinBy(i => table[i].Tier)];
        }

        var pick = dice.Next(Math.Max(1, reach.Sum(i => Math.Max(0, table[i].Weight))));
        var index = reach[^1];
        foreach (var i in reach)
        {
            pick -= Math.Max(0, table[i].Weight);
            if (pick < 0)
            {
                index = i;
                break;
            }
        }

        var boosts = new List<StarSmithBoost>();
        foreach (var range in table[index].Boosts.Take(balance.BoostSlots(rarity)))
        {
            var low = Math.Min(range.Minimum, range.Maximum);
            var rolled = low + (dice.NextDouble() * (Math.Max(range.Minimum, range.Maximum) - low));
            var cap = balance.PerItem(range.Stat, pvp: false);
            var value = Math.Min(rolled * Math.Max(0, magnitudeFactor), cap);

            // A flat stat is whole in play, so the stored value, the tooltip and the attribute agree.
            if (IsFlat(range.Stat))
            {
                value = Math.Min(Math.Round(value), Math.Floor(cap));
            }

            boosts.Add(new StarSmithBoost(range.Stat, (int)Math.Round(value * 100)));
        }

        return new StarSmithRoll(index, boosts);
    }

    /// <summary>
    /// Whether a stat is a flat, whole number in play (max HP, max mana, attack speed) rather than a percentage.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <returns>True for a flat stat.</returns>
    public static bool IsFlat(StarSmithBoostStat stat)
        => stat is StarSmithBoostStat.MaxHealth or StarSmithBoostStat.MaxMana or StarSmithBoostStat.AttackSpeed;
}
