// <copyright file="Season21ClassInfo.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// A class of Season 21 as the S21Emu data defines it (<c>Character/DefaultClassInfo.txt</c>, the level-up points of
/// <c>GameServerInfo - Common.dat</c>).
/// </summary>
/// <param name="BaseClass">The base class.</param>
/// <param name="Archetype">The formula set.</param>
/// <param name="Strength">The base strength.</param>
/// <param name="Agility">The base agility (dexterity).</param>
/// <param name="Vitality">The base vitality.</param>
/// <param name="Energy">The base energy.</param>
/// <param name="MaxLife">The maximum health at level 1.</param>
/// <param name="MaxMana">The maximum mana at level 1.</param>
/// <param name="LevelLife">The health per level.</param>
/// <param name="LevelMana">The mana per level.</param>
/// <param name="VitalityToLife">The health per vitality point.</param>
/// <param name="EnergyToMana">The mana per energy point.</param>
/// <param name="PointsPerLevel">The stat points per level.</param>
/// <param name="IntroducedIn">The version which introduced the class.</param>
/// <param name="IsUnfinishedInSource">If set to <c>true</c>, the source marks the row as not done.</param>
public sealed record Season21ClassInfo(
    BaseClassCode BaseClass,
    Season21ClassArchetype Archetype,
    int Strength,
    int Agility,
    int Vitality,
    int Energy,
    int MaxLife,
    int MaxMana,
    float LevelLife,
    float LevelMana,
    float VitalityToLife,
    float EnergyToMana,
    int PointsPerLevel,
    GameVersion IntroducedIn,
    bool IsUnfinishedInSource = false);
