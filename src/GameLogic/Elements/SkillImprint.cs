// <copyright file="SkillImprint.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// The element imprint bonus of a skill: the percentage of the elemental damage of a hit with this skill.
/// </summary>
public class SkillImprint
{
    /// <summary>
    /// Gets or sets the skill number.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SkillImprint_SkillNumber_Name))]
    public short SkillNumber { get; set; }

    /// <summary>
    /// Gets or sets the percentage, e.g. 150 for 150 %.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SkillImprint_Percent_Name))]
    [Range(0, 1000)]
    public int Percent { get; set; } = 100;

    /// <inheritdoc />
    public override string ToString() => $"Skill {this.SkillNumber}: {this.Percent} %";
}
