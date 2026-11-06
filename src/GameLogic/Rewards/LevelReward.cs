// <copyright file="LevelReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The reward of reaching a total level (level plus master level), Season 21's "item compensation level".
/// </summary>
public class LevelReward : RewardItem
{
    /// <summary>
    /// Gets or sets the total level which gives the reward.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelReward_TotalLevel_Name))]
    public int TotalLevel { get; set; }

    /// <summary>
    /// Gets or sets the base classes which get it; empty for every class.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelReward_Classes_Name))]
    public List<BaseClassCode> Classes { get; set; } = [];
}
