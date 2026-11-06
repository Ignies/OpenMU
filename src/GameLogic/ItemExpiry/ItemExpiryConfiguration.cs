// <copyright file="ItemExpiryConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemExpiry;

/// <summary>
/// The configuration of the <see cref="ItemExpiryPlugIn"/>.
/// </summary>
public class ItemExpiryConfiguration
{
    /// <summary>
    /// Gets or sets the interval in which the items of the players are checked.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ItemExpiryConfiguration_Interval_Name))]
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
}
