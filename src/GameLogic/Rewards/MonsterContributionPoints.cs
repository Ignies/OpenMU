// <copyright file="MonsterContributionPoints.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The contribution points of a special monster of an event, e.g. a gate, a statue or a boss.
/// </summary>
public class MonsterContributionPoints
{
    /// <summary>
    /// Gets or sets the number of the monster.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudMonsterReward_MonsterNumber_Name))]
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets the points.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_Points_Name))]
    public int Points { get; set; }
}
