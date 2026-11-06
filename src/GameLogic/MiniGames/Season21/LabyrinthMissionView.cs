// <copyright file="LabyrinthMissionView.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The mission of the room a character is in.
/// </summary>
/// <param name="Type">The kind of room.</param>
/// <param name="Index">The index of the mission.</param>
/// <param name="Goals">The goals and their progress.</param>
/// <param name="Completed">A value indicating whether the mission is done.</param>
public sealed record LabyrinthMissionView(LabyrinthAreaType Type, int Index, IReadOnlyList<LabyrinthGoalProgress> Goals, bool Completed);
