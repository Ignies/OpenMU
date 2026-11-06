// <copyright file="RuudShopEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Ruud;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// An item which the Ruud shop sells.
/// </summary>
public class RuudShopEntry
{
    /// <summary>
    /// Gets or sets the class which may buy it; <c>null</c> for every class.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Class_Name))]
    public BaseClassCode? Class { get; set; }

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

    /// <summary>
    /// Gets or sets the chance in percent that the item gets the luck option.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_LuckChance_Name))]
    [Range(0, 100)]
    public byte LuckChance { get; set; }

    /// <summary>
    /// Gets or sets the chance in percent that the item gets the additional option.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_OptionChance_Name))]
    [Range(0, 100)]
    public byte OptionChance { get; set; }

    /// <summary>
    /// Gets or sets the highest level of the additional option, which is chosen randomly from 1 up to it.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_OptionLevel_Name))]
    public byte OptionLevel { get; set; }

    /// <summary>
    /// Gets or sets the price in Ruud.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Price_Name))]
    public int Price { get; set; }
}
