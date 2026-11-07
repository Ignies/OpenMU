// <copyright file="EnhanceTreeDebuffKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// The kinds of the debuffs of the skill enhancement tree, the field types of S21Emu
/// <c>EnhanceSkillTreeMagicField.xml</c>.
/// </summary>
public enum EnhanceTreeDebuffKind : byte
{
    /// <summary>
    /// Poisoning, upgraded to Toxic: damage over time.
    /// </summary>
    Poison = 0,

    /// <summary>
    /// Chilling, upgraded to Frostbite: less defense, then less attack speed.
    /// </summary>
    Chill = 1,

    /// <summary>
    /// Bleeding, upgraded to Bleeding Severe: one more damage on each skill hit taken.
    /// </summary>
    Bleed = 2,
}
