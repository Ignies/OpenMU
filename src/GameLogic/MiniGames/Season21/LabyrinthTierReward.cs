// <copyright file="LabyrinthTierReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The reward of a tier of the labyrinth league on the reward day.
/// </summary>
public class LabyrinthTierReward
{
    /// <summary>
    /// Gets or sets the tier.
    /// </summary>
    public LabyrinthLeagueTier Tier { get; set; }

    /// <summary>
    /// Gets or sets the item.
    /// </summary>
    public RewardItem Item { get; set; } = new();
}
