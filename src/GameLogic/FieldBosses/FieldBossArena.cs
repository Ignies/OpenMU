// <copyright file="FieldBossArena.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

using MUnique.OpenMU.Pathfinding;

/// <summary>
/// The area around a field boss, which its gates, its entry NPC or a walk lead into.
/// </summary>
public class FieldBossArena
{
    /// <summary>
    /// Gets or sets the lowest x coordinate.
    /// </summary>
    public byte X1 { get; set; }

    /// <summary>
    /// Gets or sets the lowest y coordinate.
    /// </summary>
    public byte Y1 { get; set; }

    /// <summary>
    /// Gets or sets the highest x coordinate.
    /// </summary>
    public byte X2 { get; set; }

    /// <summary>
    /// Gets or sets the highest y coordinate.
    /// </summary>
    public byte Y2 { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate where the entry NPC puts the players.
    /// </summary>
    public byte EntryX { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate where the entry NPC puts the players.
    /// </summary>
    public byte EntryY { get; set; }

    /// <summary>
    /// Gets or sets the most players in the arena; 0 for no limit.
    /// </summary>
    [Range(0, 1000)]
    public int MaximumPlayers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the arena closes to newcomers once the fight started.
    /// </summary>
    public bool ClosesWhenEngaged { get; set; }

    /// <summary>
    /// Determines whether the arena contains a point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns><c>true</c>, if the point is inside.</returns>
    public bool Contains(Point point) => point.X >= this.X1 && point.X <= this.X2 && point.Y >= this.Y1 && point.Y <= this.Y2;
}
