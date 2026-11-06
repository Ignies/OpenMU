// <copyright file="LabyrinthStanding.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The standing of a character in the labyrinth league.
/// </summary>
/// <param name="Tier">The tier.</param>
/// <param name="Rank">The rank in the group of the current week, 0 when it has no points this week.</param>
/// <param name="GroupSize">The number of characters of the group.</param>
/// <param name="Points">The points of the current week.</param>
/// <param name="FloorsToday">The number of floors cleared today.</param>
public readonly record struct LabyrinthStanding(LabyrinthLeagueTier Tier, int Rank, int GroupSize, int Points, int FloorsToday);
