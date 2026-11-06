// <copyright file="MiningSpotDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Mining;

/// <summary>
/// A kind of mining spot: the NPC which stands for it (605 small, 606 medium, 607 large) and its stages.
/// </summary>
public class MiningSpotDefinition
{
    /// <summary>
    /// Gets or sets the number of the NPC of the spot.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningSpotDefinition_NpcNumber_Name))]
    public short NpcNumber { get; set; }

    /// <summary>
    /// Gets or sets the stages, from the first; after the last one the reward is collected by itself.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningSpotDefinition_Stages_Name))]
    public List<MiningStage> Stages { get; set; } = [];
}
