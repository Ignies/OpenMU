// <copyright file="LuckyTicketRolls.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

using MUnique.OpenMU.DataModel.Configuration.Items;

/// <summary>
/// The rolls of the lucky item ticket exchange: S21Emu <c>Item/Lucky Item - Settings/LuckySystem.xml</c> names the rows
/// "(Lucky)" (index 10) of <c>Item/ItemOptionRate.txt</c> for each item (Option0-5 = 10). Each list holds the weights of
/// the values 0, 1, 2, ...: the value is its index.
/// </summary>
/// <remarks>
/// The skill, luck, excellent and socket rows give none (10000 for 0), so they need no list. The set option row (70 % level 1, 30 % level 2) stays
/// data: the lucky sets apply without an ancient bonus, and no source names its stat. The level isn't capped by the
/// definition's maximum level (0 for the lucky armor, which keeps the jewels off it): the exchange gives it.
/// </remarks>
public class LuckyTicketRolls
{
    /// <summary>
    /// Gets or sets the weights of the item levels +0 to +15.
    /// </summary>
    public List<int> LevelWeights { get; set; } = [1000, 2000, 1700, 1400, 1200, 900, 700, 500, 300, 100, 70, 50, 35, 20, 15, 10];

    /// <summary>
    /// Gets or sets the weights of the option (Jewel of Life) levels 0 to 3.
    /// </summary>
    public List<int> OptionWeights { get; set; } = [4000, 3000, 2000, 1000];

    /// <summary>
    /// Rolls the level and the option of an exchanged lucky item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="createLink">Creates an option link of the item's persistence context.</param>
    /// <param name="randomizer">The randomizer.</param>
    public void Apply(Item item, Func<ItemOptionLink> createLink, IRandomizer randomizer)
    {
        item.Level = (byte)Roll(this.LevelWeights, randomizer);
        if (Roll(this.OptionWeights, randomizer) is var level and > 0 && FindOption(item.Definition!, ItemOptionTypes.Option) is { } option)
        {
            var link = createLink();
            link.ItemOption = option;
            link.Level = level;
            item.ItemOptions.Add(link);
        }
    }

    private static int Roll(List<int> weights, IRandomizer randomizer)
        => weights.Sum() <= 0 ? 0 : Enumerable.Range(0, weights.Count).SelectWeightedRandom(weights, randomizer);

    private static IncreasableItemOption? FindOption(ItemDefinition definition, ItemOptionType type)
        => definition.PossibleItemOptions.SelectMany(o => o.PossibleOptions).FirstOrDefault(o => o.OptionType == type);
}
