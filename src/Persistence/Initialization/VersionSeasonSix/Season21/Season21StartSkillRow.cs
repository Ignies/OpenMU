// <copyright file="Season21StartSkillRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// A row of S21Emu <c>Character/CharacterSkillInit.xml</c>: a skill of a new character.
/// </summary>
internal sealed class Season21StartSkillRow
{
    /// <summary>
    /// Gets the base class.
    /// </summary>
    public byte Cls { get; init; }

    /// <summary>
    /// Gets the skill number.
    /// </summary>
    public short Skill { get; init; }
}
