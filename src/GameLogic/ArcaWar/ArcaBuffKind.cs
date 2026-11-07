// <copyright file="ArcaBuffKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// What the buff of an obelisk raises.
/// </summary>
public enum ArcaBuffKind
{
    /// <summary>
    /// The skill damage, by the value.
    /// </summary>
    SkillDamage,

    /// <summary>
    /// The defense, by the value.
    /// </summary>
    Defense,

    /// <summary>
    /// The chance of a critical damage, by the value in percent.
    /// </summary>
    CriticalDamageChance,

    /// <summary>
    /// The damage, by the value.
    /// </summary>
    Damage,

    /// <summary>
    /// The damage reduction, by the value in percent.
    /// </summary>
    DamageReduction,
}
