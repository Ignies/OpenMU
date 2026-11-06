// <copyright file="LevelRewardsConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The configuration of the <see cref="LevelRewardsPlugIn"/>.
/// </summary>
public class LevelRewardsConfiguration
{
    /// <summary>
    /// Gets or sets the rewards.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelRewardsConfiguration_Rewards_Name))]
    public List<LevelReward> Rewards { get; set; } = [];
}
