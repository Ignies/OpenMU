// <copyright file="Season21GearStats.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// The values of a weapon, shield, armor or wing of Season 21.
/// </summary>
/// <param name="Template">The Season 6 item of the same kind, whose slot, options and level tables it takes.</param>
/// <param name="Skill">The number of the skill, or 0.</param>
/// <param name="Width">The width in the inventory.</param>
/// <param name="Height">The height in the inventory.</param>
/// <param name="DamageMinimum">The minimum damage.</param>
/// <param name="DamageMaximum">The maximum damage.</param>
/// <param name="Speed">The attack speed.</param>
/// <param name="MagicPower">The magic power; the rise is half of it.</param>
/// <param name="Defense">The defense.</param>
/// <param name="DefenseRate">The defense rate.</param>
/// <param name="Requirements">The required level, strength, agility, energy, vitality and command.</param>
/// <param name="Classes">The class rank which each of the wizard, knight, elf, magic gladiator, dark lord, summoner and rage fighter needs (0 none, 1 or 2 from the first class, 3 the master class).</param>
/// <param name="Sockets">The number of sockets.</param>
/// <param name="TakesOptions">Whether it takes the options of its template.</param>
/// <param name="Options">Which of luck, the option and the excellent options of its template it takes.</param>
internal sealed record Season21GearStats(
    (byte Group, short Number) Template,
    int Skill,
    byte Width,
    byte Height,
    int DamageMinimum,
    int DamageMaximum,
    int Speed,
    int MagicPower,
    int Defense,
    int DefenseRate,
    int[] Requirements,
    int[] Classes,
    int Sockets,
    bool TakesOptions,
    Season21OptionKinds Options = Season21OptionKinds.All);
