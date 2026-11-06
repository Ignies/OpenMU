// <copyright file="RuudBoxReward.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Ruud;

/// <summary>
/// The Ruud which a player gets for opening (using) a Ruud box item.
/// </summary>
public class RuudBoxReward
{
    /// <summary>
    /// Gets or sets the group of the box item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Group_Name))]
    public byte Group { get; set; }

    /// <summary>
    /// Gets or sets the number of the box item.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudShopEntry_Number_Name))]
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the amount of Ruud.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.RuudBoxReward_Amount_Name))]
    public int Amount { get; set; }
}
