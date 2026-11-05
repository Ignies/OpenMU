// <copyright file="Season21Monster.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// A monster of a <see cref="Season21MapSet"/>, with the values of the Season 21 server data.
/// </summary>
/// <param name="Number">The monster number.</param>
/// <param name="Name">The name of the monster.</param>
/// <param name="Level">The level, which is used for the experience and the level checks.</param>
/// <param name="Health">The maximum health.</param>
/// <param name="MinimumDamage">The minimum physical damage.</param>
/// <param name="MaximumDamage">The maximum physical damage.</param>
/// <param name="Defense">The defense.</param>
/// <param name="AttackRate">The attack rate.</param>
/// <param name="DefenseRate">The defense rate.</param>
/// <param name="MoveRange">The move range.</param>
/// <param name="AttackRange">The attack range.</param>
/// <param name="ViewRange">The view range.</param>
/// <param name="MoveDelay">The move delay in milliseconds.</param>
/// <param name="AttackDelay">The attack delay in milliseconds.</param>
/// <param name="RespawnDelay">The respawn delay in seconds.</param>
/// <param name="Attribute">The attribute.</param>
/// <param name="Resistances">The ice, poison, lightning, fire, earth, wind and water resistances, each of 255.</param>
internal sealed record Season21Monster(
    short Number,
    LocalizedString Name,
    int Level,
    int Health,
    int MinimumDamage,
    int MaximumDamage,
    int Defense,
    int AttackRate,
    int DefenseRate,
    byte MoveRange,
    byte AttackRange,
    short ViewRange,
    int MoveDelay,
    int AttackDelay,
    int RespawnDelay,
    byte Attribute,
    byte[] Resistances);
