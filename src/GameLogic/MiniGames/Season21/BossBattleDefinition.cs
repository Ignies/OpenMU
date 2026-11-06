// <copyright file="BossBattleDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The definition of the Boss Battle Together of Season 21 (S21Emu <c>Event/BossBattleEvent.xml</c>): one boss out of a
/// few, which everybody fights together; damage, the top damage and the last hit earn the rewards.
/// </summary>
public class BossBattleDefinition
{
    /// <summary>
    /// Gets or sets the numbers of the bosses, one of which is chosen at random.
    /// </summary>
    public List<short> Bosses { get; set; } = [];

    /// <summary>
    /// Gets or sets the x coordinate of the boss.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate of the boss.
    /// </summary>
    public byte Y { get; set; }

    /// <summary>
    /// Gets or sets the health of the boss.
    /// </summary>
    public long Health { get; set; }

    /// <summary>
    /// Gets or sets the health which the boss gets for each <see cref="BonusHealthPlayers"/> players.
    /// </summary>
    public long BonusHealth { get; set; }

    /// <summary>
    /// Gets or sets the number of players for one <see cref="BonusHealth"/>.
    /// </summary>
    public int BonusHealthPlayers { get; set; } = 1;

    /// <summary>
    /// Gets or sets the damage a player has to do for the main rewards.
    /// </summary>
    public long DamageForRewards { get; set; }

    /// <summary>
    /// Gets or sets the rewards of a player who did <see cref="DamageForRewards"/>.
    /// </summary>
    public List<RewardItem> DamageRewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the rewards of the player with the most damage.
    /// </summary>
    public List<RewardItem> TopDamageRewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the rewards of the player with the last hit.
    /// </summary>
    public List<RewardItem> LastHitRewards { get; set; } = [];
}
