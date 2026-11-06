// <copyright file="Season21StatFactors.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.CharacterClasses;

/// <summary>
/// Factors of the total stats which add up to one value.
/// </summary>
/// <param name="Strength">The factor of the total strength.</param>
/// <param name="Agility">The factor of the total agility (dexterity).</param>
/// <param name="Vitality">The factor of the total vitality.</param>
/// <param name="Energy">The factor of the total energy.</param>
/// <param name="Level">The factor of the total level (level and master level).</param>
/// <param name="Constant">The constant part.</param>
public readonly record struct Season21StatFactors(
    float Strength = 0,
    float Agility = 0,
    float Vitality = 0,
    float Energy = 0,
    float Level = 0,
    float Constant = 0);
