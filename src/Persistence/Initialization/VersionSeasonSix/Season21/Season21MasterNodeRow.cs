// <copyright file="Season21MasterNodeRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A node of S21Emu <c>Skill/MasterSkillTree.xml</c>.
/// </summary>
internal sealed class Season21MasterNodeRow
{
    /// <summary>
    /// Gets the base class.
    /// </summary>
    public byte Cls { get; init; }

    /// <summary>
    /// Gets the slot in the client tree: three roots of 36 (4 x 9) one after another.
    /// </summary>
    public byte Index { get; init; }

    /// <summary>
    /// Gets the skill number.
    /// </summary>
    public short Skill { get; init; }

    /// <summary>
    /// Gets the root (1-3).
    /// </summary>
    public byte Group { get; init; }

    /// <summary>
    /// Gets the points of the first step.
    /// </summary>
    public byte Min { get; init; }

    /// <summary>
    /// Gets the maximum points.
    /// </summary>
    public byte Max { get; init; }

    /// <summary>
    /// Gets the formula id of the value.
    /// </summary>
    public short Formula { get; init; }

    /// <summary>
    /// Gets the skill it replaces, <c>0</c> for none.
    /// </summary>
    public short Replace { get; init; }

    /// <summary>
    /// Gets the required skills.
    /// </summary>
    public short[] Requires { get; init; } = [];

    /// <summary>
    /// Gets the value type: 0 none, 1 damage increase, 2 cooldown reduction.
    /// </summary>
    public byte ValueType { get; init; }
}
