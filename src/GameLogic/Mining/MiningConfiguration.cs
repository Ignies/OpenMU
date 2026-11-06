// <copyright file="MiningConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Mining;

/// <summary>
/// Configuration of the <see cref="MiningFeaturePlugIn"/>.
/// </summary>
public class MiningConfiguration
{
    /// <summary>
    /// Gets or sets the group of the pickaxe (Pandora Pick, 0/41, sold by Bolo in Karutan).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_PickaxeGroup_Name))]
    public byte PickaxeGroup { get; set; }

    /// <summary>
    /// Gets or sets the number of the pickaxe.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_PickaxeNumber_Name))]
    public short PickaxeNumber { get; set; } = 41;

    /// <summary>
    /// Gets or sets the group of the mined jewel (S21Emu item 7181, the Jewel of Bless 14/13).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_JewelGroup_Name))]
    public byte JewelGroup { get; set; } = 14;

    /// <summary>
    /// Gets or sets the number of the mined jewel.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_JewelNumber_Name))]
    public short JewelNumber { get; set; } = 13;

    /// <summary>
    /// Gets or sets the distance in tiles within which a character mines a spot (design: the talk range of NPCs).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_MaximumDistance_Name))]
    [Range(1, 20)]
    public byte MaximumDistance { get; set; } = 3;

    /// <summary>
    /// Gets or sets a value indicating whether a spot serves one character at a time (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_OneMinerPerSpot_Name))]
    public bool OneMinerPerSpot { get; set; } = true;

    /// <summary>
    /// Gets or sets the kinds of spots.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_Spots_Name))]
    public List<MiningSpotDefinition> Spots { get; set; } = [];

    /// <summary>
    /// Gets or sets the packed items of the jewel; the largest that fits is used first (design: the packed jewels of bless of 30, 20 and 10).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningConfiguration_Bundles_Name))]
    public List<MiningBundle> Bundles { get; set; } = [];
}
