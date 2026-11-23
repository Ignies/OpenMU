// <copyright file="StarSmithOutcome.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// One row of a recipe's weighted outcome table: its weight, the tier it needs, and its boosts.
/// </summary>
public class StarSmithOutcome
{
    /// <summary>Gets or sets the weight among the outcomes in reach.</summary>
    public int Weight { get; set; } = 1;

    /// <summary>Gets or sets the tier: the outcome is in reach of a forge of this rarity or above.</summary>
    public StarSmithRarity Tier { get; set; }

    /// <summary>Gets or sets the boosts, at most 3; a tier fills only its boost slots.</summary>
    public List<StarSmithBoostRange> Boosts { get; set; } = [];
}
