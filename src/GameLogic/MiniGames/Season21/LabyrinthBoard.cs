// <copyright file="LabyrinthBoard.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The board of the floor a character plays, and the room it is in.
/// </summary>
/// <param name="Path">The number of the path.</param>
/// <param name="Room">The index of the room, row * 10 + column.</param>
/// <param name="Area">The area of the room.</param>
/// <param name="Type">The kind of the room.</param>
/// <param name="Portals">The portals of the room as they are now: <see cref="LabyrinthPortal.Mission"/> while the mission
/// isn't done, <see cref="LabyrinthPortal.Open"/> after it.</param>
/// <param name="Discovered">The rooms the character opened on this floor, by index.</param>
/// <param name="Cleared">The rooms whose mission is done, by index.</param>
public sealed record LabyrinthBoard(int Path, int Room, LabyrinthArea Area, LabyrinthAreaType Type, IReadOnlyList<LabyrinthPortal> Portals, IReadOnlyList<bool> Discovered, IReadOnlyList<bool> Cleared);
