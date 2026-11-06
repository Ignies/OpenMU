// <copyright file="MiningBundle.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Mining;

/// <summary>
/// A packed item which carries several of the mined jewels, so a large reward takes few slots.
/// </summary>
public class MiningBundle
{
    /// <summary>
    /// Gets or sets the group of the packed item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningBundle_ItemGroup_Name))]
    public byte ItemGroup { get; set; }

    /// <summary>
    /// Gets or sets the number of the packed item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningBundle_ItemNumber_Name))]
    public short ItemNumber { get; set; }

    /// <summary>
    /// Gets or sets the item level of the packed item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningBundle_ItemLevel_Name))]
    [Range(0, 15)]
    public byte ItemLevel { get; set; }

    /// <summary>
    /// Gets or sets the jewels one packed item carries.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MiningBundle_JewelCount_Name))]
    [Range(1, 1000)]
    public int JewelCount { get; set; }
}
