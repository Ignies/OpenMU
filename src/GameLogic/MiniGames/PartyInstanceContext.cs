// <copyright file="PartyInstanceContext.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

using System.Collections.Concurrent;
using System.Threading;
using MUnique.OpenMU.GameLogic.NPC;

/// <summary>
/// The context of a party instance of Season 21: a copy of a map for one party (<see cref="MiniGameMapCreationPolicy.OnePerParty"/>),
/// with its own monsters and timer.
/// </summary>
/// <remarks>
/// The instance starts right after its short entry phase, and members of the party may join it while it runs. It ends
/// when the time is over, or as soon as the last player left it, so the party gets a fresh copy next time.
/// An instance whose map has monsters which spawn once (at the event start or at a wave start) is cleared when all of
/// them are dead after the last wave started; then the players succeeded and get the rewards of the definition.
/// An instance without such monsters is only a place, e.g. the trial of the class quests spawns its own monsters.
/// </remarks>
public class PartyInstanceContext : MiniGameContext
{
    private readonly ConcurrentDictionary<Monster, byte> _onceSpawned = new();

    private readonly TimeSpan _lastWaveStart;

    private readonly bool _canBeCleared;

    private DateTime _startedAt = DateTime.MaxValue;

    private int _ending;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyInstanceContext"/> class.
    /// </summary>
    /// <param name="key">The key of this context.</param>
    /// <param name="definition">The definition of the mini game.</param>
    /// <param name="gameContext">The game context to which this game belongs.</param>
    /// <param name="mapInitializer">The map initializer, which is used when the event starts.</param>
    public PartyInstanceContext(MiniGameMapKey key, MiniGameDefinition definition, IGameContext gameContext, IMapInitializer mapInitializer)
        : base(key, definition, gameContext, mapInitializer)
    {
        this._lastWaveStart = definition.SpawnWaves.Select(w => w.StartTime).DefaultIfEmpty(TimeSpan.Zero).Max();
        this._canBeCleared = definition.Entrance?.Map?.MonsterSpawns.Any(IsOnceSpawn) is true;
    }

    /// <summary>
    /// Gets a value indicating whether the players cleared the instance.
    /// </summary>
    public bool IsCleared { get; private set; }

    /// <inheritdoc />
    protected override bool AllowEnterWhilePlaying => true;

    /// <inheritdoc />
    protected override TimeSpan CountdownDuration => TimeSpan.Zero;

    /// <inheritdoc />
    protected override TimeSpan MinimumEnterDuration => TimeSpan.Zero;

    /// <inheritdoc />
    protected override TimeSpan RemainingTime
    {
        get
        {
            var left = this._startedAt.Add(this.Definition.GameDuration) - DateTime.UtcNow;
            return left > TimeSpan.Zero ? left : TimeSpan.Zero;
        }
    }

    /// <inheritdoc />
    protected override async ValueTask OnGameStartAsync(ICollection<Player> players)
    {
        this._startedAt = DateTime.UtcNow;
        await base.OnGameStartAsync(players).ConfigureAwait(false);
        foreach (var player in players)
        {
            await this.ShowStateAsync(player).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override bool IsWinner(Player player) => this.IsCleared;

    /// <inheritdoc />
    protected override bool IsInWinningParty(Player player) => this.IsCleared;

    /// <inheritdoc />
    protected override async ValueTask OnObjectAddedToMapAsync((GameMap Map, ILocateable Object) args)
    {
        await base.OnObjectAddedToMapAsync(args).ConfigureAwait(false);
        if (args.Object is Player player && this.State == MiniGameState.Playing)
        {
            await this.ShowStateAsync(player).ConfigureAwait(false);
        }

        if (args.Object is Monster { SpawnArea: { } area } monster && IsOnceSpawn(area))
        {
            this._onceSpawned.TryAdd(monster, 0);
        }
    }

    /// <inheritdoc />
    protected override void OnMonsterDied(object? sender, DeathInformation e)
    {
        base.OnMonsterDied(sender, e);
        if (!this._canBeCleared
            || sender is not Monster monster
            || !this._onceSpawned.TryRemove(monster, out _)
            || !this._onceSpawned.IsEmpty
            || DateTime.UtcNow - this._startedAt < this._lastWaveStart)
        {
            return;
        }

        this.IsCleared = true;
        this.FinishEvent();
    }

    /// <inheritdoc />
    protected override async ValueTask GameEndedAsync(ICollection<Player> finishers)
    {
        await base.GameEndedAsync(finishers).ConfigureAwait(false);
        foreach (var player in finishers)
        {
            await player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowResultAsync(this.Definition.Type, this.IsCleared, 0, this.IsCleared ? 1 : 0, 0)).ConfigureAwait(false);
        }

        if (!this.IsCleared)
        {
            return;
        }

        foreach (var player in finishers)
        {
            // Everybody who cleared the instance shares the first rank.
            await this.GiveRewardsAndGetBonusScoreAsync(player, 1).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override async ValueTask OnObjectRemovedFromMapAsync((GameMap Map, ILocateable Object) args)
    {
        await base.OnObjectRemovedFromMapAsync(args).ConfigureAwait(false);
        if (args.Object is Player && this.PlayerCount == 0 && !this.Map.GetPlayers().Any() && Interlocked.Exchange(ref this._ending, 1) == 0)
        {
            // The party left: the copy goes away now, not only after the exit time, so the party can start a new one.
            _ = Task.Run(async () => await this.DisposeAsync().ConfigureAwait(false));
        }
    }

    private static bool IsOnceSpawn(MonsterSpawnArea area)
        => area.SpawnTrigger is SpawnTrigger.OnceAtEventStart or SpawnTrigger.OnceAtWaveStart;

    private ValueTask ShowStateAsync(Player player)
        => player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowStateAsync(this.Definition.Type, this.Definition.GameLevel, this.State, (uint)this.RemainingTime.TotalSeconds));
}
