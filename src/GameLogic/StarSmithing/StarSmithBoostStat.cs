// <copyright file="StarSmithBoostStat.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The stats a smithed boost raises (Readme.md "PR 6 contract", "Boost stats"); the byte is its wire value.
/// </summary>
public enum StarSmithBoostStat : byte
{
    /// <summary>Damage, in percent.</summary>
    Damage,

    /// <summary>Defense, in percent.</summary>
    Defense,

    /// <summary>Maximum health, flat.</summary>
    MaxHealth,

    /// <summary>Maximum mana, flat.</summary>
    MaxMana,

    /// <summary>Attack speed, flat.</summary>
    AttackSpeed,

    /// <summary>Critical damage rate, in percent.</summary>
    CriticalRate,

    /// <summary>Excellent damage rate, in percent.</summary>
    ExcellentRate,

    /// <summary>Damage absorbed, in percent.</summary>
    DamageAbsorbed,
}
