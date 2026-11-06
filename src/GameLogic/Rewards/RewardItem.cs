// <copyright file="RewardItem.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// An item which a reward gives.
/// </summary>
public class RewardItem
{
    /// <summary>
    /// Gets or sets the group of the item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Group_Name))]
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the number of the item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Number_Name))]
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the level of the item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Level_Name))]
    public byte Level { get; set; }

    /// <summary>
    /// Gets or sets the count (durability) of the item; 0 for the durability of its definition.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Count_Name))]
    public byte Count { get; set; }
}
