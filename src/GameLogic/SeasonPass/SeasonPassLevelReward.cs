// <copyright file="SeasonPassLevelReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// The rewards of a level of a pass (<c>RewardList</c>).
/// </summary>
public class SeasonPassLevelReward
{
    /// <summary>
    /// Gets or sets the pass.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassLevelReward_Pass_Name))]
    public SeasonPassKind Pass { get; set; }

    /// <summary>
    /// Gets or sets the level, from 1.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassLevelReward_Level_Name))]
    public byte Level { get; set; }

    /// <summary>
    /// Gets or sets the ids of the rewards.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassLevelReward_RewardIds_Name))]
    public List<int> RewardIds { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => $"{this.Pass} {this.Level}";
}
