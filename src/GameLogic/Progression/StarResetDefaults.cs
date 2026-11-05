// <copyright file="StarResetDefaults.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Resets;

/// <summary>
/// The star defaults of the reset configuration: ten stars at most, level 400, 5,000,000 zen times the star, and from the
/// sixth star one Jewel of Life more per star (15 jewels and 275,000,000 zen to ten stars).
/// </summary>
/// <remarks>
/// Only fields which are still at the reset feature's defaults are changed, so an operator's values stay.
/// </remarks>
public static class StarResetDefaults
{
    /// <summary>
    /// The default star limit.
    /// </summary>
    internal const int StarLimit = 10;

    /// <summary>
    /// The zen per star; it is multiplied with the star.
    /// </summary>
    internal const int ZenPerStar = 5_000_000;

    /// <summary>
    /// The first star which asks for the reset item.
    /// </summary>
    internal const int FirstStarWithItem = 6;

    /// <summary>
    /// The additional amount of the reset item per star.
    /// </summary>
    internal const int ItemsPerStar = 1;

    /// <summary>
    /// Applies the star defaults to the fields of a reset configuration which are still at their defaults.
    /// </summary>
    /// <param name="configuration">The reset configuration.</param>
    /// <param name="resetItem">The reset item (Jewel of Life by default), or <c>null</c> to ask for no item.</param>
    public static void ApplyTo(ResetConfiguration configuration, ItemDefinition? resetItem)
    {
        var defaults = new ResetConfiguration();
        if (configuration.ResetLimit == defaults.ResetLimit)
        {
            configuration.ResetLimit = StarLimit;
        }

        if (configuration.RequiredMoney == defaults.RequiredMoney)
        {
            configuration.RequiredMoney = ZenPerStar;
        }

        if (resetItem is null)
        {
            return;
        }

        configuration.RequiredResetItem ??= resetItem;
        if (configuration.ItemCostTiers.Count > 0)
        {
            return;
        }

        var limit = configuration.ResetLimit ?? StarLimit;
        for (var star = FirstStarWithItem; star <= limit; star++)
        {
            configuration.ItemCostTiers.Add(new ResetConfiguration.ResetItemCostTier
            {
                MinimumResetCount = star,
                RequiredItemAmount = (star - FirstStarWithItem + 1) * ItemsPerStar,
            });
        }
    }
}
