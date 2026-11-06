// <copyright file="EnhanceTreeMagicField.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// A debuff field of the skill enhancement tree (poison, chilling, bleeding), as S21Emu
/// <c>Skill/EnhanceSkillTree/EnhanceSkillTreeMagicField.xml</c> defines it. Data only: no skill places it yet.
/// </summary>
public class EnhanceTreeMagicField
{
    /// <summary>
    /// Gets or sets the field type (0 poison, 1 chilling, 2 bleeding).
    /// </summary>
    public byte Type { get; set; }

    /// <summary>
    /// Gets or sets the number of the skill which places the field.
    /// </summary>
    public short Skill { get; set; }

    /// <summary>
    /// Gets or sets the seconds the field stays.
    /// </summary>
    public short Seconds { get; set; }

    /// <summary>
    /// Gets or sets the cooldown in seconds.
    /// </summary>
    public short CooldownSeconds { get; set; }
}
