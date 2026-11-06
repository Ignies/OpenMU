// <copyright file="RuudMonsterReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Ruud;

/// <summary>
/// The Ruud which the killer of a monster gets, a random amount between the minimum and the maximum.
/// </summary>
public class RuudMonsterReward
{
    /// <summary>
    /// Gets or sets the number of the monster.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudMonsterReward_MonsterNumber_Name))]
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets the minimum amount.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudMonsterReward_Minimum_Name))]
    public int Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum amount.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudMonsterReward_Maximum_Name))]
    public int Maximum { get; set; }
}
