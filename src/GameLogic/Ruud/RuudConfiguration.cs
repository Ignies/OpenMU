// <copyright file="RuudConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Ruud;

/// <summary>
/// The configuration of the <see cref="RuudFeaturePlugIn"/>: the Ruud shop and the sources of Ruud.
/// </summary>
public class RuudConfiguration
{
    /// <summary>
    /// Gets or sets the number of the NPC which opens the Ruud shop (Priest James).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudConfiguration_ShopNpcNumber_Name))]
    public short ShopNpcNumber { get; set; } = 725;

    /// <summary>
    /// Gets or sets the most Ruud a character can have.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudConfiguration_MaximumRuud_Name))]
    [Range(0, int.MaxValue)]
    public int MaximumRuud { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the items of the Ruud shop. Their index in the list is how the client refers to them.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudConfiguration_ShopEntries_Name))]
    public List<RuudShopEntry> ShopEntries { get; set; } = [];

    /// <summary>
    /// Gets or sets the Ruud which the killers of monsters get.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudConfiguration_MonsterRewards_Name))]
    public List<RuudMonsterReward> MonsterRewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the Ruud of the Ruud box items.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudConfiguration_BoxRewards_Name))]
    public List<RuudBoxReward> BoxRewards { get; set; } = [];
}
