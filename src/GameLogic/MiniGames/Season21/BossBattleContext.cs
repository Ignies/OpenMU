// <copyright file="BossBattleContext.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Collections.Concurrent;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The context of the Boss Battle Together of Season 21 (<see cref="BossBattleDefinition"/>).
/// </summary>
/// <remarks>
/// When the game starts, one of the bosses spawns with the health of the definition plus its bonus for the number of
/// players. Every player's damage on it is counted (<see cref="BossBattleDamagePlugIn"/>). The game ends when the boss
/// is dead or the time is over. A player with the damage of the definition gets the damage rewards, the player with
/// the most damage the top damage rewards and the killer the last hit rewards, into the gremory case when it's on.
/// </remarks>
public sealed class BossBattleContext : MiniGameContext
{
    /// <summary>
    /// The shortest time between two score packets of a player while it hits the boss.
    /// </summary>
    private static readonly TimeSpan ScoreInterval = TimeSpan.FromSeconds(1);

    private readonly BossBattleDefinition _battle;

    private readonly ConcurrentDictionary<Player, long> _damage = new();

    private readonly ConcurrentDictionary<Player, DateTime> _lastScore = new();

    private Player? _lastHitter;

    /// <summary>
    /// Whether the boss died: the end of the game clears it from the map before the results, so its state can't tell.
    /// </summary>
    private bool _bossKilled;

    /// <summary>
    /// Initializes a new instance of the <see cref="BossBattleContext"/> class.
    /// </summary>
    /// <param name="key">The key of this context.</param>
    /// <param name="definition">The definition of the mini game.</param>
    /// <param name="gameContext">The game context to which this game belongs.</param>
    /// <param name="mapInitializer">The map initializer, which is used when the event starts.</param>
    /// <param name="battle">The battle.</param>
    public BossBattleContext(MiniGameMapKey key, MiniGameDefinition definition, IGameContext gameContext, IMapInitializer mapInitializer, BossBattleDefinition battle)
        : base(key, definition, gameContext, mapInitializer)
    {
        this._battle = battle;
    }

    /// <summary>
    /// Gets the boss, once it spawned.
    /// </summary>
    public Monster? Boss { get; private set; }

    /// <summary>
    /// Gets the damage which a player did on the boss.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The damage.</returns>
    public long GetDamage(Player player) => this._damage.GetValueOrDefault(player);

    /// <summary>
    /// Counts a hit of a player on the boss.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="damage">The damage.</param>
    public void RegisterHit(Player player, long damage)
    {
        if (this.State != MiniGameState.Playing || damage <= 0)
        {
            return;
        }

        this._damage.AddOrUpdate(player, damage, (_, sum) => sum + damage);
        var now = DateTime.UtcNow;
        if (now - this._lastScore.GetValueOrDefault(player) >= ScoreInterval)
        {
            this._lastScore[player] = now;
            _ = this.ShowScoreAsync(player).AsTask();
        }
    }

    /// <inheritdoc />
    protected override async ValueTask OnGameStartAsync(ICollection<Player> players)
    {
        await base.OnGameStartAsync(players).ConfigureAwait(false);
        var bosses = this._battle.Bosses
            .Select(number => this.GameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == number))
            .OfType<MonsterDefinition>()
            .ToList();
        if (bosses.Count == 0)
        {
            this.Logger.LogWarning("{context}: None of the bosses exists.", this);
            this.FinishEvent();
            return;
        }

        var bonus = this._battle.BonusHealthPlayers > 0 ? players.Count / this._battle.BonusHealthPlayers * this._battle.BonusHealth : 0;
        this.Boss = await EventMonsterSpawner.SpawnAsync(this, bosses[Rand.NextInt(0, bosses.Count)], this._battle.X, this._battle.Y, 1, healthOverride: this._battle.Health + bonus).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void OnMonsterDied(object? sender, DeathInformation e)
    {
        base.OnMonsterDied(sender, e);
        if (sender is not Monster monster || monster != this.Boss)
        {
            return;
        }

        this._lastHitter = this.Map.GetObject(e.KillerId) as Player;
        this._bossKilled = true;
        this.FinishEvent();
    }

    /// <inheritdoc />
    protected override async ValueTask GameEndedAsync(ICollection<Player> finishers)
    {
        await base.GameEndedAsync(finishers).ConfigureAwait(false);
        var killed = this._bossKilled;
        var ranked = finishers.OrderByDescending(this.GetDamage).ToList();
        var top = ranked.FirstOrDefault(p => this.GetDamage(p) > 0);
        for (var index = 0; index < ranked.Count; index++)
        {
            var player = ranked[index];
            var damage = this.GetDamage(player);
            if (damage >= this._battle.DamageForRewards && damage > 0)
            {
                await DeliverAsync(player, this._battle.DamageRewards).ConfigureAwait(false);
            }

            if (player == top)
            {
                await DeliverAsync(player, this._battle.TopDamageRewards).ConfigureAwait(false);
            }

            if (killed && player == this._lastHitter)
            {
                await DeliverAsync(player, this._battle.LastHitRewards).ConfigureAwait(false);
            }

            await this.NotifyObjectiveReachedAsync(player, MiniGameObjective.Rank, index + 1).ConfigureAwait(false);
            await player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowResultAsync(this.Definition.Type, killed, (int)Math.Min(damage, int.MaxValue), index + 1, 0)).ConfigureAwait(false);
        }

        if (this.Boss is { IsAlive: true } boss)
        {
            await EventMonsterSpawner.RemoveAsync(this, boss).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override async ValueTask ShowScoreAsync(Player player)
    {
        var damage = this.GetDamage(player);
        var rank = this._damage.Values.Count(d => d > damage) + 1;
        await player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowScoreAsync(this.Definition.Type, 0, (int)Math.Min(damage, int.MaxValue), rank, this.PlayerCount, 0)).ConfigureAwait(false);
    }

    private static async ValueTask DeliverAsync(Player player, IEnumerable<RewardItem> rewards)
    {
        foreach (var reward in rewards)
        {
            await RewardDelivery.DeliverAsync(player, reward).ConfigureAwait(false);
        }
    }
}
