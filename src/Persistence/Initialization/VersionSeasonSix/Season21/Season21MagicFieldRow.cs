// <copyright file="Season21MagicFieldRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A field of S21Emu <c>Skill/EnhanceSkillTree/EnhanceSkillTreeMagicField.xml</c>.
/// </summary>
internal sealed class Season21MagicFieldRow
{
    /// <summary>
    /// Gets the type.
    /// </summary>
    public byte Type { get; init; }

    /// <summary>
    /// Gets the skill.
    /// </summary>
    public short Skill { get; init; }

    /// <summary>
    /// Gets the seconds the field stays.
    /// </summary>
    public short Seconds { get; init; }

    /// <summary>
    /// Gets the cooldown in seconds.
    /// </summary>
    public short Cooldown { get; init; }
}
