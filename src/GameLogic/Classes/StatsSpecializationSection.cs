// <copyright file="StatsSpecializationSection.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// A section of a <see cref="StatsSpecialization"/>: the percent for a stat up to <see cref="UpToStat"/>.
/// </summary>
public class StatsSpecializationSection
{
    /// <summary>
    /// Gets or sets the highest stat value of the section (S21Emu <c>StatSection</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.StatsSpecializationSection_UpToStat_Name))]
    public int UpToStat { get; set; }

    /// <summary>
    /// Gets or sets the percent by which the option rises in the section (S21Emu <c>StatValue</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.StatsSpecializationSection_Percent_Name))]
    public float Percent { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"<= {this.UpToStat}: {this.Percent} %";
}
