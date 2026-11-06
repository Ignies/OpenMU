// <copyright file="ZenSinksConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ZenSinks;

/// <summary>
/// The configuration of the <see cref="ZenSinksFeaturePlugIn"/>. The defaults are the numbers of Season 21.
/// </summary>
public class ZenSinksConfiguration
{
    /// <summary>
    /// Gets or sets the part in percent of the price of an item sold for zen in a personal store, which the seller doesn't get.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ZenSinksConfiguration_PersonalStoreTaxPercent_Name))]
    [Range(0, 100)]
    public int PersonalStoreTaxPercent { get; set; } = 1;

    /// <summary>
    /// Gets or sets the part in percent of the money a killed monster drops, compared to the amount without this plugin.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ZenSinksConfiguration_MoneyDropPercent_Name))]
    [Range(0, 1000)]
    public int MoneyDropPercent { get; set; } = 50;
}
