// <copyright file="HelperPlusConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HelperPlus;

/// <summary>
/// Configuration of the <see cref="HelperPlusPlugIn"/>.
/// </summary>
public class HelperPlusConfiguration
{
    /// <summary>
    /// Gets or sets the total level (level and master level) the plus mode needs (guides: characters over level 400).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_PlusMinimumTotalLevel_Name))]
    public int PlusMinimumTotalLevel { get; set; } = 400;

    /// <summary>
    /// Gets or sets a value indicating whether only a character without a party or the party leader may use the plus mode
    /// (guides).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_PartyLeaderOnly_Name))]
    public bool PartyLeaderOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets the largest hunting range of the normal helper, in tiles (design: the classic helper window).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_HuntRange_Name))]
    [Range(1, 30)]
    public byte HuntRange { get; set; } = 8;

    /// <summary>
    /// Gets or sets the largest pick-up range of the normal helper, in tiles (design: the classic helper window).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_PickupRange_Name))]
    [Range(1, 30)]
    public byte PickupRange { get; set; } = 8;

    /// <summary>
    /// Gets or sets the largest hunting range of the plus mode, in tiles (design, no source).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_PlusHuntRange_Name))]
    [Range(1, 30)]
    public byte PlusHuntRange { get; set; } = 8;

    /// <summary>
    /// Gets or sets the largest pick-up range of the plus mode, in tiles (design, no source).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_PlusPickupRange_Name))]
    [Range(1, 30)]
    public byte PlusPickupRange { get; set; } = 8;

    /// <summary>
    /// Gets or sets the zen cost of the helper in the plus mode, in percent of the normal cost (no source: the guides name
    /// no other cost, so 100).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_PlusCostPercent_Name))]
    [Range(0, 1000)]
    public short PlusCostPercent { get; set; } = 100;

    /// <summary>
    /// Gets or sets the hunting grounds.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_Grounds_Name))]
    public List<HelperPlusGround> Grounds { get; set; } = [];

    /// <summary>
    /// Gets or sets the hunting zones.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HelperPlusConfiguration_Zones_Name))]
    public List<HelperPlusZone> Zones { get; set; } = [];
}
