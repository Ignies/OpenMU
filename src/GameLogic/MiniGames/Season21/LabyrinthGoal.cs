// <copyright file="LabyrinthGoal.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// A goal of a mission of a room of the Labyrinth of Dimensions.
/// </summary>
public class LabyrinthGoal
{
    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public LabyrinthGoalKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the target: monsters, minutes, damage, seconds or experience by the kind.
    /// </summary>
    public long Target { get; set; }
}
