// <copyright file="EventSpawnPoint.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// A point on the map of a Season 21 event around which monsters of a kind spawn.
/// </summary>
public class EventSpawnPoint
{
    /// <summary>
    /// Gets or sets the kind of monsters which spawn here, e.g. 0 normal, 1 boss, 2 bonus.
    /// </summary>
    public byte Kind { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate.
    /// </summary>
    public byte Y { get; set; }

    /// <summary>
    /// Gets or sets the distance from the point within which a monster spawns.
    /// </summary>
    public byte Range { get; set; } = 5;
}
