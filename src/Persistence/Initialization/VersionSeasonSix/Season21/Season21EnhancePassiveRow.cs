// <copyright file="Season21EnhancePassiveRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A passive of S21Emu <c>Skill/EnhanceSkillTree/EnhanceSkillTreePassive.xml</c>.
/// </summary>
internal sealed class Season21EnhancePassiveRow
{
    /// <summary>
    /// Gets the base class.
    /// </summary>
    public byte Cls { get; init; }

    /// <summary>
    /// Gets the group.
    /// </summary>
    public byte Group { get; init; }

    /// <summary>
    /// Gets the index.
    /// </summary>
    public byte Index { get; init; }

    /// <summary>
    /// Gets the skill number.
    /// </summary>
    public short Skill { get; init; }

    /// <summary>
    /// Gets the stages: point condition, points per level, maximum level, formula id.
    /// </summary>
    public short[][] Stages { get; init; } = [];
}
