// <copyright file="StatsSpecializationOption.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// The option which a <see cref="StatsSpecialization"/> raises; the numbers are the S21Emu <c>OptionIndex</c>.
/// </summary>
public enum StatsSpecializationOption
{
    /// <summary>
    /// The minimum and maximum physical base damage.
    /// </summary>
    AttackPower = 1,

    /// <summary>
    /// The attack rate against monsters.
    /// </summary>
    AttackRate = 2,

    /// <summary>
    /// The attack rate against players.
    /// </summary>
    AttackRatePvp = 3,

    /// <summary>
    /// The base defense.
    /// </summary>
    Defense = 4,

    /// <summary>
    /// The defense rate against monsters.
    /// </summary>
    DefenseRate = 6,

    /// <summary>
    /// The defense rate against players.
    /// </summary>
    DefenseRatePvp = 7,

    /// <summary>
    /// The minimum and maximum wizardry base damage.
    /// </summary>
    WizardryDamage = 9,

    /// <summary>
    /// The minimum and maximum curse base damage.
    /// </summary>
    CurseDamage = 10,
}
