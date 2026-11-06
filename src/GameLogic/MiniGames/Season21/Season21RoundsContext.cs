// <copyright file="Season21RoundsContext.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Collections.Concurrent;
using System.Threading;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Ruud;

/// <summary>
/// The context of a Season 21 event which is played in rounds of monsters (<see cref="RoundsEventDefinition"/>): the
/// Tormented Square, the Labyrinth of Dimensions and the Protector of Acheron.
/// </summary>
/// <remarks>
/// The rounds run one after the other. A round spawns its monsters (and maybe its bonus monsters) at the spawn points of
/// their kind; it ends when all of them are dead or its time is over. A kill gives its points to the killer; a round
/// cleared in time gives its clear points to every player alive. After the last round, or when a round failed in an
/// event which ends then, the players are ranked by their points (a party counts together when it's a team), get the
/// rewards of the definition for their rank and the result.
/// A game whose map isn't shared (an instance) starts right after its entry phase and takes players while it runs.
/// </remarks>
public sealed class Season21RoundsContext : MiniGameContext
{
    private readonly RoundsEventDefinition _rounds;

    private readonly ConcurrentDictionary<Player, int> _scores = new();

    private readonly ConcurrentDictionary<Monster, int> _roundMonsters = new();

    private readonly bool _isInstance;

    private TaskCompletionSource _roundCleared = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private DateTime _roundEndsAt;

    private int _round;

    private int _clearedRounds;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21RoundsContext"/> class.
    /// </summary>
    /// <param name="key">The key of this context.</param>
    /// <param name="definition">The definition of the mini game.</param>
    /// <param name="gameContext">The game context to which this game belongs.</param>
    /// <param name="mapInitializer">The map initializer, which is used when the event starts.</param>
    /// <param name="rounds">The rounds.</param>
    public Season21RoundsContext(MiniGameMapKey key, MiniGameDefinition definition, IGameContext gameContext, IMapInitializer mapInitializer, RoundsEventDefinition rounds)
        : base(key, definition, gameContext, mapInitializer)
    {
        this._rounds = rounds;
        this._isInstance = definition.MapCreationPolicy != MiniGameMapCreationPolicy.Shared;
    }

    /// <summary>
    /// Gets the current round, starting at 1; 0 before the first.
    /// </summary>
    public int Round => this._round;

    /// <summary>
    /// Gets the number of rounds which were cleared in time.
    /// </summary>
    public int ClearedRounds => this._clearedRounds;

    /// <inheritdoc />
    protected override bool AllowEnterWhilePlaying => this._isInstance;

    /// <inheritdoc />
    protected override TimeSpan CountdownDuration => this._isInstance ? TimeSpan.Zero : base.CountdownDuration;

    /// <inheritdoc />
    protected override TimeSpan MinimumEnterDuration => this._isInstance ? TimeSpan.Zero : base.MinimumEnterDuration;

    /// <summary>
    /// Gets the points of a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The points.</returns>
    public int GetScore(Player player) => this._scores.GetValueOrDefault(player);

    /// <inheritdoc />
    protected override async ValueTask OnGameStartAsync(ICollection<Player> players)
    {
        await base.OnGameStartAsync(players).ConfigureAwait(false);
        foreach (var player in players)
        {
            this._scores.TryAdd(player, 0);
        }

        var token = this.GameEndedToken;
        _ = Task.Run(() => this.RunRoundsAsync(token), token);
    }

    /// <inheritdoc />
    protected override void OnMonsterDied(object? sender, DeathInformation e)
    {
        base.OnMonsterDied(sender, e);
        if (sender is not Monster monster || !this._roundMonsters.TryRemove(monster, out var points))
        {
            return;
        }

        if (this.Map.GetObject(e.KillerId) is Player killer)
        {
            this._scores.AddOrUpdate(killer, points, (_, score) => score + points);
            _ = this.ShowScoreAsync(killer).AsTask();
        }

        if (this._roundMonsters.IsEmpty)
        {
            this._roundCleared.TrySetResult();
        }
    }

    /// <inheritdoc />
    protected override async ValueTask GameEndedAsync(ICollection<Player> finishers)
    {
        await base.GameEndedAsync(finishers).ConfigureAwait(false);
        var success = this._clearedRounds == this._rounds.Rounds.Count;
        foreach (var (player, rank, score) in this.Rank(finishers))
        {
            if (score >= this._rounds.MinimumPointsForRewards && score > 0)
            {
                await this.GiveRewardsAndGetBonusScoreAsync(player, rank).ConfigureAwait(false);
            }

            await this.NotifyObjectiveReachedAsync(player, MiniGameObjective.Rank, rank).ConfigureAwait(false);
            await player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowResultAsync(this.Definition.Type, success, score, rank, 0)).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override async ValueTask ShowScoreAsync(Player player)
    {
        var score = this.GetScore(player);
        var rank = this.Rank(this._scores.Keys.ToList()).FirstOrDefault(r => r.Player == player).Rank;
        var left = this._roundEndsAt - DateTime.UtcNow;
        await player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowScoreAsync(this.Definition.Type, (byte)this._round, score, rank, this.PlayerCount, left > TimeSpan.Zero ? (uint)left.TotalSeconds : 0)).ConfigureAwait(false);
    }

    private IEnumerable<(Player Player, int Rank, int Score)> Rank(ICollection<Player> players)
    {
        // A team is a party, when the event counts parties together; a player without a party is its own team.
        var teams = players
            .GroupBy(p => this._rounds.PartyIsATeam && p.Party is { } party ? (object)party : p)
            .Select(team => (Players: team.ToList(), Score: team.Sum(this.GetScore)))
            .OrderByDescending(team => team.Score)
            .ToList();
        var rank = 0;
        foreach (var team in teams)
        {
            rank++;
            foreach (var player in team.Players)
            {
                yield return (player, rank, team.Players.Count == 1 ? team.Score : this.GetScore(player));
            }
        }
    }

    private async Task RunRoundsAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (var index = 0; index < this._rounds.Rounds.Count; index++)
            {
                var round = this._rounds.Rounds[index];
                this._round = index + 1;
                var cleared = await this.PlayRoundAsync(round, cancellationToken).ConfigureAwait(false);
                if (cleared)
                {
                    Interlocked.Increment(ref this._clearedRounds);
                    await this.ForEachPlayerAsync(player => this.RewardClearedRoundAsync(player, round).AsTask()).ConfigureAwait(false);
                }
                else if (this._rounds.EndsWhenARoundFails)
                {
                    break;
                }
                else
                {
                    // The next round starts anyway.
                }

                if (index < this._rounds.Rounds.Count - 1)
                {
                    await this.DelayWithSkipAsync(round.IsBossRound ? this._rounds.PauseAfterBossRound : this._rounds.PauseAfterRound, cancellationToken).ConfigureAwait(false);
                }
            }

            this.FinishEvent();
        }
        catch (OperationCanceledException)
        {
            // The game ended.
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "{context}: Unexpected error in the rounds.", this);
        }
    }

    private async ValueTask RewardClearedRoundAsync(Player player, EventRound round)
    {
        if (player.IsAlive)
        {
            if (round.ClearPoints > 0)
            {
                this._scores.AddOrUpdate(player, round.ClearPoints, (_, score) => score + round.ClearPoints);
            }

            if (round.ClearRuud > 0)
            {
                await player.AddRuudAsync(round.ClearRuud).ConfigureAwait(false);
            }

            foreach (var reward in round.ClearRewards)
            {
                await RewardDelivery.DeliverAsync(player, reward).ConfigureAwait(false);
            }

            if (round.ClearRandomItems.Count > 0)
            {
                await RewardDelivery.DeliverAsync(player, round.ClearRandomItems[Rand.NextInt(0, round.ClearRandomItems.Count)]).ConfigureAwait(false);
            }
        }

        await this.ShowScoreAsync(player).ConfigureAwait(false);
    }

    private async ValueTask<bool> PlayRoundAsync(EventRound round, CancellationToken cancellationToken)
    {
        var roundCleared = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        this._roundCleared = roundCleared;
        this._roundEndsAt = DateTime.UtcNow.Add(this._rounds.RoundDuration);
        await this.SpawnAsync(round.MonsterNumber, round.Count, round.SpawnKind, round.Points, round).ConfigureAwait(false);
        foreach (var bonus in round.BonusMonsters.Where(b => Rand.NextRandomBool(b.ChancePercent)))
        {
            await this.SpawnAsync(bonus.MonsterNumber, bonus.Count, bonus.SpawnKind, bonus.Points, null).ConfigureAwait(false);
        }

        if (this._roundMonsters.IsEmpty)
        {
            this.Logger.LogWarning("{context}: Round {round} spawned no monster.", this, this._round);
            return false;
        }

        await this.ForEachPlayerAsync(player => this.ShowScoreAsync(player).AsTask()).ConfigureAwait(false);
        var timeout = Task.Delay(this._rounds.RoundDuration, cancellationToken);
        var cleared = await Task.WhenAny(roundCleared.Task, timeout).ConfigureAwait(false) == roundCleared.Task;
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var monster in this._roundMonsters.Keys.ToList())
        {
            this._roundMonsters.TryRemove(monster, out _);
            await EventMonsterSpawner.RemoveAsync(this, monster).ConfigureAwait(false);
        }

        return cleared;
    }

    private async ValueTask SpawnAsync(short monsterNumber, int count, byte kind, int points, EventRound? multipliers)
    {
        if (this.GameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == monsterNumber) is not { } definition)
        {
            this.Logger.LogWarning("{context}: The monster {monster} of round {round} doesn't exist.", this, monsterNumber, this._round);
            return;
        }

        var candidates = this._rounds.SpawnPoints.Where(p => p.Kind == kind).ToList();
        if (candidates.Count == 0)
        {
            candidates = this._rounds.SpawnPoints;
        }

        for (var i = 0; i < count && candidates.Count > 0; i++)
        {
            var point = candidates[i % candidates.Count];
            var monster = await EventMonsterSpawner.SpawnAsync(this, definition, point.X, point.Y, point.Range, multipliers?.HealthMultiplier ?? 1, multipliers?.DamageMultiplier ?? 1, multipliers?.DefenseMultiplier ?? 1).ConfigureAwait(false);
            if (monster is not null)
            {
                this._roundMonsters.TryAdd(monster, points);
            }
        }
    }
}
