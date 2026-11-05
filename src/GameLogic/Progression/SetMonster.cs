// <copyright file="SetMonster.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// A monster of a Season 21 map set, as the source data has it.
/// </summary>
/// <param name="Number">The monster number.</param>
/// <param name="Designation">The designation (name).</param>
/// <param name="Source">The source numbers.</param>
/// <param name="Spawned">If set to <c>true</c>, the set's spawn rows place this monster.</param>
public readonly record struct SetMonster(short Number, string Designation, MonsterStatRow Source, bool Spawned);
