// <copyright file="Season21SkillBookRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A book (group 12 or 15) which teaches a skill; from <c>Skill/SkillRequire.txt</c> or by its name.
/// </summary>
internal sealed class Season21SkillBookRow
{
    /// <summary>
    /// Gets the item group.
    /// </summary>
    public byte Group { get; init; }

    /// <summary>
    /// Gets the item number.
    /// </summary>
    public short Number { get; init; }

    /// <summary>
    /// Gets the skill number.
    /// </summary>
    public short Skill { get; init; }

    /// <summary>
    /// Gets the required skill, <c>0</c> for none.
    /// </summary>
    public short RequiredSkill { get; init; }

    /// <summary>
    /// Gets the required master skill, <c>0</c> for none.
    /// </summary>
    public short RequiredMasterSkill { get; init; }

    /// <summary>
    /// Gets the points the master skill needs.
    /// </summary>
    public short RequiredPoints { get; init; }
}
