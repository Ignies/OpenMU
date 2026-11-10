// <copyright file="EvolutionTrial.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

using System.Threading;
using MUnique.OpenMU.GameLogic.NPC;

/// <summary>
/// A running trial step of one player: its monsters on the trial map, which come back after their respawn delay
/// until the trial ends (time limit, the player left the map, or it started another step).
/// </summary>
internal sealed class EvolutionTrial : IEventStateProvider
{
    /// <summary>
    /// The distance from the tile of a spawn within which the monster spawns, as the tiles of the data aren't all walkable.
    /// </summary>
    private const byte SpawnRange = 3;

    private readonly Player _owner;

    private readonly GameMap _map;

    private readonly Func<Player, ValueTask> _onTimeout;

    private readonly List<Monster> _monsters = [];

    private readonly CancellationTokenSource _cancellation = new();

    private volatile bool _ended;

    private DateTime _startedAt = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvolutionTrial"/> class.
    /// </summary>
    /// <param name="owner">The player who started the trial.</param>
    /// <param name="step">The step.</param>
    /// <param name="map">The trial map.</param>
    /// <param name="onTimeout">Called when the time limit is over.</param>
    public EvolutionTrial(Player owner, EvolutionTrialStep step, GameMap map, Func<Player, ValueTask> onTimeout)
    {
        this._owner = owner;
        this.Step = step;
        this._map = map;
        this._onTimeout = onTimeout;
    }

    /// <summary>
    /// Gets the step.
    /// </summary>
    public EvolutionTrialStep Step { get; }

    /// <inheritdoc />
    public bool IsEventRunning => !this._ended;

    /// <summary>
    /// Gets the time left until the time limit ends the trial; zero when it ended.
    /// </summary>
    public TimeSpan TimeLeft
    {
        get
        {
            var left = this._startedAt.Add(this.Step.TimeLimit) - DateTime.UtcNow;
            return this._ended || left < TimeSpan.Zero ? TimeSpan.Zero : left;
        }
    }

    /// <summary>
    /// Gets the monsters of the trial.
    /// </summary>
    public IReadOnlyList<Monster> Monsters => this._monsters;

    /// <inheritdoc />
    public bool IsSpawnWaveActive(byte waveNumber) => !this._ended;

    /// <summary>
    /// Determines whether the monster belongs to this trial.
    /// </summary>
    /// <param name="monster">The monster.</param>
    /// <returns><c>true</c>, if it belongs to this trial.</returns>
    public bool Contains(Monster monster) => this._monsters.Contains(monster);

    /// <summary>
    /// Spawns the monsters and starts the time limit.
    /// </summary>
    public async ValueTask StartAsync()
    {
        var gameContext = this._owner.GameContext;
        this._startedAt = DateTime.UtcNow;
        foreach (var spawn in this.Step.Spawns)
        {
            if (gameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == spawn.MonsterNumber) is not { } definition)
            {
                this._owner.Logger.LogWarning("The monster {0} of the trial step {1} doesn't exist.", spawn.MonsterNumber, this.Step.QuestNumber);
                continue;
            }

            var area = new MonsterSpawnArea
            {
                GameMap = this._map.Definition,
                MonsterDefinition = definition,
                SpawnTrigger = SpawnTrigger.AutomaticDuringEvent,
                Quantity = 1,
                X1 = (byte)Math.Max(spawn.X - SpawnRange, byte.MinValue),
                X2 = (byte)Math.Min(spawn.X + SpawnRange, byte.MaxValue),
                Y1 = (byte)Math.Max(spawn.Y - SpawnRange, byte.MinValue),
                Y2 = (byte)Math.Min(spawn.Y + SpawnRange, byte.MaxValue),
            };

            var intelligence = new BasicMonsterIntelligence();
            var monster = new Monster(area, definition, this._map, gameContext.DropGenerator, intelligence, gameContext.PlugInManager, gameContext.PathFinderPool, this);
            intelligence.Npc = monster;
            monster.Initialize();
            this._monsters.Add(monster);
            await this._map.AddAsync(monster).ConfigureAwait(false);
            monster.OnSpawn();
        }

        if (this.Step.TimeLimit > TimeSpan.Zero)
        {
            _ = this.EndAfterTimeLimitAsync(this._cancellation.Token);
        }
    }

    /// <summary>
    /// Ends the trial: its monsters leave the map and don't come back.
    /// </summary>
    public async ValueTask EndAsync()
    {
        if (this._ended)
        {
            return;
        }

        this._ended = true;
        await this._cancellation.CancelAsync().ConfigureAwait(false);
        foreach (var monster in this._monsters.Where(m => !m.IsDisposed && !m.IsDisposing))
        {
            await this._map.RemoveAsync(monster).ConfigureAwait(false);
            monster.Dispose();
        }

        this._cancellation.Dispose();
    }

    private async Task EndAfterTimeLimitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(this.Step.TimeLimit, cancellationToken).ConfigureAwait(false);
            await this._onTimeout(this._owner).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // ended before the time limit
        }
        catch (Exception ex)
        {
            this._owner.Logger.LogError(ex, "Unexpected error when ending the trial step {0}.", this.Step.QuestNumber);
        }
    }
}
