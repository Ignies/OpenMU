// <copyright file="RoundsEventDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The definition of a Season 21 event which is played in rounds of monsters: the Tormented Square (rounds), the
/// Labyrinth of Dimensions (floors) and the Protector of Acheron (waves).
/// </summary>
public class RoundsEventDefinition
{
    /// <summary>
    /// Gets or sets the rounds, played one after the other.
    /// </summary>
    public List<EventRound> Rounds { get; set; } = [];

    /// <summary>
    /// Gets or sets the spawn points.
    /// </summary>
    public List<EventSpawnPoint> SpawnPoints { get; set; } = [];

    /// <summary>
    /// Gets or sets the longest time of a round; a round ends earlier when all its monsters are dead.
    /// </summary>
    public TimeSpan RoundDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the pause after a round.
    /// </summary>
    public TimeSpan PauseAfterRound { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the pause after a boss round.
    /// </summary>
    public TimeSpan PauseAfterBossRound { get; set; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Gets or sets a value indicating whether the event ends as soon as the monsters of a round weren't killed in time,
    /// as on a floor of the labyrinth; otherwise the next round starts.
    /// </summary>
    public bool EndsWhenARoundFails { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the points of a party count together for the rank (a team).
    /// </summary>
    public bool PartyIsATeam { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum points for the rewards of the definition; 0 for none.
    /// </summary>
    public int MinimumPointsForRewards { get; set; }
}
