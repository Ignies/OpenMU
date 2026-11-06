// <copyright file="SeasonPassMission.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// A mission of the season pass (<c>DailyMission</c>, <c>SpecialMission</c>).
/// </summary>
public class SeasonPassMission
{
    /// <summary>
    /// Gets or sets the index of the mission (the client's mission text).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassMission_Index_Name))]
    public byte Index { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether it is a special mission, which counts for the whole season, not a day.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassMission_IsSpecial_Name))]
    public bool IsSpecial { get; set; }

    /// <summary>
    /// Gets or sets what the mission counts.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassMission_Kind_Name))]
    public SeasonPassMissionKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the pass experience of one completion.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassMission_Experience_Name))]
    public int Experience { get; set; }

    /// <summary>
    /// Gets or sets the count one completion needs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassMission_Required_Name))]
    public int Required { get; set; }

    /// <summary>
    /// Gets or sets the completions a day (a special mission: a season).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassMission_Repeat_Name))]
    public int Repeat { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{(this.IsSpecial ? "Special " : string.Empty)}{this.Index}: {this.Kind}";
}
