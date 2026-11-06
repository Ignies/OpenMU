// <copyright file="FieldBossesFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The field bosses of Season 21 (Medusa, Lord Silvester, Core Magriffy, Lord of Ferea, Nix, God of Darkness):
/// spawned by the shared random respawn, fought in stages, and with an arena which its gates and entry NPC lead into.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.FieldBossesFeaturePlugIn_Name), Description = nameof(PlugInResources.FieldBossesFeaturePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8E2C4A17-6D3B-4F95-A1C8-2B7E9D0F5A63")]
public class FieldBossesFeaturePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<FieldBossConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IPeriodicTaskPlugIn, IAttackableGotKilledPlugIn, IAttackableGotHitPlugIn, IWarpGateEnteringPlugIn, IWalkRequestValidationPlugIn, IPlayerTalkToNpcPlugIn, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The tiles around its point within which a stage monster may spawn, when the point itself isn't walkable.
    /// </summary>
    private const int StageSpawnRadius = 2;

    /// <summary>
    /// The distance to the entry NPC within which party members are taken along.
    /// </summary>
    private const int PartyEntryRange = 10;

    private readonly ConditionalWeakTable<IGameContext, World> _worlds = [];

    private FieldBossConfiguration? _configuration;

    private volatile bool _forceSpawn;

    /// <summary>
    /// The result of an arena entry check.
    /// </summary>
    internal enum ArenaEntry
    {
        /// <summary>
        /// The player may enter.
        /// </summary>
        Allowed,

        /// <summary>
        /// The boss isn't there.
        /// </summary>
        NotPresent,

        /// <summary>
        /// The arena holds its maximum of players.
        /// </summary>
        Full,

        /// <summary>
        /// The fight started and the arena is closed.
        /// </summary>
        Closed,
    }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public FieldBossConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._worlds.Clear();
        }
    }

    /// <summary>
    /// Gets or sets the clock; tests replace it.
    /// </summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static FieldBossesFeaturePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<FieldBossesFeaturePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new FieldBossConfiguration();

    /// <summary>
    /// Gets the states of the bosses of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The states.</returns>
    public IReadOnlyList<FieldBossState> GetStates(IGameContext gameContext) => this.GetWorld(gameContext).States;

    /// <summary>
    /// Finds a boss by its name, ignoring the case.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The boss, or <c>null</c>.</returns>
    public FieldBossDefinition? FindBoss(string name)
        => this.Configuration?.Bosses.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        if (this.Configuration is null)
        {
            return;
        }

        var force = this._forceSpawn;
        this._forceSpawn = false;
        var now = this.UtcNow();
        foreach (var state in this.GetWorld(gameContext).States.Where(s => s.Definition.IsActive))
        {
            await state.Lock.WaitAsync().ConfigureAwait(false);
            try
            {
                switch (state.Status)
                {
                    case FieldBossStatus.Waiting when force || now >= state.NextChangeAt:
                        await this.SpawnCoreAsync(gameContext, state).ConfigureAwait(false);
                        break;
                    case FieldBossStatus.Engaged when now >= state.NextChangeAt:
                        await this.FailCoreAsync(gameContext, state).ConfigureAwait(false);
                        break;
                    case FieldBossStatus.Defeated when now >= state.NextChangeAt:
                        await this.MoveOutAsync(gameContext, state.Definition).ConfigureAwait(false);
                        state.Status = FieldBossStatus.Waiting;
                        state.NextChangeAt = state.RespawnAt;
                        await this.ShowStateAsync(gameContext, state).ConfigureAwait(false);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                gameContext.LoggerFactory.CreateLogger<FieldBossesFeaturePlugIn>().LogError(ex, "Field boss {Boss} failed to change its state.", state.Definition);
            }
            finally
            {
                state.Lock.Release();
            }
        }
    }

    /// <inheritdoc />
    public void ForceStart() => this._forceSpawn = true;

    /// <summary>
    /// Spawns a boss now, e.g. by a game master. A boss which lives already is spawned again.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="boss">The boss.</param>
    /// <returns><c>true</c>, if the boss spawned.</returns>
    public async ValueTask<bool> SpawnAsync(IGameContext gameContext, FieldBossDefinition boss)
    {
        if (this.GetWorld(gameContext).States.FirstOrDefault(s => s.Definition == boss) is not { } state)
        {
            return false;
        }

        await state.Lock.WaitAsync().ConfigureAwait(false);
        try
        {
            return await this.SpawnCoreAsync(gameContext, state).ConfigureAwait(false);
        }
        finally
        {
            state.Lock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killed is not Monster monster
            || this.FindByMonster(monster) is not { } found
            || found.State.StageMonster != monster)
        {
            return;
        }

        var (gameContext, state) = found;
        await state.Lock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (state.StageMonster != monster)
            {
                return;
            }

            state.Monsters.TryRemove(monster, out _);
            state.StageMonster = null;
            if (state.StageIndex + 1 < state.Definition.Stages.Count)
            {
                await this.SpawnStageAsync(gameContext, state, state.StageIndex + 1).ConfigureAwait(false);
                await this.ShowStateAsync(gameContext, state).ConfigureAwait(false);
                return;
            }

            await this.DefeatCoreAsync(gameContext, state, monster.LastDeath?.KillerName ?? (killer as Player)?.Name ?? string.Empty).ConfigureAwait(false);
        }
        finally
        {
            state.Lock.Release();
        }
    }

    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo)
    {
        if (attackable is not Monster monster
            || this.FindByMonster(monster) is not { } found
            || found.State.StageMonster != monster
            || !monster.IsAlive)
        {
            return;
        }

        _ = this.OnStageHitAsync(found.GameContext, found.State, monster);
    }

    /// <inheritdoc />
    public async ValueTask WarpGateEnteringAsync(Player player, ExitGate targetGate, CancelEventArgs eventArgs)
    {
        if (eventArgs.Cancel
            || targetGate.Map is not { } map
            || this.FindArenaState(player.GameContext, map.Number, new Point(targetGate.X1, targetGate.Y1)) is not { } state)
        {
            return;
        }

        var entry = await this.CheckArenaEntryAsync(player.GameContext, state, true).ConfigureAwait(false);
        if (entry != ArenaEntry.Allowed)
        {
            eventArgs.Cancel = true;
            await ShowEntryRefusalAsync(player, state, entry).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ValidateWalkRequestAsync(Player player, Memory<WalkingStep> steps, WalkRequestValidationArgs eventArgs)
    {
        if (!eventArgs.IsWalkAllowed
            || this.Configuration is null
            || player.CurrentMap is not { } map
            || this.FindEnteredArena(player.GameContext, map.Definition.Number, steps.Span) is not { } state)
        {
            return;
        }

        var entry = await this.CheckArenaEntryAsync(player.GameContext, state, false).ConfigureAwait(false);
        if (entry != ArenaEntry.Allowed)
        {
            eventArgs.IsWalkAllowed = false;
            await ShowEntryRefusalAsync(player, state, entry).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask PlayerTalksToNpcAsync(Player player, NonPlayerCharacter npc, NpcTalkEventArgs eventArgs)
    {
        if (this.Configuration is null
            || npc.Definition.Number == 0
            || player.CurrentMap is not { } map
            || this.GetWorld(player.GameContext).States.FirstOrDefault(s =>
                s.Definition.EntryNpcNumber == npc.Definition.Number && s.Definition.MapNumber == map.Definition.Number && s.Definition.Arena is not null) is not { } state)
        {
            return;
        }

        eventArgs.HasBeenHandled = true;
        var entry = await this.CheckArenaEntryAsync(player.GameContext, state, true).ConfigureAwait(false);
        if (entry != ArenaEntry.Allowed)
        {
            await ShowEntryRefusalAsync(player, state, entry).ConfigureAwait(false);
            return;
        }

        var arena = state.Definition.Arena!;
        var gate = new ExitGate { Map = map.Definition, X1 = arena.EntryX, X2 = arena.EntryX, Y1 = arena.EntryY, Y2 = arena.EntryY };
        var members = player.Party?.PartyList.OfType<Player>()
            .Where(p => p != player && p.CurrentMap == map && p.IsInRange(npc.Position, PartyEntryRange) && !arena.Contains(p.Position))
            .ToList() ?? [];
        await player.WarpToAsync(gate).ConfigureAwait(false);
        foreach (var member in members)
        {
            await member.WarpToAsync(gate).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is not Player player || this.Configuration is null)
        {
            return;
        }

        foreach (var state in this.GetWorld(player.GameContext).States.Where(s => s.Definition.MapNumber == map.Definition.Number))
        {
            await this.ShowStateAsync(player, state).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Checks whether a player may enter the arena of a boss now.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="state">The state of the boss.</param>
    /// <param name="requiresBoss">If set, the arena is only open while the boss lives (gates and the entry NPC).</param>
    /// <returns>The result.</returns>
    internal async ValueTask<ArenaEntry> CheckArenaEntryAsync(IGameContext gameContext, FieldBossState state, bool requiresBoss)
    {
        if (state.Definition.Arena is not { } arena)
        {
            return ArenaEntry.Allowed;
        }

        var alive = state.Status is FieldBossStatus.Alive or FieldBossStatus.Engaged;
        if (requiresBoss && !alive)
        {
            return ArenaEntry.NotPresent;
        }

        if (arena.ClosesWhenEngaged && state.Status == FieldBossStatus.Engaged)
        {
            return ArenaEntry.Closed;
        }

        if (arena.MaximumPlayers > 0 && (await GetPlayersInArenaAsync(gameContext, state.Definition).ConfigureAwait(false)).Count >= arena.MaximumPlayers)
        {
            return ArenaEntry.Full;
        }

        return ArenaEntry.Allowed;
    }

    private static async ValueTask ShowEntryRefusalAsync(Player player, FieldBossState state, ArenaEntry entry)
    {
        var bossName = player.GameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == state.Definition.BossNumber)?.Designation.GetTranslation(player.Culture) ?? state.Definition.Name;
        var key = entry switch
        {
            ArenaEntry.Full => nameof(PlayerMessage.FieldBossArenaFull),
            ArenaEntry.Closed => nameof(PlayerMessage.FieldBossArenaClosed),
            _ => nameof(PlayerMessage.FieldBossNotPresent),
        };
        await player.ShowLocalizedBlueMessageAsync(key, bossName).ConfigureAwait(false);
    }

    private static ExitGate? GetExitGate(GameMap map, FieldBossArena arena)
    {
        var gates = map.Definition.ExitGates.Where(g => g.IsSpawnGate && !arena.Contains(new Point(g.X1, g.Y1))).ToList();
        return gates.FirstOrDefault(g => map.Terrain.SafezoneMap[g.X1, g.Y1]) ?? gates.FirstOrDefault();
    }

    private static async ValueTask AnnounceAsync(IGameContext gameContext, FieldBossState state, string messageKey, string? killerName)
    {
        var monster = gameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == state.Definition.BossNumber);
        var mapName = gameContext.Configuration.Maps.FirstOrDefault(m => m.Number == state.Definition.MapNumber)?.Name;
        await gameContext.ForEachPlayerAsync(player =>
        {
            var bossName = monster?.Designation.GetTranslation(player.Culture) ?? state.Definition.Name;
            var map = mapName?.GetTranslation(player.Culture) ?? string.Empty;
            return killerName is null
                ? player.ShowLocalizedGoldenMessageAsync(messageKey, bossName, map).AsTask()
                : player.ShowLocalizedGoldenMessageAsync(messageKey, killerName, bossName, map).AsTask();
        }).ConfigureAwait(false);
    }

    private static async ValueTask DespawnAsync(FieldBossState state)
    {
        var monsters = state.Monsters.Keys.ToList();
        state.Monsters.Clear();
        state.StageMonster = null;
        foreach (var monster in monsters.Where(m => !m.IsDisposed))
        {
            await monster.CurrentMap.RemoveAsync(monster).ConfigureAwait(false);
            monster.Dispose();
        }
    }

    private static async ValueTask<List<Player>> GetPlayersInArenaAsync(IGameContext gameContext, FieldBossDefinition boss)
    {
        if (boss.Arena is not { } arena
            || await gameContext.GetMapAsync((ushort)boss.MapNumber, false).ConfigureAwait(false) is not { } map)
        {
            return [];
        }

        return map.GetPlayers().Where(p => arena.Contains(p.Position)).ToList();
    }

    private World GetWorld(IGameContext gameContext)
        => this._worlds.GetValue(gameContext, _ => new World(this.Configuration, this.UtcNow(), this.GetRespawnDelay));

    private (IGameContext GameContext, FieldBossState State)? FindByMonster(Monster monster)
    {
        foreach (var (gameContext, world) in this._worlds)
        {
            if (world.States.FirstOrDefault(s => s.Monsters.ContainsKey(monster)) is { } state)
            {
                return (gameContext, state);
            }
        }

        return null;
    }

    private FieldBossState? FindArenaState(IGameContext gameContext, short mapNumber, Point point)
    {
        if (this.Configuration is null)
        {
            return null;
        }

        return this.GetWorld(gameContext).States.FirstOrDefault(s =>
            s.Definition.IsActive && s.Definition.MapNumber == mapNumber && s.Definition.Arena is { } arena && arena.Contains(point));
    }

    private FieldBossState? FindEnteredArena(IGameContext gameContext, short mapNumber, ReadOnlySpan<WalkingStep> steps)
    {
        foreach (var step in steps)
        {
            if (this.FindArenaState(gameContext, mapNumber, step.To) is { } state && !state.Definition.Arena!.Contains(step.From))
            {
                return state;
            }
        }

        return null;
    }

    private TimeSpan GetRespawnDelay()
    {
        var configuration = this.Configuration ?? new FieldBossConfiguration();
        var randomMinutes = (int)Math.Max(0, configuration.RespawnRandom.TotalMinutes);
        return configuration.RespawnMinimum + TimeSpan.FromMinutes(Rand.NextInt(0, randomMinutes + 1));
    }

    private async ValueTask<bool> SpawnCoreAsync(IGameContext gameContext, FieldBossState state)
    {
        if (state.Definition.Stages.Count == 0
            || await gameContext.GetMapAsync((ushort)state.Definition.MapNumber).ConfigureAwait(false) is null)
        {
            return false;
        }

        await DespawnAsync(state).ConfigureAwait(false);
        state.KillerName = null;
        state.Status = FieldBossStatus.Alive;
        state.NextChangeAt = DateTime.MaxValue;
        if (!await this.SpawnStageAsync(gameContext, state, 0).ConfigureAwait(false))
        {
            state.Status = FieldBossStatus.Waiting;
            state.NextChangeAt = this.UtcNow() + this.GetRespawnDelay();
            return false;
        }

        if (this.Configuration?.Announce is true)
        {
            await AnnounceAsync(gameContext, state, nameof(PlayerMessage.FieldBossAppeared), null).ConfigureAwait(false);
        }

        await this.ShowStateAsync(gameContext, state).ConfigureAwait(false);
        return true;
    }

    private async ValueTask DefeatCoreAsync(IGameContext gameContext, FieldBossState state, string killerName)
    {
        var configuration = this.Configuration ?? new FieldBossConfiguration();
        await DespawnAsync(state).ConfigureAwait(false);
        var now = this.UtcNow();
        state.KillerName = killerName;
        state.Status = FieldBossStatus.Defeated;
        state.RespawnAt = now + this.GetRespawnDelay();
        state.NextChangeAt = state.Definition.Arena is null ? now : now + configuration.ExitDelay;
        if (configuration.Announce)
        {
            await AnnounceAsync(gameContext, state, nameof(PlayerMessage.FieldBossDefeated), killerName).ConfigureAwait(false);
        }

        var minutes = (int)Math.Ceiling(configuration.ExitDelay.TotalMinutes);
        foreach (var player in await GetPlayersInArenaAsync(gameContext, state.Definition).ConfigureAwait(false))
        {
            await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.FieldBossArenaExit), minutes).ConfigureAwait(false);
        }

        await this.ShowStateAsync(gameContext, state).ConfigureAwait(false);
    }

    private async ValueTask FailCoreAsync(IGameContext gameContext, FieldBossState state)
    {
        var configuration = this.Configuration ?? new FieldBossConfiguration();
        await DespawnAsync(state).ConfigureAwait(false);
        await this.MoveOutAsync(gameContext, state.Definition).ConfigureAwait(false);
        state.Status = FieldBossStatus.Waiting;
        state.NextChangeAt = this.UtcNow() + configuration.FailedRespawn;
        if (configuration.Announce)
        {
            await AnnounceAsync(gameContext, state, nameof(PlayerMessage.FieldBossBattleLost), null).ConfigureAwait(false);
        }

        await this.ShowStateAsync(gameContext, state).ConfigureAwait(false);
    }

    private async ValueTask MoveOutAsync(IGameContext gameContext, FieldBossDefinition boss)
    {
        if (boss.Arena is not { } arena
            || await gameContext.GetMapAsync((ushort)boss.MapNumber, false).ConfigureAwait(false) is not { } map
            || GetExitGate(map, arena) is not { } exit)
        {
            return;
        }

        foreach (var player in await GetPlayersInArenaAsync(gameContext, boss).ConfigureAwait(false))
        {
            await player.WarpToAsync(exit).ConfigureAwait(false);
        }
    }

    private async ValueTask<bool> SpawnStageAsync(IGameContext gameContext, FieldBossState state, int index)
    {
        var stage = state.Definition.Stages[index];
        state.StageIndex = index;
        state.StartedPhases.Clear();
        state.ShownHealthBucket = 10;
        var monster = await this.SpawnMonsterAsync(gameContext, state, stage.MonsterNumber, (byte)Math.Max(0, stage.X - StageSpawnRadius), (byte)Math.Max(0, stage.Y - StageSpawnRadius), (byte)Math.Min(255, stage.X + StageSpawnRadius), (byte)Math.Min(255, stage.Y + StageSpawnRadius)).ConfigureAwait(false);
        state.StageMonster = monster;
        if (monster is null)
        {
            return false;
        }

        await this.SpawnSummonsAsync(gameContext, state, stage.Summons).ConfigureAwait(false);
        return true;
    }

    private async ValueTask SpawnSummonsAsync(IGameContext gameContext, FieldBossState state, IEnumerable<FieldBossSummon> summons)
    {
        foreach (var summon in summons)
        {
            for (var i = 0; i < summon.Count; i++)
            {
                await this.SpawnMonsterAsync(gameContext, state, summon.MonsterNumber, summon.X1, summon.Y1, summon.X2, summon.Y2).ConfigureAwait(false);
            }
        }
    }

    private async ValueTask<Monster?> SpawnMonsterAsync(IGameContext gameContext, FieldBossState state, short monsterNumber, byte x1, byte y1, byte x2, byte y2)
    {
        var logger = gameContext.LoggerFactory.CreateLogger<FieldBossesFeaturePlugIn>();
        if (gameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == monsterNumber) is not { } definition
            || await gameContext.GetMapAsync((ushort)state.Definition.MapNumber).ConfigureAwait(false) is not { } map)
        {
            logger.LogWarning("Field boss {Boss}: monster {Monster} or map {Map} isn't defined.", state.Definition, monsterNumber, state.Definition.MapNumber);
            return null;
        }

        var area = new MonsterSpawnArea
        {
            GameMap = map.Definition,
            MonsterDefinition = definition,
            SpawnTrigger = SpawnTrigger.OnceAtEventStart,
            Quantity = 1,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
        };

        var monster = new Monster(area, definition, map, gameContext.DropGenerator, new BasicMonsterIntelligence(), gameContext.PlugInManager, gameContext.PathFinderPool);
        try
        {
            monster.Initialize();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Field boss {Boss}: monster {Monster} found no walkable tile in {X1},{Y1}-{X2},{Y2}.", state.Definition, monsterNumber, x1, y1, x2, y2);
            monster.Dispose();
            return null;
        }

        state.Monsters[monster] = true;
        await map.AddAsync(monster).ConfigureAwait(false);
        monster.OnSpawn();
        return monster;
    }

    private async Task OnStageHitAsync(IGameContext gameContext, FieldBossState state, Monster monster)
    {
        try
        {
            await state.Lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (state.StageMonster != monster)
                {
                    return;
                }

                if (state.Status == FieldBossStatus.Alive)
                {
                    state.Status = FieldBossStatus.Engaged;
                    state.NextChangeAt = state.Definition.BattleMinutes > 0 ? this.UtcNow().AddMinutes(state.Definition.BattleMinutes) : DateTime.MaxValue;
                }

                var health = state.HealthPercent;
                foreach (var phase in state.Definition.Stages[state.StageIndex].Phases.Where(p => health <= p.HealthPercent && !state.StartedPhases.Contains(p)).ToList())
                {
                    state.StartedPhases.Add(phase);
                    await this.SpawnSummonsAsync(gameContext, state, phase.Summons).ConfigureAwait(false);
                }

                var bucket = health / 10;
                if (bucket != state.ShownHealthBucket)
                {
                    state.ShownHealthBucket = bucket;
                    await this.ShowStateAsync(gameContext, state).ConfigureAwait(false);
                }
            }
            finally
            {
                state.Lock.Release();
            }
        }
        catch (Exception ex)
        {
            gameContext.LoggerFactory.CreateLogger<FieldBossesFeaturePlugIn>().LogError(ex, "Field boss {Boss}: error after a hit.", state.Definition);
        }
    }

    private async ValueTask ShowStateAsync(IGameContext gameContext, FieldBossState state)
    {
        if (await gameContext.GetMapAsync((ushort)state.Definition.MapNumber, false).ConfigureAwait(false) is not { } map)
        {
            return;
        }

        foreach (var player in map.GetPlayers())
        {
            await this.ShowStateAsync(player, state).ConfigureAwait(false);
        }
    }

    private async ValueTask ShowStateAsync(Player player, FieldBossState state)
    {
        var seconds = state.NextChangeAt == DateTime.MaxValue ? 0 : (uint)Math.Clamp((state.NextChangeAt - this.UtcNow()).TotalSeconds, 0, uint.MaxValue);
        await player.InvokeViewPlugInAsync<IFieldBossStateViewPlugIn>(p => p.ShowFieldBossStateAsync(state.Definition.MapNumber, state.Definition.BossNumber, state.Status, state.HealthPercent, seconds)).ConfigureAwait(false);
    }

    /// <summary>
    /// The bosses of one game context.
    /// </summary>
    private sealed class World
    {
        public World(FieldBossConfiguration? configuration, DateTime now, Func<TimeSpan> respawnDelay)
        {
            var firstSpawn = configuration?.SpawnOnStart is false ? now + respawnDelay() : now;
            this.States = configuration?.Bosses.Select(b => new FieldBossState(b, firstSpawn)).ToList() ?? [];
        }

        public IReadOnlyList<FieldBossState> States { get; }
    }
}
