// <copyright file="GoldenMonster.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// A monster row of the progression design.
/// </summary>
/// <param name="Number">The monster number.</param>
/// <param name="Name">The designation.</param>
/// <param name="Set">The set id.</param>
/// <param name="Source">The Season 21 row.</param>
/// <param name="Spawned">The number of spawns of the set's spawn rows.</param>
/// <param name="Expected">The expected normalized row.</param>
public sealed record GoldenMonster(short Number, string Name, string Set, MonsterStatRow Source, int Spawned, MonsterStatRow Expected);
