// <copyright file="LabyrinthRoom.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// A room of a path of the Labyrinth of Dimensions.
/// </summary>
/// <param name="Area">The number of the <see cref="LabyrinthArea"/> it shows.</param>
/// <param name="Type">The kind of room.</param>
/// <param name="Index">The index of its <see cref="LabyrinthMission"/>.</param>
/// <param name="Portals">The portals of the sides, by <see cref="LabyrinthDirection"/>.</param>
public readonly record struct LabyrinthRoom(int Area, LabyrinthAreaType Type, int Index, IReadOnlyList<LabyrinthPortal> Portals)
{
    /// <summary>
    /// Gets the room which can't be entered.
    /// </summary>
    public static LabyrinthRoom None { get; } = new(0, LabyrinthAreaType.None, 0, [LabyrinthPortal.Closed, LabyrinthPortal.Closed, LabyrinthPortal.Closed, LabyrinthPortal.Closed]);
}
