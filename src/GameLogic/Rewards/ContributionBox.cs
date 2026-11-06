// <copyright file="ContributionBox.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The box (Mastery Box) which a player gets at the end of an event for a range of contribution points.
/// </summary>
public class ContributionBox : RewardItem
{
    /// <summary>
    /// Gets or sets the least points for the box.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ContributionBox_MinimumPoints_Name))]
    public int MinimumPoints { get; set; }

    /// <summary>
    /// Gets or sets the most points for the box; 0 for no upper bound.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ContributionBox_MaximumPoints_Name))]
    public int MaximumPoints { get; set; }
}
