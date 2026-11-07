// <copyright file="OwnMapRespawnConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// The configuration of the <see cref="OwnMapRespawnPlugIn"/>.
/// </summary>
public class OwnMapRespawnConfiguration
{
    /// <summary>
    /// Gets or sets the numbers of the maps on which a character respawns at the map's own spawn gate instead of its
    /// safezone map. The default is Kanturu Underground (137), whose Webzen guide (405) names the safe zone in the map; no
    /// source gives the rule for the other later maps.
    /// </summary>
    [Display(ResourceType = typeof(Properties.PlugInResources), Name = nameof(Properties.PlugInResources.OwnMapRespawnConfiguration_Maps_Name))]
    public List<short> Maps { get; set; } = [137];
}
