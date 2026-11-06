// <copyright file="SoulConverterPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SoulConverter;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The monster soul converter of Season 21: a converter item of a tier, used by a character of its level, draws soul
/// missions of the tier; kills of their monsters absorb souls, and when all missions are done the player claims a reward
/// of a kind the tier offers. Converters expire when the week starts again.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SoulConverterPlugIn_Name), Description = nameof(PlugInResources.SoulConverterPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5B2D8E41-7C93-4A06-B1F5-2E8A6C4D9F17")]
public class SoulConverterPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<SoulConverterConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IAttackableGotKilledPlugIn, IObjectAddedToMapPlugIn
{
    /// <summary>
    /// The basis of <see cref="SoulMission.AbsorbRate"/>.
    /// </summary>
    private const int AbsorbRateBasis = 1_000_000;

    /// <summary>
    /// The basis of <see cref="SoulRewardDraw.Chance"/>.
    /// </summary>
    private const int ChanceBasis = 10_000;

    private SoulConverterConfiguration? _configuration;

    private CharacterStateStore<CharacterSoulConverter> _store = CharacterStateStore<CharacterSoulConverter>.Get(null);

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public SoulConverterConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._store = CharacterStateStore<CharacterSoulConverter>.Get(value?.StateFile);
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
    public static SoulConverterPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<SoulConverterPlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new SoulConverterConfiguration();

    /// <summary>
    /// Gets the tier of a converter item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The tier, or <c>null</c> when the item is no converter.</returns>
    public SoulConverterTier? FindTier(Item item)
        => this.Configuration?.Tiers.FirstOrDefault(t => t.ItemGroup == item.Definition?.Group && t.ItemNumber == item.Definition.Number);

    /// <summary>
    /// Activates the converter of an item for the character; the caller consumes the item when it is activated.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="item">The converter item.</param>
    /// <returns>The result.</returns>
    public async ValueTask<SoulConverterActivation> ActivateAsync(Player player, Item item)
    {
        if (this.FindTier(item) is not { } tier || player.SelectedCharacter is not { } character || this.Configuration is not { } configuration)
        {
            return SoulConverterActivation.NotAConverter;
        }

        if (TotalLevel(player) < tier.MinimumTotalLevel)
        {
            return SoulConverterActivation.LevelTooLow;
        }

        var now = this.UtcNow();
        var week = this.WeekStart(now);
        lock (this._store.SyncRoot)
        {
            var state = this._store.GetOrCreate(character.Id);
            Refresh(state, now, week);
            if (state.Tier != 0)
            {
                return SoulConverterActivation.AlreadyActive;
            }

            if (state.Activations >= configuration.ActivationsPerWeek)
            {
                return SoulConverterActivation.WeeklyLimitReached;
            }

            state.Activations++;
            state.Tier = tier.Tier;
            state.ExpiresAt = week.AddDays(7);
            state.Missions = tier.Missions.OrderBy(_ => Rand.NextInt(0, int.MaxValue)).Take(Math.Max(1, tier.MissionCount))
                .Select(m => new SoulMissionProgress { MapNumber = m.MapNumber, MonsterNumber = m.MonsterNumber, RequiredSouls = m.RequiredSouls, AbsorbRate = m.AbsorbRate })
                .ToList();
        }

        this._store.Save(configuration.SaveInterval, player.Logger);
        await this.ShowStateAsync(player).ConfigureAwait(false);
        return SoulConverterActivation.Activated;
    }

    /// <summary>
    /// Gets the state of the character: the tier of the active converter (0 none), the activations left this week, when
    /// the converter expires and its missions.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The state.</returns>
    public (byte Tier, int ActivationsLeft, DateTime? ExpiresAt, IReadOnlyList<SoulMissionProgress> Missions) GetState(Player player)
    {
        var perWeek = this.Configuration?.ActivationsPerWeek ?? 0;
        if (player.SelectedCharacter is not { } character)
        {
            return (0, 0, null, []);
        }

        var now = this.UtcNow();
        lock (this._store.SyncRoot)
        {
            if (this._store.Find(character.Id) is not { } state)
            {
                return (0, perWeek, null, []);
            }

            Refresh(state, now, this.WeekStart(now));
            var missions = state.Missions
                .Select(m => new SoulMissionProgress { MapNumber = m.MapNumber, MonsterNumber = m.MonsterNumber, Souls = m.Souls, RequiredSouls = m.RequiredSouls, AbsorbRate = m.AbsorbRate })
                .ToList();
            return (state.Tier, Math.Max(0, perWeek - state.Activations), state.Tier == 0 ? null : state.ExpiresAt, missions);
        }
    }

    /// <summary>
    /// Claims the reward of a kind of the completed converter; the converter is used up.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="kind">The reward kind.</param>
    /// <returns>The result.</returns>
    public async ValueTask<SoulConverterClaimResult> ClaimAsync(Player player, SoulRewardKind kind)
    {
        if (this.Configuration is not { } configuration || player.SelectedCharacter is not { } character)
        {
            return SoulConverterClaimResult.Off;
        }

        SoulReward reward;
        var now = this.UtcNow();
        lock (this._store.SyncRoot)
        {
            if (this._store.Find(character.Id) is not { } state)
            {
                return SoulConverterClaimResult.NoConverter;
            }

            Refresh(state, now, this.WeekStart(now));
            if (state.Tier == 0 || configuration.Tiers.FirstOrDefault(t => t.Tier == state.Tier) is not { } tier)
            {
                return SoulConverterClaimResult.NoConverter;
            }

            if (!state.Missions.TrueForAll(m => m.IsDone))
            {
                return SoulConverterClaimResult.MissionsNotDone;
            }

            if (tier.Rewards.FirstOrDefault(r => r.Kind == kind) is not { } offered)
            {
                return SoulConverterClaimResult.KindNotOffered;
            }

            reward = offered;
            state.Tier = 0;
            state.Missions = [];
            this._store.GetOrCreate(character.Id);
        }

        var delivered = true;
        foreach (var draw in reward.Draws.Where(d => Rolls(d.Chance, ChanceBasis)))
        {
            if (draw.Items.Count > 0)
            {
                delivered &= await RewardDelivery.DeliverAsync(player, draw.Items[Rand.NextInt(0, draw.Items.Count)]).ConfigureAwait(false);
            }
            else if (draw.MaximumRuud > 0)
            {
                await player.AddRuudAsync(Rand.NextInt(draw.MinimumRuud, draw.MaximumRuud + 1)).ConfigureAwait(false);
            }
        }

        this._store.Save(configuration.SaveInterval, player.Logger);
        await this.ShowStateAsync(player).ConfigureAwait(false);
        return delivered ? SoulConverterClaimResult.Rewarded : SoulConverterClaimResult.NotDelivered;
    }

    /// <summary>
    /// Shows the state of the converter to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public async ValueTask ShowStateAsync(Player player)
    {
        var (tier, left, expiresAt, missions) = this.GetState(player);
        await player.InvokeViewPlugInAsync<ISoulConverterViewPlugIn>(p => p.ShowSoulConverterStateAsync(tier, left, expiresAt, missions)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killer is not Player player || killed is not Monster monster || player.CurrentMap?.Definition.Number is not { } map || this.Configuration is not { } configuration)
        {
            return;
        }

        var hunters = configuration.ShareWithParty && player.Party is { } party
            ? party.PartyList.OfType<Player>().Where(p => p.CurrentMap == player.CurrentMap).ToList()
            : [player];
        var number = monster.Definition.Number;
        var now = this.UtcNow();
        var week = this.WeekStart(now);
        bool? absorbed = null;
        var changed = new List<Player>();
        lock (this._store.SyncRoot)
        {
            foreach (var hunter in hunters)
            {
                if (hunter.SelectedCharacter is not { } character || this._store.Find(character.Id) is not { } state)
                {
                    continue;
                }

                Refresh(state, now, week);
                if (state.Missions.Find(m => m.MapNumber == map && m.MonsterNumber == number && !m.IsDone) is not { } mission)
                {
                    continue;
                }

                // One roll a kill; the party members with the mission share it.
                absorbed ??= Rolls(mission.AbsorbRate, AbsorbRateBasis);
                if (absorbed.Value)
                {
                    mission.Souls++;
                    this._store.GetOrCreate(character.Id);
                    changed.Add(hunter);
                }
            }
        }

        if (changed.Count == 0)
        {
            return;
        }

        this._store.Save(configuration.SaveInterval, player.Logger);
        foreach (var hunter in changed)
        {
            await this.ShowStateAsync(hunter).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player && this.GetState(player).Tier != 0)
        {
            await this.ShowStateAsync(player).ConfigureAwait(false);
        }
    }

    private static int TotalLevel(Player player) => (int)((player.Attributes?[Stats.Level] ?? 0) + (player.Attributes?[Stats.MasterLevel] ?? 0));

    private static bool Rolls(int rate, int basis) => rate >= basis || (rate > 0 && Rand.NextInt(0, basis) < rate);

    /// <summary>
    /// Starts the week again and drops an expired converter. The caller holds the lock of the store.
    /// </summary>
    private static void Refresh(CharacterSoulConverter state, DateTime now, DateTime week)
    {
        if (state.Week != week)
        {
            state.Week = week;
            state.Activations = 0;
        }

        if (state.Tier != 0 && now >= state.ExpiresAt)
        {
            state.Tier = 0;
            state.Missions = [];
        }
    }

    private DateTime WeekStart(DateTime now)
        => now.Date.AddDays(-(((int)now.DayOfWeek - (int)(this.Configuration?.WeekStartsOn ?? DayOfWeek.Thursday) + 7) % 7));
}
