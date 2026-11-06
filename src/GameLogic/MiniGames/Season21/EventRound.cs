// <copyright file="EventRound.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// A round (or floor) of a Season 21 rounds event: monsters which spawn together and are killed for points.
/// </summary>
public class EventRound
{
    /// <summary>
    /// Gets or sets the number of the monster.
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets how many monsters spawn.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the kind of spawn points they spawn at (<see cref="EventSpawnPoint.Kind"/>).
    /// </summary>
    public byte SpawnKind { get; set; }

    /// <summary>
    /// Gets or sets the points of a kill.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets the points which every player gets when all monsters of the round are dead in time.
    /// </summary>
    public int ClearPoints { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether it's a boss round, which takes the longer pause after it.
    /// </summary>
    public bool IsBossRound { get; set; }

    /// <summary>
    /// Gets or sets the multiplier of the health of the monsters.
    /// </summary>
    public float HealthMultiplier { get; set; } = 1;

    /// <summary>
    /// Gets or sets the multiplier of the damage of the monsters.
    /// </summary>
    public float DamageMultiplier { get; set; } = 1;

    /// <summary>
    /// Gets or sets the multiplier of the defense of the monsters.
    /// </summary>
    public float DefenseMultiplier { get; set; } = 1;

    /// <summary>
    /// Gets or sets the bonus monsters which may spawn in the round, e.g. the golden monsters of the Tormented Square.
    /// </summary>
    public List<EventBonusMonster> BonusMonsters { get; set; } = [];

    /// <summary>
    /// Gets or sets the Ruud which every player gets when the round is cleared in time.
    /// </summary>
    public int ClearRuud { get; set; }

    /// <summary>
    /// Gets or sets the items which every player gets when the round is cleared in time, into the gremory case when it's on.
    /// </summary>
    public List<RewardItem> ClearRewards { get; set; } = [];
}
