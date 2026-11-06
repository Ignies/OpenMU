// <copyright file="EliteDebuffKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// The debuffs an elite puts on a character it hits (S21Emu <c>ELITE_MONSTER_DEBUFF_TYPE_*</c>).
/// </summary>
public enum EliteDebuffKind
{
    /// <summary>
    /// Bleeding: each second a share of the hit's damage.
    /// </summary>
    Bleeding = 0,

    /// <summary>
    /// Poisoning: each second a share of the maximum health.
    /// </summary>
    Poisoning = 1,

    /// <summary>
    /// The damage dealt is decreased.
    /// </summary>
    DecreaseAttack = 2,

    /// <summary>
    /// The attack speed is decreased.
    /// </summary>
    DecreaseAttackSpeed = 3,

    /// <summary>
    /// The defense is decreased.
    /// </summary>
    DecreaseDefense = 4,
}
