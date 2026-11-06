// <copyright file="StatsSpecialization.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// A row of the stat specialization: one stat of a class raises one option by the percent of the section the stat is in.
/// </summary>
public class StatsSpecialization
{
    /// <summary>
    /// Gets or sets the base class whose evolutions get the specialization.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.StatsSpecialization_BaseClass_Name))]
    public BaseClassCode BaseClass { get; set; }

    /// <summary>
    /// Gets or sets the stat which drives the specialization.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.StatsSpecialization_Stat_Name))]
    public StatsSpecializationStat Stat { get; set; }

    /// <summary>
    /// Gets or sets the option which the specialization raises.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.StatsSpecialization_Option_Name))]
    public StatsSpecializationOption Option { get; set; }

    /// <summary>
    /// Gets or sets the sections, ordered by <see cref="StatsSpecializationSection.UpToStat"/>. A stat above the last
    /// section gets the percent of the last one.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.StatsSpecialization_Sections_Name))]
    public List<StatsSpecializationSection> Sections { get; set; } = [];

    /// <summary>
    /// Gets the percent of the section which the stat value is in.
    /// </summary>
    /// <param name="statValue">The value of the stat.</param>
    /// <returns>The percent, e.g. 10 for +10 %.</returns>
    public float GetPercent(float statValue)
    {
        if (this.Sections.Count == 0)
        {
            return 0;
        }

        foreach (var section in this.Sections)
        {
            if (statValue <= section.UpToStat)
            {
                return section.Percent;
            }
        }

        return this.Sections[^1].Percent;
    }

    /// <inheritdoc />
    public override string ToString() => $"{this.BaseClass}: {this.Stat} -> {this.Option}";
}
