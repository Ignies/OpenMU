// <copyright file="Season21ClassArchetype.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// The formula set of a Season 21 class: which stat drives its damage and defense.
/// </summary>
/// <remarks>
/// The S21Emu data names the main stats of each class (<c>Character/StatsSpecialize.txt</c>), but keeps the damage,
/// defense, rate and speed formulas in its binary. Each archetype takes the coefficients of the Season 6 class of the
/// same kind, so the new classes play on the Season 6 curve; they are their own rows, so balancing them never touches
/// a Season 6 class.
/// </remarks>
public enum Season21ClassArchetype
{
    /// <summary>
    /// Strength drives the physical damage, like the dark knight.
    /// </summary>
    Knight,

    /// <summary>
    /// Energy drives the wizardry damage, like the dark wizard.
    /// </summary>
    Wizard,

    /// <summary>
    /// Strength and agility drive the physical damage, like the melee attack of the fairy elf.
    /// </summary>
    Agility,
}
