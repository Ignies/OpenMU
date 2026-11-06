// <copyright file="RankingEntry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HuntingRecord;

/// <summary>
/// A character of a ranking.
/// </summary>
/// <param name="Rank">The rank, from 1.</param>
/// <param name="Name">The name of the character.</param>
/// <param name="ClassNumber">The number of the character class.</param>
/// <param name="Value">The value the ranking orders by.</param>
public readonly record struct RankingEntry(int Rank, string Name, byte ClassNumber, long Value);
