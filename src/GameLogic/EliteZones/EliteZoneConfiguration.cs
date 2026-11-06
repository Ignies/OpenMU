// <copyright file="EliteZoneConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// Configuration of the <see cref="EliteZonesFeaturePlugIn"/>.
/// </summary>
public class EliteZoneConfiguration
{
    /// <summary>
    /// Gets or sets the Season 21 total levels a character needs above the entry level of the map to enter a zone.
    /// </summary>
    /// <remarks>
    /// Webzen's guides name no rule of their own, so a zone asks what its map asks (0).
    /// In the normalized and stars modes of the progression, the sum is mapped like the map's own requirement.
    /// </remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EliteZoneConfiguration_LevelAboveMap_Name))]
    [Range(0, 2000)]
    public int LevelAboveMap { get; set; }

    /// <summary>
    /// Gets or sets the zones.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EliteZoneConfiguration_Zones_Name))]
    public List<EliteZoneDefinition> Zones { get; set; } = [];

    /// <summary>
    /// Gets or sets the debuffs the elites of each element put on the characters they hit.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EliteZoneConfiguration_Debuffs_Name))]
    public List<EliteDebuff> Debuffs { get; set; } = [];

    /// <summary>
    /// Gets or sets the weekly drop limits per character.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EliteZoneConfiguration_DropLimits_Name))]
    public List<EliteDropLimit> DropLimits { get; set; } = [];

    /// <summary>
    /// Gets or sets the day on which the weekly limits start again (S21Emu <c>DayOfWeek="2"</c>, Tuesday, the maintenance day).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EliteZoneConfiguration_WeekStartsOn_Name))]
    public DayOfWeek WeekStartsOn { get; set; } = DayOfWeek.Tuesday;

    /// <summary>
    /// Gets or sets the file which keeps the weekly drop counts over a restart, relative to the server directory; empty for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EliteZoneConfiguration_StateFile_Name))]
    public string StateFile { get; set; } = "state/elite_drops.json";
}
