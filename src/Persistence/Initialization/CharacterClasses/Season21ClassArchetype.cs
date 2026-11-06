// <copyright file="Season21ClassArchetype.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// The kind of a Season 21 class for what its source doesn't define: the fenrir damage, the ability recovery and the
/// wizardry bonuses of items and buffs, taken from the Season 6 class of the same kind.
/// </summary>
/// <remarks>
/// The stat formulas themselves come from the S21Emu character calculation (<see cref="Season21ClassFormulas"/>).
/// </remarks>
public enum Season21ClassArchetype
{
    /// <summary>
    /// A strength fighter, like the dark knight.
    /// </summary>
    Knight,

    /// <summary>
    /// A caster with wizardry damage, like the dark wizard.
    /// </summary>
    Wizard,

    /// <summary>
    /// An agility fighter, like the fairy elf.
    /// </summary>
    Agility,
}
