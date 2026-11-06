// <copyright file="EventBonusMonster.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// Monsters which may spawn in a round besides its own, with their own points.
/// </summary>
public class EventBonusMonster
{
    /// <summary>
    /// Gets or sets the number of the monster.
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets how many spawn.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the kind of spawn points they spawn at.
    /// </summary>
    public byte SpawnKind { get; set; }

    /// <summary>
    /// Gets or sets the points of a kill.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets the chance in percent that they spawn in the round.
    /// </summary>
    public int ChancePercent { get; set; } = 100;
}
