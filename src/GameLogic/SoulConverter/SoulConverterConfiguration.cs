// <copyright file="SoulConverterConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// Configuration of the <see cref="SoulConverterPlugIn"/>.
/// </summary>
public class SoulConverterConfiguration
{
    /// <summary>
    /// Gets or sets the converters a character may activate a week (S21Emu <c>MonsterSoulConversion.txt</c>: 3).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterConfiguration_ActivationsPerWeek_Name))]
    [Range(1, 100)]
    public int ActivationsPerWeek { get; set; } = 3;

    /// <summary>
    /// Gets or sets the day (00:00 UTC) on which the week starts again: the converters of the week expire and the
    /// activations count again (Webzen's guide: Thursday).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterConfiguration_WeekStartsOn_Name))]
    public DayOfWeek WeekStartsOn { get; set; } = DayOfWeek.Thursday;

    /// <summary>
    /// Gets or sets a value indicating whether the members of a party on the same map share the absorbed souls of a
    /// mission they all have (Webzen's guide).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterConfiguration_ShareWithParty_Name))]
    public bool ShareWithParty { get; set; } = true;

    /// <summary>
    /// Gets or sets the tiers.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterConfiguration_Tiers_Name))]
    public List<SoulConverterTier> Tiers { get; set; } = [];

    /// <summary>
    /// Gets or sets the minimum time between two writes of the state file (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterConfiguration_SaveInterval_Name))]
    public TimeSpan SaveInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the file which keeps the converters over a restart, relative to the server directory; empty for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SoulConverterConfiguration_StateFile_Name))]
    public string StateFile { get; set; } = "state/soul_converters.json";
}
