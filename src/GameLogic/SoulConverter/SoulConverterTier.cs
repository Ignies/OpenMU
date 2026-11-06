// <copyright file="SoulConverterTier.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// A tier of the monster soul converter (S21Emu <c>MonsterSoulConversion.txt</c>): its item, level, missions and rewards.
/// </summary>
public class SoulConverterTier
{
    /// <summary>
    /// Gets or sets the tier, 1 to 5.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterTier_Tier_Name))]
    public byte Tier { get; set; }

    /// <summary>
    /// Gets or sets the group of the converter item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterTier_ItemGroup_Name))]
    public byte ItemGroup { get; set; }

    /// <summary>
    /// Gets or sets the number of the converter item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterTier_ItemNumber_Name))]
    public short ItemNumber { get; set; }

    /// <summary>
    /// Gets or sets the total level (level and master level) the tier needs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterTier_MinimumTotalLevel_Name))]
    public int MinimumTotalLevel { get; set; }

    /// <summary>
    /// Gets or sets the missions a converter draws from <see cref="Missions"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterTier_MissionCount_Name))]
    public int MissionCount { get; set; }

    /// <summary>
    /// Gets or sets the missions of the tier.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterTier_Missions_Name))]
    public List<SoulMission> Missions { get; set; } = [];

    /// <summary>
    /// Gets or sets the rewards of the tier, one per kind.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterTier_Rewards_Name))]
    public List<SoulReward> Rewards { get; set; } = [];

    /// <inheritdoc />
    public override string ToString() => $"Tier {this.Tier}";
}
