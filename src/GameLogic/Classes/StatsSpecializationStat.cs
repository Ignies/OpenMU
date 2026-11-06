// <copyright file="StatsSpecializationStat.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The stat which drives a <see cref="StatsSpecialization"/>; the numbers are the S21Emu <c>StatType</c>.
/// </summary>
public enum StatsSpecializationStat
{
    /// <summary>
    /// The total strength.
    /// </summary>
    Strength = 1,

    /// <summary>
    /// The total agility (dexterity).
    /// </summary>
    Agility = 2,

    /// <summary>
    /// The total vitality.
    /// </summary>
    Vitality = 3,

    /// <summary>
    /// The total energy.
    /// </summary>
    Energy = 4,

    /// <summary>
    /// The total leadership (command).
    /// </summary>
    Leadership = 5,
}
