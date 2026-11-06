// <copyright file="Season21EnhanceNodeRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A node of S21Emu <c>Skill/EnhanceSkillTree/EnhanceSkillTree.xml</c>.
/// </summary>
internal sealed class Season21EnhanceNodeRow
{
    /// <summary>
    /// Gets the base class.
    /// </summary>
    public byte Cls { get; init; }

    /// <summary>
    /// Gets the sheet.
    /// </summary>
    public byte Group { get; init; }

    /// <summary>
    /// Gets the slot.
    /// </summary>
    public byte Index { get; init; }

    /// <summary>
    /// Gets the skill number.
    /// </summary>
    public short Skill { get; init; }

    /// <summary>
    /// Gets the column color.
    /// </summary>
    public byte Color { get; init; }

    /// <summary>
    /// Gets the points of the first step.
    /// </summary>
    public byte Min { get; init; }

    /// <summary>
    /// Gets the maximum points.
    /// </summary>
    public byte Max { get; init; }

    /// <summary>
    /// Gets the formula id.
    /// </summary>
    public short Formula { get; init; }

    /// <summary>
    /// Gets the improved skill.
    /// </summary>
    public short Related { get; init; }

    /// <summary>
    /// Gets the required skills with their points (<c>-1</c>: learned).
    /// </summary>
    public short[][] Requires { get; init; } = [];
}
