// <copyright file="MonsterStatRow.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The numbers of a monster which the progression shapes.
/// </summary>
/// <param name="Level">The level (it decides experience and money per kill).</param>
/// <param name="Health">The maximum health.</param>
/// <param name="DamageMin">The minimum physical damage.</param>
/// <param name="DamageMax">The maximum physical damage.</param>
/// <param name="Defense">The defense.</param>
/// <param name="AttackRate">The attack rate.</param>
/// <param name="DefenseRate">The defense rate.</param>
public readonly record struct MonsterStatRow(int Level, int Health, int DamageMin, int DamageMax, int Defense, int AttackRate, int DefenseRate);
