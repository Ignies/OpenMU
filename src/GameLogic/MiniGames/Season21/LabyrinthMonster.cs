// <copyright file="LabyrinthMonster.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// Monsters which a mission of a room of the Labyrinth of Dimensions spawns.
/// </summary>
public class LabyrinthMonster
{
    /// <summary>
    /// Gets or sets the number of the monster.
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets how many are in the room at once.
    /// </summary>
    public int Count { get; set; }
}
