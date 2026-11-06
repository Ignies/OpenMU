// <copyright file="LabyrinthGoalProgress.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The progress of a goal of a mission of the Labyrinth of Dimensions.
/// </summary>
/// <param name="Kind">The kind of the goal.</param>
/// <param name="Target">The target.</param>
/// <param name="Progress">The progress towards the target.</param>
public readonly record struct LabyrinthGoalProgress(LabyrinthGoalKind Kind, long Target, long Progress);
