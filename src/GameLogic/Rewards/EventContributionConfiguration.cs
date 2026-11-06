// <copyright file="EventContributionConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The configuration of the <see cref="EventContributionPlugIn"/>.
/// </summary>
public class EventContributionConfiguration
{
    /// <summary>
    /// Gets or sets the points per kind of event.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionConfiguration_Rules_Name))]
    public List<EventContributionRule> Rules { get; set; } = [];

    /// <summary>
    /// Gets or sets the boxes per range of points.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionConfiguration_Boxes_Name))]
    public List<ContributionBox> Boxes { get; set; } = [];
}
