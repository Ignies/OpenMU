// <copyright file="MuunExchangeOffer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// An offer of the Muun exchange NPC (Monica): items of a kind for an evolution stone of a Muun (MuunExchange.txt,
/// sections 2 and 3). The index in the list is how the client refers to it.
/// </summary>
public class MuunExchangeOffer
{
    /// <summary>
    /// Gets or sets the group of the item which is paid.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunExchangeOffer_CostGroup_Name))]
    public byte CostGroup { get; set; }

    /// <summary>
    /// Gets or sets the number of the item which is paid.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunExchangeOffer_CostNumber_Name))]
    public short CostNumber { get; set; }

    /// <summary>
    /// Gets or sets how many of the items are paid.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunExchangeOffer_CostCount_Name))]
    [Range(1, byte.MaxValue)]
    public byte CostCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the group of the item which the player gets.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunExchangeOffer_ResultGroup_Name))]
    public byte ResultGroup { get; set; }

    /// <summary>
    /// Gets or sets the number of the item which the player gets.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunExchangeOffer_ResultNumber_Name))]
    public short ResultNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of the Muun which the evolution stone evolves; <c>null</c> for an item without target.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunExchangeOffer_TargetNumber_Name))]
    public short? TargetNumber { get; set; }
}
