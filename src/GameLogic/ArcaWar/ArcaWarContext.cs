// <copyright file="ArcaWarContext.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// The Arca War of a game context: the stages by the schedule, the registered guilds, the obelisks with their auras, the
/// monster groups, the points, the rewards and the holdings which outlast the war. It runs on the maps of the obelisks and
/// spawns themselves (Acheron and Deventer of the Arca War, maps 92 and 96), which only the war uses.
/// </summary>
public sealed class ArcaWarContext : IEventStateProvider
{
    private const int ElementCount = 5;

    private readonly IGameContext _gameContext;

    private readonly Func<DateTime> _utcNow;

    private readonly object _sync = new();

    private readonly List<ArcaGuild> _guilds = [];

    private readonly Dictionary<Guid, int> _points = [];

    private readonly HashSet<(Guid Killer, Guid Killed)> _trophyKills = [];

    private readonly HashSet<Guid> _penalized = [];

    private readonly Dictionary<Monster, int> _monsters = [];

    private List<ArcaObelisk> _obelisks = [];

    private DateTime[] _groupRegenerationsUtc = [];

    private DateTime? _nextStartLocal;

    private ArcaWarState? _state;

    private ArcaWarConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArcaWarContext"/> class.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="utcNow">The clock.</param>
    public ArcaWarContext(IGameContext gameContext, ArcaWarConfiguration configuration, Func<DateTime> utcNow)
    {
        this._gameContext = gameContext;
        this._configuration = configuration;
        this._utcNow = utcNow;
    }

    /// <summary>
    /// Gets or sets the configuration; a change of the schedule applies from the next closed stage.
    /// </summary>
    public ArcaWarConfiguration Configuration
    {
        get => this._configuration;
        set
        {
            if (!ReferenceEquals(this._configuration, value))
            {
                this._configuration = value;
                this._nextStartLocal = null;
                this._state = null;
            }
        }
    }

    /// <summary>
    /// Gets the stage.
    /// </summary>
    public ArcaWarStage Stage { get; private set; }

    /// <summary>
    /// Gets the time at which the current stage ends.
    /// </summary>
    public DateTime StageEndsAtUtc { get; private set; }

    /// <inheritdoc />
    public bool IsEventRunning => this.Stage == ArcaWarStage.Playing;

    /// <summary>
    /// Gets the registered guilds.
    /// </summary>
    public IReadOnlyList<ArcaGuild> Guilds
    {
        get
        {
            lock (this._sync)
            {
                return [.. this._guilds];
            }
        }
    }

    /// <summary>
    /// Gets the obelisks of the current war.
    /// </summary>
    public IReadOnlyList<ArcaObelisk> Obelisks => this._obelisks;

    /// <summary>
    /// Gets the living monsters of the monster groups.
    /// </summary>
    public IReadOnlyCollection<Monster> Monsters
    {
        get
        {
            lock (this._sync)
            {
                return [.. this._monsters.Keys];
            }
        }
    }

    private ArcaWarState State => this._state ??= this.StateFile?.Load<ArcaWarState>() ?? new ArcaWarState();

    private WorldStateFile? StateFile => string.IsNullOrWhiteSpace(this._configuration.StateFile) ? null : new WorldStateFile(this._configuration.StateFile);

    /// <inheritdoc />
    public bool IsSpawnWaveActive(byte waveNumber) => false;

    /// <summary>
    /// Gets the player behind an attacker: the player itself or the owner of its summon.
    /// </summary>
    /// <param name="attacker">The attacker.</param>
    /// <returns>The player, if any.</returns>
    public static Player? PlayerOf(IAttacker? attacker) => attacker as Player ?? (attacker as IPlayerSurrogate)?.Owner ?? (attacker as Monster)?.SummonedBy;

    /// <summary>
    /// Advances the stages and, during the war, the obelisks and the monster groups.
    /// </summary>
    /// <returns>A task.</returns>
    public async ValueTask TickAsync()
    {
        var now = this._utcNow();
        if (this.Stage == ArcaWarStage.Closed)
        {
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(now, this._gameContext.ServerTimeZone);
            this._nextStartLocal ??= this.GetNextStart(localNow);
            if (localNow >= this._nextStartLocal)
            {
                await this.EnterStageAsync(ArcaWarStage.MasterRegistration, now).ConfigureAwait(false);
            }
        }
        else if (now >= this.StageEndsAtUtc)
        {
            await this.EnterStageAsync(this.Stage == ArcaWarStage.Closing ? ArcaWarStage.Closed : this.Stage + 1, now).ConfigureAwait(false);
        }

        if (this.Stage == ArcaWarStage.Playing)
        {
            await this.UpdateObelisksAsync(now).ConfigureAwait(false);
            await this.RegenerateMonstersAsync(now).ConfigureAwait(false);
            await this.ShowObelisksAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles a request of a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="kind">The kind of the request.</param>
    /// <returns>The result.</returns>
    public async ValueTask<ArcaWarRequestResult> RequestAsync(Player player, ArcaWarRequestKind kind)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return ArcaWarRequestResult.NotRegistered;
        }

        var status = player.GuildStatus;
        var result = kind switch
        {
            ArcaWarRequestKind.RegisterGuild => await this.RegisterGuildAsync(player, character.Id, status).ConfigureAwait(false),
            ArcaWarRequestKind.RegisterMember => this.RegisterMember(character.Id, status),
            _ => this.Stage != ArcaWarStage.Playing ? ArcaWarRequestResult.NotOpen
                : this.GetGuild(player) is null ? ArcaWarRequestResult.NotRegistered
                : ArcaWarRequestResult.Success,
        };

        if (result == ArcaWarRequestResult.Success && kind == ArcaWarRequestKind.Enter && this.GetGuild(player) is { } guild)
        {
            await this.EnterAsync(player, guild).ConfigureAwait(false);
        }

        await this.ShowStateAsync(player).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Gets the points of a player in the current war.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The points.</returns>
    public int GetPoints(Player player)
    {
        lock (this._sync)
        {
            return player.SelectedCharacter is { } character ? this._points.GetValueOrDefault(character.Id) : 0;
        }
    }

    /// <summary>
    /// Gets the registered guild of a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The guild, or <c>null</c> when the player isn't registered.</returns>
    public ArcaGuild? GetGuild(Player player)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return null;
        }

        lock (this._sync)
        {
            return this._guilds.FirstOrDefault(g => g.Members.Contains(character.Id));
        }
    }

    /// <summary>
    /// Handles a kill during the war: an obelisk changes its holder, a monster or a member of another guild gives points
    /// and the trophy, and a killed member gets the death penalty when it's back.
    /// </summary>
    /// <param name="killed">The killed object.</param>
    /// <param name="killer">The killer.</param>
    /// <returns>A task.</returns>
    public async ValueTask OnKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (this.Stage != ArcaWarStage.Playing)
        {
            return;
        }

        var killerPlayer = PlayerOf(killer);
        var killerGuild = killerPlayer is null ? null : this.GetGuild(killerPlayer);
        switch (killed)
        {
            case ArcaObeliskNpc npc when this._obelisks.Contains(npc.Obelisk):
                await this.OnObeliskDestroyedAsync(npc.Obelisk, killerPlayer, killerGuild).ConfigureAwait(false);
                break;
            case Monster monster when this.RemoveMonster(monster):
                if (killerGuild is not null)
                {
                    await this.AddPointsAsync(killerPlayer!, this._configuration.MonsterPoints, true).ConfigureAwait(false);
                }

                break;
            case Player victim when this.GetGuild(victim) is { } victimGuild:
                lock (this._sync)
                {
                    this._penalized.Add(victim.SelectedCharacter!.Id);
                }

                if (killerGuild is not null && killerGuild != victimGuild)
                {
                    bool firstKill;
                    lock (this._sync)
                    {
                        firstKill = this._trophyKills.Add((killerPlayer!.SelectedCharacter!.Id, victim.SelectedCharacter!.Id));
                    }

                    await this.AddPointsAsync(killerPlayer!, this._configuration.PlayerPoints, firstKill).ConfigureAwait(false);
                }

                break;
        }
    }

    /// <summary>
    /// Handles the state changes of players: a member killed in the war gets the death penalty when it's back, and a
    /// member of a guild holding obelisks gets their buffs when it enters the world.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="previous">The previous state.</param>
    /// <param name="current">The current state.</param>
    /// <returns>A task.</returns>
    public async ValueTask OnPlayerStateChangedAsync(Player player, State previous, State current)
    {
        if (current != PlayerState.EnteredWorld || player.SelectedCharacter is not { } character)
        {
            return;
        }

        bool penalized;
        lock (this._sync)
        {
            penalized = this._penalized.Remove(character.Id);
        }

        if (penalized && this.Stage == ArcaWarStage.Playing)
        {
            await this.ApplyPenaltyAsync(player).ConfigureAwait(false);
        }

        if (previous == PlayerState.CharacterSelection)
        {
            await this.ApplyHoldingBuffsAsync(player).ConfigureAwait(false);
            if (this.Stage != ArcaWarStage.Closed)
            {
                await this.ShowStateAsync(player).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Determines whether the war prevents an attack of a player on another: during the war, on its maps, a member can't
    /// hurt one of its own guild (unless friendly fire is on), and players outside the war can't fight the ones in it.
    /// </summary>
    /// <param name="attacker">The attacker.</param>
    /// <param name="defender">The defender.</param>
    /// <returns><c>true</c>, if the attack is prevented.</returns>
    public bool PreventsAttack(Player attacker, Player defender)
    {
        if (this.Stage != ArcaWarStage.Playing || !(this.IsOnWarMap(attacker) || this.IsOnWarMap(defender)))
        {
            return false;
        }

        var attackerGuild = this.GetGuild(attacker);
        var defenderGuild = this.GetGuild(defender);
        if (attackerGuild is null || defenderGuild is null)
        {
            return attackerGuild != defenderGuild;
        }

        return attackerGuild == defenderGuild && !this._configuration.FriendlyFire;
    }

    /// <summary>
    /// Determines whether the attacks and kills between two players are free of the player killer penalty: members of
    /// different guilds on the maps of the war, while it plays.
    /// </summary>
    /// <param name="attacker">The attacker.</param>
    /// <param name="defender">The defender.</param>
    /// <returns><c>true</c>, if they are.</returns>
    public bool ExemptsFromPenalty(Player attacker, Player defender)
        => this.Stage == ArcaWarStage.Playing
           && this.IsOnWarMap(attacker) && this.IsOnWarMap(defender)
           && this.GetGuild(attacker) is { } attackerGuild && this.GetGuild(defender) is { } defenderGuild
           && attackerGuild != defenderGuild;

    /// <summary>
    /// Gets the factor of the damage of a hit between two members of the war on its maps, while it plays: reduced by
    /// <see cref="ArcaWarConfiguration.SelfTeamDamageReduction"/> within a guild and by
    /// <see cref="ArcaWarConfiguration.OppositeTeamDamageReduction"/> between guilds.
    /// </summary>
    /// <param name="attacker">The attacker.</param>
    /// <param name="defender">The defender.</param>
    /// <returns>The factor, 1 for a hit outside of the war.</returns>
    public double GetDamageFactor(Player attacker, Player defender)
    {
        if (this.Stage != ArcaWarStage.Playing
            || !(this.IsOnWarMap(attacker) || this.IsOnWarMap(defender))
            || this.GetGuild(attacker) is not { } attackerGuild
            || this.GetGuild(defender) is not { } defenderGuild)
        {
            return 1;
        }

        var reduction = attackerGuild == defenderGuild ? this._configuration.SelfTeamDamageReduction : this._configuration.OppositeTeamDamageReduction;
        return Math.Clamp(1 - (reduction / 100.0), 0, 1);
    }

    /// <summary>
    /// Gives the bonus to a member of a guild holding obelisks which entered a map of the bonus, or takes it when it
    /// left one.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="map">The map which the player entered or left.</param>
    /// <param name="entered">If set to <c>true</c>, the player entered the map; otherwise, it left it.</param>
    /// <returns>A task.</returns>
    public async ValueTask UpdateBonusAsync(Player player, GameMap map, bool entered)
    {
        var configuration = this._configuration;
        if (configuration.BonusEffectNumber == 0 || !configuration.BonusMaps.Contains(map.Definition.Number))
        {
            return;
        }

        if (!entered)
        {
            if (player.MagicEffectList.TryGetEffect(configuration.BonusEffectNumber, out var active))
            {
                await active.DisposeAsync().ConfigureAwait(false);
            }

            return;
        }

        if (player.Attributes is null
            || player.MagicEffectList.ContainsEffect(configuration.BonusEffectNumber)
            || this._gameContext.Configuration.MagicEffects.FirstOrDefault(e => e.Number == configuration.BonusEffectNumber) is not { } definition
            || (await this.GetHoldingsAsync(player).ConfigureAwait(false)).MaxBy(h => h.ExpiresUtc) is not { } holding)
        {
            return;
        }

        var effect = new MagicEffect(
            holding.ExpiresUtc - this._utcNow(),
            definition,
            new MagicEffect.ElementWithTarget(new ConstantElement(configuration.BonusExperiencePercent / 100f), Stats.BonusExperienceRate),
            new MagicEffect.ElementWithTarget(new ConstantElement(configuration.BonusDropPercent / 100f), Stats.ItemDropRateBonus));
        await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens the registration at the next tick, if the war is closed.
    /// </summary>
    internal void ForceStart()
    {
        if (this.Stage == ArcaWarStage.Closed)
        {
            this._nextStartLocal = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Opens the registration at the next tick if the war is closed; otherwise ends the current stage at the next tick.
    /// </summary>
    internal void Advance()
    {
        if (this.Stage == ArcaWarStage.Closed)
        {
            this.ForceStart();
        }
        else
        {
            this.StageEndsAtUtc = this._utcNow();
        }
    }

    /// <summary>
    /// Registers a guild without its master asking, at any stage.
    /// </summary>
    /// <param name="guildId">The id of the guild.</param>
    /// <param name="name">The name.</param>
    /// <param name="masterId">The id of the character of the master.</param>
    internal void AddGuild(uint guildId, string name, Guid masterId)
    {
        lock (this._sync)
        {
            var guild = new ArcaGuild(guildId, name, this.NextSpawnGroup());
            guild.Members.Add(masterId);
            this._guilds.Add(guild);
        }
    }

    /// <summary>
    /// Applies the buffs of the obelisks the guild of a player holds.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>A task.</returns>
    internal async ValueTask ApplyHoldingBuffsAsync(Player player)
    {
        if (player.Attributes is null)
        {
            return;
        }

        var now = this._utcNow();
        foreach (var holding in await this.GetHoldingsAsync(player).ConfigureAwait(false))
        {
            if (this._configuration.Buffs.FirstOrDefault(b => b.Element == holding.Element) is not { } buff
                || player.MagicEffectList.ContainsEffect(buff.EffectNumber)
                || this._gameContext.Configuration.MagicEffects.FirstOrDefault(e => e.Number == buff.EffectNumber) is not { } definition)
            {
                continue;
            }

            var (attribute, value) = buff.Kind switch
            {
                ArcaBuffKind.SkillDamage => (Stats.SkillDamageBonus, buff.Value),
                ArcaBuffKind.Defense => (Stats.DefenseBase, buff.Value),
                ArcaBuffKind.CriticalDamageChance => (Stats.CriticalDamageChance, buff.Value / 100f),
                ArcaBuffKind.Damage => (Stats.BaseDamageBonus, buff.Value),
                _ => (Stats.ArmorDamageDecrease, buff.Value / 100f),
            };

            var effect = new MagicEffect(holding.ExpiresUtc - now, definition, new MagicEffect.ElementWithTarget(new ConstantElement(value), attribute));
            await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
        }

        if (player.CurrentMap is { } map)
        {
            await this.UpdateBonusAsync(player, map, true).ConfigureAwait(false);
        }
    }

    private static async ValueTask ForEachAsync(IEnumerable<Player> players, Func<Player, ValueTask> action)
    {
        foreach (var player in players)
        {
            try
            {
                await action(player).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                player.Logger.LogError(ex, "Arca War: couldn't update {player}.", player);
            }
        }
    }

    private static List<(Item Item, byte Count)>? GetFeeItems(Player player, RewardItem? fee)
    {
        if (fee is not { Count: > 0 })
        {
            return [];
        }

        var items = new List<(Item Item, byte Count)>();
        var left = (int)fee.Count;
        foreach (var item in player.Inventory?.Items ?? [])
        {
            if (left == 0)
            {
                break;
            }

            if (item.Definition is { } definition && definition.Group == fee.Group && definition.Number == fee.Number && item.Level == fee.Level)
            {
                var count = Math.Min(left, item.IsStackable() ? (int)item.Durability : 1);
                items.Add((item, (byte)count));
                left -= count;
            }
        }

        return left == 0 ? items : null;
    }

    private static async ValueTask PayAsync(Player player, List<(Item Item, byte Count)> items)
    {
        foreach (var (item, count) in items)
        {
            if (item.IsStackable() && item.Durability > count)
            {
                item.Durability -= count;
                await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(item, true)).ConfigureAwait(false);
            }
            else
            {
                await player.DestroyInventoryItemAsync(item).ConfigureAwait(false);
            }
        }
    }

    private async ValueTask<List<ArcaHolding>> GetHoldingsAsync(Player player)
    {
        if (player.GuildStatus is not { } status)
        {
            return [];
        }

        var now = this._utcNow();
        List<ArcaHolding> holdings;
        lock (this._sync)
        {
            holdings = [.. this.State.Holdings.Where(h => h.ExpiresUtc > now)];
        }

        if (holdings.Count == 0)
        {
            return holdings;
        }

        var persistentId = await this.GetPersistentGuildIdAsync(status.GuildId).ConfigureAwait(false);
        return [.. holdings.Where(h => h.PersistentGuildId is { } id && persistentId is { } current ? id == current : h.GuildId == status.GuildId)];
    }

    private DateTime GetNextStart(DateTime localNow)
    {
        var times = this._configuration.StartTimes.OrderBy(t => t).ToList();
        for (var day = 0; day <= 7; day++)
        {
            var date = localNow.Date.AddDays(day);
            if (!this._configuration.Days.Contains(date.DayOfWeek))
            {
                continue;
            }

            foreach (var time in times)
            {
                var start = date + time.ToTimeSpan();
                if (start > localNow)
                {
                    return start;
                }
            }
        }

        return DateTime.MaxValue;
    }

    private async ValueTask EnterStageAsync(ArcaWarStage stage, DateTime now)
    {
        var configuration = this._configuration;
        var minutes = stage switch
        {
            ArcaWarStage.MasterRegistration => configuration.MasterRegistrationMinutes,
            ArcaWarStage.MemberRegistration => configuration.MemberRegistrationMinutes,
            ArcaWarStage.Waiting => configuration.WaitMinutes,
            ArcaWarStage.Playing => configuration.DurationMinutes,
            ArcaWarStage.Closing => configuration.CloseMinutes,
            _ => 0,
        };

        string? message = null;
        object? argument = minutes;
        switch (stage)
        {
            case ArcaWarStage.MasterRegistration:
                this.Reset();
                message = nameof(PlayerMessage.ArcaWarMasterRegistration);
                break;
            case ArcaWarStage.MemberRegistration:
                message = nameof(PlayerMessage.ArcaWarMemberRegistration);
                break;
            case ArcaWarStage.Waiting:
                int guilds;
                lock (this._sync)
                {
                    this._guilds.RemoveAll(g => g.Members.Count < configuration.MinimumGuildMembers);
                    guilds = this._guilds.Count;
                }

                if (guilds < Math.Max(1, configuration.MinimumGuilds))
                {
                    await this.AnnounceAsync(nameof(PlayerMessage.ArcaWarCancelled), null).ConfigureAwait(false);
                    await this.EnterStageAsync(ArcaWarStage.Closed, now).ConfigureAwait(false);
                    return;
                }

                message = nameof(PlayerMessage.ArcaWarStartsSoon);
                break;
            case ArcaWarStage.Playing:
                message = nameof(PlayerMessage.ArcaWarStarted);
                argument = null;
                break;
            case ArcaWarStage.Closing:
                message = nameof(PlayerMessage.ArcaWarEnded);
                argument = null;
                break;
        }

        this.Stage = stage;
        this.StageEndsAtUtc = now.AddMinutes(minutes);
        switch (stage)
        {
            case ArcaWarStage.Playing:
                await this.StartWarAsync(now).ConfigureAwait(false);
                break;
            case ArcaWarStage.Closing:
                await this.FinishWarAsync(now).ConfigureAwait(false);
                break;
            case ArcaWarStage.Closed:
                await this.CloseAsync().ConfigureAwait(false);
                break;
        }

        if (message is not null)
        {
            await this.AnnounceAsync(message, argument).ConfigureAwait(false);
        }

        var players = await this._gameContext.GetPlayersAsync().ConfigureAwait(false);
        await ForEachAsync(players, this.ShowStateAsync).ConfigureAwait(false);
    }

    private void Reset()
    {
        lock (this._sync)
        {
            this._guilds.Clear();
            this._points.Clear();
            this._trophyKills.Clear();
            this._penalized.Clear();
        }
    }

    private async ValueTask<ArcaWarRequestResult> RegisterGuildAsync(Player player, Guid characterId, GuildMemberStatus? status)
    {
        if (this.Stage != ArcaWarStage.MasterRegistration)
        {
            return ArcaWarRequestResult.NotOpen;
        }

        if (status is null)
        {
            return ArcaWarRequestResult.NoGuild;
        }

        if (status.Position != GuildPosition.GuildMaster)
        {
            return ArcaWarRequestResult.NotGuildMaster;
        }

        var (name, persistentId) = await this.GetGuildInfoAsync(player, status.GuildId).ConfigureAwait(false);
        var fee = GetFeeItems(player, this._configuration.RegistrationFee);
        lock (this._sync)
        {
            if (this._guilds.Any(g => g.Id == status.GuildId))
            {
                return ArcaWarRequestResult.AlreadyRegistered;
            }

            if (this._guilds.Count >= this._configuration.MaximumGuilds)
            {
                return ArcaWarRequestResult.TooManyGuilds;
            }

            if (fee is null)
            {
                return ArcaWarRequestResult.NotEnoughSignsOfLord;
            }

            var guild = new ArcaGuild(status.GuildId, name, this.NextSpawnGroup()) { PersistentId = persistentId };
            guild.Members.Add(characterId);
            this._guilds.Add(guild);
        }

        await PayAsync(player, fee).ConfigureAwait(false);
        return ArcaWarRequestResult.Success;
    }

    private ArcaWarRequestResult RegisterMember(Guid characterId, GuildMemberStatus? status)
    {
        if (this.Stage != ArcaWarStage.MemberRegistration)
        {
            return ArcaWarRequestResult.NotOpen;
        }

        if (status is null)
        {
            return ArcaWarRequestResult.NoGuild;
        }

        lock (this._sync)
        {
            if (this._guilds.FirstOrDefault(g => g.Id == status.GuildId) is not { } guild)
            {
                return ArcaWarRequestResult.GuildNotRegistered;
            }

            if (guild.Members.Contains(characterId))
            {
                return ArcaWarRequestResult.AlreadyRegistered;
            }

            if (guild.Members.Count >= this._configuration.MaximumGuildMembers)
            {
                return ArcaWarRequestResult.TooManyMembers;
            }

            guild.Members.Add(characterId);
            return ArcaWarRequestResult.Success;
        }
    }

    private int NextSpawnGroup() => this._guilds.Count % Math.Max(1, this._configuration.SpawnGroups.Count);

    private async ValueTask<(string Name, Guid? PersistentId)> GetGuildInfoAsync(Player player, uint guildId)
    {
        if (this._gameContext is not IGameServerContext serverContext)
        {
            return (string.Empty, null);
        }

        try
        {
            var guild = await serverContext.GuildServer.GetGuildAsync(guildId).ConfigureAwait(false);
            var persistentId = await serverContext.GuildServer.GetPersistentGuildIdAsync(guildId).ConfigureAwait(false);
            return (guild?.Name ?? string.Empty, persistentId);
        }
        catch (Exception ex)
        {
            player.Logger.LogWarning(ex, "Arca War: couldn't get the guild {guildId}.", guildId);
            return (string.Empty, null);
        }
    }

    private async ValueTask<Guid?> GetPersistentGuildIdAsync(uint guildId)
    {
        if (this._gameContext is not IGameServerContext serverContext)
        {
            return null;
        }

        try
        {
            return await serverContext.GuildServer.GetPersistentGuildIdAsync(guildId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this._gameContext.LoggerFactory.CreateLogger<ArcaWarContext>().LogWarning(ex, "Arca War: couldn't get the guild {guildId}.", guildId);
            return null;
        }
    }

    private bool IsWarMap(short mapNumber)
        => this._configuration.Obelisks.Any(o => o.MapNumber == mapNumber)
           || this._configuration.SpawnGroups.Any(g => g.MapNumber == mapNumber)
           || this._configuration.MonsterGroups.Any(g => g.MapNumber == mapNumber);

    private bool IsOnWarMap(Player player) => player.CurrentMap is { } map && this.IsWarMap(map.Definition.Number);

    private async ValueTask StartWarAsync(DateTime now)
    {
        var elements = Enumerable.Range(1, ElementCount).Select(e => (ArcaElement)e).OrderBy(_ => Rand.NextInt(0, 1000)).ToList();
        this._obelisks = [.. this._configuration.Obelisks.Select((o, i) => new ArcaObelisk(o, o.Element != ArcaElement.None ? o.Element : elements[i % elements.Count]))];
        foreach (var obelisk in this._obelisks)
        {
            await this.SpawnObeliskAsync(obelisk).ConfigureAwait(false);
        }

        this._groupRegenerationsUtc = new DateTime[this._configuration.MonsterGroups.Count];
        await this.RegenerateMonstersAsync(now).ConfigureAwait(false);

        foreach (var guild in this.Guilds)
        {
            var members = await this.GetOnlineAsync(guild.Members).ConfigureAwait(false);
            await ForEachAsync(members, player => this.EnterAsync(player, guild)).ConfigureAwait(false);
        }
    }

    private async ValueTask<List<Player>> GetOnlineAsync(IReadOnlyCollection<Guid> characterIds)
    {
        var players = await this._gameContext.GetPlayersAsync().ConfigureAwait(false);
        return [.. players.Where(p => p.SelectedCharacter is { } c && characterIds.Contains(c.Id))];
    }

    private async ValueTask EnterAsync(Player player, ArcaGuild guild)
    {
        var groups = this._configuration.SpawnGroups;
        if (groups.Count == 0
            || groups[guild.SpawnGroup % groups.Count] is not { Areas.Count: > 0 } group
            || this._gameContext.Configuration.Maps.FirstOrDefault(m => m.Number == group.MapNumber && m.Discriminator == 0) is not { } map)
        {
            return;
        }

        var (x1, y1, x2, y2) = group.Areas[Rand.NextInt(0, group.Areas.Count)].Normalized();
        await player.WarpToAsync(new ExitGate { Map = map, X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 }).ConfigureAwait(false);
    }

    private async ValueTask SpawnObeliskAsync(ArcaObelisk obelisk)
    {
        var definition = obelisk.Definition;
        if (await this._gameContext.GetMapAsync((ushort)definition.MapNumber).ConfigureAwait(false) is not { } map
            || this._gameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == definition.MonsterNumber) is not { } monster)
        {
            return;
        }

        var area = new MonsterSpawnArea
        {
            GameMap = map.Definition,
            MonsterDefinition = monster,
            SpawnTrigger = SpawnTrigger.OnceAtEventStart,
            Quantity = 1,
            X1 = definition.X,
            X2 = definition.X,
            Y1 = definition.Y,
            Y2 = definition.Y,
            MaximumHealthOverride = obelisk.OwnerGuildId == 0 ? definition.Health : definition.OccupiedHealth,
        };

        var npc = new ArcaObeliskNpc(area, monster, map, obelisk, this._gameContext.DropGenerator, this._gameContext.PlugInManager);
        npc.Initialize();
        await map.AddAsync(npc).ConfigureAwait(false);
        npc.OnSpawn();
        obelisk.Npc = npc;
        obelisk.RespawnAtUtc = null;
    }

    private async ValueTask UpdateObelisksAsync(DateTime now)
    {
        foreach (var obelisk in this._obelisks)
        {
            if (obelisk.Npc is not { IsAlive: true } npc)
            {
                if (obelisk.RespawnAtUtc is { } respawnAt && now >= respawnAt)
                {
                    await this.SpawnObeliskAsync(obelisk).ConfigureAwait(false);
                }

                continue;
            }

            this.UpdateAuras(obelisk, npc.CurrentMap, now);
        }
    }

    private void UpdateAuras(ArcaObelisk obelisk, GameMap map, DateTime now)
    {
        var auras = obelisk.Definition.Auras;
        obelisk.AuraMasks.Clear();
        for (var i = 0; i < auras.Count; i++)
        {
            foreach (var player in map.GetAttackablesInRange(new Point(auras[i].X, auras[i].Y), this._configuration.AuraRange).OfType<Player>())
            {
                if (player.IsAlive && this.GetGuild(player) is { } guild)
                {
                    obelisk.AuraMasks[guild.Id] = (byte)(obelisk.AuraMasks.GetValueOrDefault(guild.Id) | (1 << i));
                }
            }
        }

        var all = (byte)((1 << auras.Count) - 1);
        var holders = obelisk.AuraMasks.Where(m => m.Value == all && auras.Count > 0 && m.Key != obelisk.OwnerGuildId).Select(m => m.Key).ToList();
        if (holders.Count == 0)
        {
            obelisk.HoldingGuildId = 0;
            obelisk.OpenForGuildId = 0;
        }
        else if (!holders.Contains(obelisk.HoldingGuildId))
        {
            obelisk.HoldingGuildId = holders[0];
            obelisk.HoldingSinceUtc = now;
            obelisk.OpenForGuildId = 0;
        }
        else if (now - obelisk.HoldingSinceUtc >= TimeSpan.FromSeconds(this._configuration.CaptureSeconds))
        {
            obelisk.OpenForGuildId = obelisk.HoldingGuildId;
        }
    }

    private async ValueTask OnObeliskDestroyedAsync(ArcaObelisk obelisk, Player? killer, ArcaGuild? killerGuild)
    {
        obelisk.Npc = null;
        obelisk.RespawnAtUtc = this._utcNow().AddSeconds(obelisk.Definition.RegenSeconds);
        obelisk.HoldingGuildId = 0;
        if (killerGuild is not null && obelisk.OpenForGuildId == killerGuild.Id)
        {
            obelisk.OwnerGuildId = killerGuild.Id;
            await this.AddPointsAsync(killer!, this._configuration.ObeliskPoints, false).ConfigureAwait(false);
            await this.AnnounceAsync(nameof(PlayerMessage.ArcaWarObeliskTaken), killerGuild.Name).ConfigureAwait(false);
        }

        obelisk.OpenForGuildId = 0;
    }

    private bool RemoveMonster(Monster monster)
    {
        lock (this._sync)
        {
            return this._monsters.Remove(monster);
        }
    }

    private async ValueTask AddPointsAsync(Player player, int points, bool withTrophy)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return;
        }

        lock (this._sync)
        {
            this._points[character.Id] = this._points.GetValueOrDefault(character.Id) + points;
        }

        if (withTrophy && this._configuration.Trophy is { } trophy)
        {
            await RewardDelivery.DeliverAsync(player, trophy, true).ConfigureAwait(false);
        }

        await this.ShowStateAsync(player).ConfigureAwait(false);
    }

    private async ValueTask RegenerateMonstersAsync(DateTime now)
    {
        var groups = this._configuration.MonsterGroups;
        for (var i = 0; i < groups.Count && i < this._groupRegenerationsUtc.Length; i++)
        {
            if (now < this._groupRegenerationsUtc[i])
            {
                continue;
            }

            var group = groups[i];
            this._groupRegenerationsUtc[i] = now.AddMinutes(Math.Max(1, group.RegenMinutes));
            if (group.Monsters.Count == 0 || group.Spots.Count == 0
                || await this._gameContext.GetMapAsync((ushort)group.MapNumber).ConfigureAwait(false) is not { } map)
            {
                continue;
            }

            int living;
            lock (this._sync)
            {
                living = this._monsters.Count(m => m.Value == i);
            }

            for (var n = living; n < group.Count; n++)
            {
                var number = group.Monsters[Rand.NextInt(0, group.Monsters.Count)];
                if (this._gameContext.Configuration.Monsters.FirstOrDefault(m => m.Number == number) is not { } definition)
                {
                    continue;
                }

                var (x1, y1, x2, y2) = group.Spots[Rand.NextInt(0, group.Spots.Count)].Normalized();
                if (await EventMonsterSpawner.SpawnInAreaAsync(map, this._gameContext, this, definition, x1, y1, x2, y2).ConfigureAwait(false) is { } monster)
                {
                    lock (this._sync)
                    {
                        this._monsters[monster] = i;
                    }
                }
            }
        }
    }

    private async ValueTask FinishWarAsync(DateTime now)
    {
        var configuration = this._configuration;
        lock (this._sync)
        {
            var holdings = this.State.Holdings;
            holdings.RemoveAll(h => h.ExpiresUtc <= now);
            foreach (var obelisk in this._obelisks.Where(o => o.OwnerGuildId != 0))
            {
                var guild = this._guilds.FirstOrDefault(g => g.Id == obelisk.OwnerGuildId);
                holdings.RemoveAll(h => h.GuildId == obelisk.OwnerGuildId && h.Element == obelisk.Element);
                holdings.Add(new ArcaHolding { GuildId = obelisk.OwnerGuildId, PersistentGuildId = guild?.PersistentId, Element = obelisk.Element, ExpiresUtc = now.AddDays(configuration.BuffDays) });
            }
        }

        this.SaveState();
        var players = await this._gameContext.GetPlayersAsync().ConfigureAwait(false);
        var owners = this._obelisks.Select(o => o.OwnerGuildId).Where(id => id != 0).ToHashSet();
        await ForEachAsync(players.Where(p => p.GuildStatus is { } s && owners.Contains(s.GuildId)), this.ApplyHoldingBuffsAsync).ConfigureAwait(false);

        var participants = this.Guilds.SelectMany(g => g.Members).ToHashSet();
        await ForEachAsync(players.Where(p => p.SelectedCharacter is { } c && participants.Contains(c.Id)), this.GiveExperienceAsync).ConfigureAwait(false);

        await this.RemoveSpawnsAsync().ConfigureAwait(false);
    }

    private ValueTask GiveExperienceAsync(Player player)
    {
        var points = this.GetPoints(player);
        var reward = this._configuration.ExperienceRewards.Where(r => points >= r.MinimumPoints).MaxBy(r => r.MinimumPoints);
        return reward is null ? ValueTask.CompletedTask : player.AddEventExperienceAsync(reward.Experience);
    }

    private void SaveState()
    {
        if (this.StateFile is not { } file)
        {
            return;
        }

        try
        {
            lock (this._sync)
            {
                file.Save(this.State);
            }
        }
        catch (Exception ex)
        {
            this._gameContext.LoggerFactory.CreateLogger<ArcaWarContext>().LogWarning(ex, "The Arca War state can't be written to {File}.", file.Path);
        }
    }

    private async ValueTask RemoveSpawnsAsync()
    {
        foreach (var obelisk in this._obelisks)
        {
            if (obelisk.Npc is { } npc)
            {
                await EventMonsterSpawner.RemoveAsync(npc.CurrentMap, npc).ConfigureAwait(false);
                obelisk.Npc = null;
            }

            obelisk.RespawnAtUtc = null;
        }

        List<Monster> monsters;
        lock (this._sync)
        {
            monsters = [.. this._monsters.Keys];
            this._monsters.Clear();
        }

        foreach (var monster in monsters)
        {
            await EventMonsterSpawner.RemoveAsync(monster.CurrentMap, monster).ConfigureAwait(false);
        }
    }

    private async ValueTask CloseAsync()
    {
        await this.RemoveSpawnsAsync().ConfigureAwait(false);
        var players = await this._gameContext.GetPlayersAsync().ConfigureAwait(false);
        await ForEachAsync(players.Where(this.IsOnWarMap), p => p.WarpToSafezoneAsync()).ConfigureAwait(false);
        this.Reset();
        this._obelisks = [];
        this._nextStartLocal = null;
    }

    private async ValueTask ApplyPenaltyAsync(Player player)
    {
        var configuration = this._configuration;
        if (configuration.DeathPenaltyEffectNumber == 0
            || this._gameContext.Configuration.MagicEffects.FirstOrDefault(e => e.Number == configuration.DeathPenaltyEffectNumber) is not { } definition)
        {
            return;
        }

        var slow = new SimpleElement(configuration.DeathPenaltySpeedFactor, AggregateType.Multiplicate);
        var effect = new MagicEffect(TimeSpan.FromSeconds(configuration.DeathPenaltySeconds), definition, new MagicEffect.ElementWithTarget(slow, Stats.MovementSpeedFactor));
        await player.MagicEffectList.AddEffectAsync(effect).ConfigureAwait(false);
    }

    private async ValueTask AnnounceAsync(string messageKey, object? argument)
    {
        var players = await this._gameContext.GetPlayersAsync().ConfigureAwait(false);
        await ForEachAsync(players, p => p.ShowLocalizedGoldenMessageAsync(messageKey, argument)).ConfigureAwait(false);
    }

    private ValueTask ShowStateAsync(Player player)
    {
        var secondsLeft = this.Stage == ArcaWarStage.Closed ? 0 : (uint)Math.Max(0, (this.StageEndsAtUtc - this._utcNow()).TotalSeconds);
        var registered = this.GetGuild(player) is not null;
        var points = (uint)this.GetPoints(player);
        var guilds = (byte)Math.Min(this.Guilds.Count, byte.MaxValue);
        var stage = this.Stage;
        return player.InvokeViewPlugInAsync<IArcaWarViewPlugIn>(p => p.ShowStateAsync(stage, secondsLeft, guilds, registered, points));
    }

    private async ValueTask ShowObelisksAsync()
    {
        var players = await this._gameContext.GetPlayersAsync().ConfigureAwait(false);
        var names = this.Guilds.ToDictionary(g => g.Id, g => g.Name);
        await ForEachAsync(players.Where(this.IsOnWarMap), player =>
        {
            var guildId = this.GetGuild(player)?.Id ?? 0;
            var views = this._obelisks.Select(o => new ArcaObeliskView(
                o.Definition.Index,
                o.Element,
                o.Definition.MapNumber,
                o.Definition.X,
                o.Definition.Y,
                o.HealthPercent,
                guildId == 0 ? (byte)0 : o.AuraMasks.GetValueOrDefault(guildId),
                o.IsOpenFor(guildId),
                o.OwnerGuildId,
                names.GetValueOrDefault(o.OwnerGuildId) ?? string.Empty)).ToList();
            return player.InvokeViewPlugInAsync<IArcaWarViewPlugIn>(p => p.ShowObelisksAsync(views));
        }).ConfigureAwait(false);
    }
}
