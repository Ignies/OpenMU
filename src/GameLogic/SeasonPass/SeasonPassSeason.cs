// <copyright file="SeasonPassSeason.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// A season of the schedule (S21Emu <c>BattlePass.xml</c>, <c>Schedule</c>).
/// </summary>
public class SeasonPassSeason
{
    /// <summary>
    /// Gets or sets the number of the season.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassSeason_Number_Name))]
    public ushort Number { get; set; }

    /// <summary>
    /// Gets or sets the start (UTC).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassSeason_Start_Name))]
    public DateTime Start { get; set; }

    /// <summary>
    /// Gets or sets the end (UTC).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassSeason_End_Name))]
    public DateTime End { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{this.Number}: {this.Start:d} - {this.End:d}";
}
