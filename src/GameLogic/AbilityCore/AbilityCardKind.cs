// <copyright file="AbilityCardKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// The kind of an ability card; the numbers of the skill, option and elite cards are the card types of S21Emu
/// <c>AbilityLevelUP.xml</c> and <c>AbilityReduction.xml</c>.
/// </summary>
public enum AbilityCardKind : byte
{
    /// <summary>
    /// No card.
    /// </summary>
    None = 0,

    /// <summary>
    /// A skill card: it gives a fifth evolution skill while it is in a skill slot.
    /// </summary>
    Skill = 1,

    /// <summary>
    /// An option card of a color and a type.
    /// </summary>
    Option = 2,

    /// <summary>
    /// An elite card of a color.
    /// </summary>
    Elite = 3,

    /// <summary>
    /// An extra damage card (nuke, bolt, wide) of the extra slot.
    /// </summary>
    Extra = 4,
}
