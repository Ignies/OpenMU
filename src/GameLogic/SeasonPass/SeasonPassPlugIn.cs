// <copyright file="SeasonPassPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The season pass (MU Pass) of Season 21: during a season of the schedule, missions give pass experience, the pass levels
/// give the rewards of the free, gold and premium pass. Buffs last until the season ends, items go to the gremory case.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SeasonPassPlugIn_Name), Description = nameof(PlugInResources.SeasonPassPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8C5F2A39-6E14-4B7D-9A30-1D8E5C7B2F46")]
public class SeasonPassPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<SeasonPassConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IAttackableGotKilledPlugIn, IPlayerGainedExperiencePlugIn, IObjectAddedToMapPlugIn, IObjectRemovedFromMapPlugIn
{
    /// <summary>
    /// The key offset of the special missions in <see cref="CharacterSeasonPass.Missions"/>.
    /// </summary>
    private const int SpecialKeyOffset = 128;

    private static readonly IReadOnlyDictionary<Guid, AttributeDefinition> Attributes = typeof(Stats)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Select(p => p.GetValue(null))
        .OfType<AttributeDefinition>()
        .GroupBy(a => a.Id)
        .ToDictionary(g => g.Key, g => g.First());

    private readonly ConditionalWeakTable<Player, List<(AttributeDefinition Attribute, IElement Element)>> _buffs = new();

    private readonly ConditionalWeakTable<Player, StrongBox<DateTime>> _visits = new();

    private SeasonPassConfiguration? _configuration;

    private CharacterStateStore<CharacterSeasonPass> _store = CharacterStateStore<CharacterSeasonPass>.Get(null);

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public SeasonPassConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._store = CharacterStateStore<CharacterSeasonPass>.Get(value?.StateFile);
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
    public static SeasonPassPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<SeasonPassPlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new SeasonPassConfiguration();

    /// <summary>
    /// Gets the season which runs at a point in time.
    /// </summary>
    /// <param name="now">The point in time (UTC).</param>
    /// <returns>The number, start and end of the season; the number is 0 when none runs.</returns>
    public (ushort Number, DateTime Start, DateTime End) GetSeason(DateTime now)
    {
        foreach (var season in this.Configuration?.Seasons ?? [])
        {
            if (this.Configuration!.RepeatEveryYear)
            {
                for (var year = now.Year - 1; year <= now.Year; year++)
                {
                    var start = InYear(season.Start, year);
                    var end = InYear(season.End, year);
                    end = end < start ? end.AddYears(1) : end;
                    if (start <= now && now <= end)
                    {
                        return (season.Number, start, end);
                    }
                }
            }
            else if (season.Start <= now && now <= season.End)
            {
                return (season.Number, season.Start, season.End);
            }
        }

        return (0, default, default);
    }

    /// <summary>
    /// Gets what the character sees of its season pass.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The state.</returns>
    public SeasonPassView GetState(Player player)
    {
        var now = this.UtcNow();
        var season = this.GetSeason(now);
        if (season.Number == 0 || player.SelectedCharacter is not { } character)
        {
            return new(0, null, 0, 0, 0, 0, 0, 0, []);
        }

        lock (this._store.SyncRoot)
        {
            var state = this.GetCurrent(character.Id, season.Start, now);
            var missions = (this.Configuration?.Missions ?? [])
                .Select(m => Key(m))
                .Select(key => state.Missions.GetValueOrDefault(key) ?? new SeasonPassMissionState(key, 0, 0))
                .ToList();
            return new(season.Number, season.End, this.LevelOf(state.Experience), state.Experience, state.Passes, state.Claimed[0], state.Claimed[1], state.Claimed[2], missions);
        }
    }

    /// <summary>
    /// Adds pass experience, e.g. of an Exp Pass Ticket.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="experience">The experience.</param>
    /// <returns><c>true</c>, if a season runs and the experience was added.</returns>
    public async ValueTask<bool> AddExperienceAsync(Player player, int experience)
    {
        var added = this.Change(player, state =>
        {
            state.Experience += experience;
            return true;
        });
        if (experience <= 0 || !added)
        {
            return false;
        }

        await this.ShowStateAsync(player).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Unlocks the gold or premium pass of the running season; the premium pass needs the gold pass.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="pass">The pass.</param>
    /// <returns><c>true</c>, if the pass was unlocked.</returns>
    public async ValueTask<bool> UnlockAsync(Player player, SeasonPassKind pass)
    {
        var configuration = this.Configuration;
        var enabled = pass switch
        {
            SeasonPassKind.Gold => configuration?.GoldPassEnabled ?? false,
            SeasonPassKind.Premium => configuration?.PremiumPassEnabled ?? false,
            _ => false,
        };
        var bit = (byte)(1 << (int)pass);
        var unlocked = enabled && this.Change(player, state =>
        {
            if ((state.Passes & bit) != 0 || (pass == SeasonPassKind.Premium && (state.Passes & (1 << (int)SeasonPassKind.Gold)) == 0))
            {
                return false;
            }

            state.Passes |= bit;
            return true;
        });
        if (unlocked)
        {
            await this.ShowStateAsync(player).ConfigureAwait(false);
        }

        return unlocked;
    }

    /// <summary>
    /// Claims the rewards of a reached level of an owned pass, once.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="pass">The pass.</param>
    /// <param name="level">The level, from 1.</param>
    /// <returns>The result.</returns>
    public async ValueTask<SeasonPassClaimResult> ClaimAsync(Player player, SeasonPassKind pass, byte level)
    {
        if (this.Configuration is not { } configuration || pass > SeasonPassKind.Premium)
        {
            return SeasonPassClaimResult.Off;
        }

        var result = SeasonPassClaimResult.Off;
        var rewards = new List<SeasonPassReward>();
        this.Change(player, state =>
        {
            var bit = level is > 0 and <= 32 ? 1u << (level - 1) : 0;
            result = level == 0 || level > this.LevelOf(state.Experience) ? SeasonPassClaimResult.LevelNotReached
                : (state.Passes & (1 << (int)pass)) == 0 ? SeasonPassClaimResult.PassNotOwned
                : (state.Claimed[(int)pass] & bit) != 0 ? SeasonPassClaimResult.AlreadyClaimed
                : configuration.LevelRewards.Find(r => r.Pass == pass && r.Level == level) is not { RewardIds.Count: > 0 } row ? SeasonPassClaimResult.NoReward
                : SeasonPassClaimResult.Claimed;
            if (result != SeasonPassClaimResult.Claimed)
            {
                return false;
            }

            state.Claimed[(int)pass] |= bit;
            rewards.AddRange(configuration.LevelRewards.First(r => r.Pass == pass && r.Level == level).RewardIds
                .Select(id => configuration.Rewards.Find(r => r.Id == id)).OfType<SeasonPassReward>());
            foreach (var buff in rewards.Where(r => r.EffectIndex >= 0))
            {
                state.Buffs[buff.EffectIndex] = Math.Max(level, state.Buffs.GetValueOrDefault(buff.EffectIndex));
            }

            return true;
        });

        if (result != SeasonPassClaimResult.Claimed)
        {
            return result;
        }

        foreach (var item in rewards.Where(r => r.EffectIndex < 0))
        {
            if (!await RewardDelivery.DeliverAsync(player, new RewardItem { Group = item.ItemGroup, Number = item.ItemNumber, Count = item.Count }).ConfigureAwait(false))
            {
                result = SeasonPassClaimResult.NotDelivered;
            }
        }

        await this.RefreshBuffsAsync(player).ConfigureAwait(false);
        await this.ShowStateAsync(player).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Sets the attribute bonuses of the claimed buffs of the running season; a new season takes them away.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public ValueTask RefreshBuffsAsync(Player player)
    {
        var wanted = new List<(AttributeDefinition Attribute, IElement Element)>();
        var now = this.UtcNow();
        var season = this.GetSeason(now);
        if (season.Number != 0 && player.SelectedCharacter is { } character && this.Configuration is { } configuration)
        {
            lock (this._store.SyncRoot)
            {
                foreach (var (effect, level) in this.GetCurrent(character.Id, season.Start, now).Buffs)
                {
                    if (configuration.Buffs.Find(b => b.EffectIndex == effect) is { AttributeId: { } id } buff
                        && Attributes.TryGetValue(id, out var attribute)
                        && buff.Values.Count > 0)
                    {
                        wanted.Add((attribute, new SimpleElement(buff.Values[Math.Clamp(level, 1, buff.Values.Count) - 1], AggregateType.AddRaw)));
                    }
                }
            }
        }

        var applied = this._buffs.GetOrCreateValue(player);
        lock (applied)
        {
            foreach (var (attribute, element) in applied)
            {
                player.Attributes?.RemoveElement(element, attribute);
            }

            foreach (var (attribute, element) in wanted)
            {
                player.Attributes?.AddElement(element, attribute);
            }

            applied.Clear();
            applied.AddRange(wanted);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Shows the state of the season pass to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The task.</returns>
    public async ValueTask ShowStateAsync(Player player)
    {
        var state = this.GetState(player);
        await player.InvokeViewPlugInAsync<ISeasonPassViewPlugIn>(p => p.ShowSeasonPassStateAsync(state)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killer is Player player && killed is NPC.Monster)
        {
            await this.AddProgressAsync(player, SeasonPassMissionKind.MonsterKills, 1).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask PlayerGainedExperienceAsync(Player player, int experience, IAttackable killedObject, bool isMasterExperience)
    {
        if (experience > 0)
        {
            await this.AddProgressAsync(player, SeasonPassMissionKind.Experience, experience).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player)
        {
            this._visits.AddOrUpdate(player, new StrongBox<DateTime>(this.UtcNow()));
            await this.RefreshBuffsAsync(player).ConfigureAwait(false);
            await this.ShowStateAsync(player).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async ValueTask ObjectRemovedFromMapAsync(GameMap map, ILocateable removedObject)
    {
        if (removedObject is Player player && this._visits.TryGetValue(player, out var since))
        {
            this._visits.Remove(player);
            var seconds = (long)(this.UtcNow() - since.Value).TotalSeconds;
            if (seconds > 0)
            {
                await this.AddProgressAsync(player, SeasonPassMissionKind.PlaySeconds, seconds).ConfigureAwait(false);
            }

            this._store.Save(this.Configuration?.SaveInterval ?? TimeSpan.Zero, player.Logger);
        }
    }

    private static int Key(SeasonPassMission mission) => (mission.IsSpecial ? SpecialKeyOffset : 0) + mission.Index;

    private static DateTime InYear(DateTime date, int year)
        => new DateTime(year, date.Month, Math.Min(date.Day, DateTime.DaysInMonth(year, date.Month)), date.Hour, date.Minute, date.Second, DateTimeKind.Utc);

    private int LevelOf(int experience) => this.Configuration?.LevelExperience.Count(needed => experience >= needed) ?? 0;

    /// <summary>
    /// Counts a kind for the missions of the character.
    /// </summary>
    private async ValueTask AddProgressAsync(Player player, SeasonPassMissionKind kind, long amount)
    {
        if (this.Configuration?.Missions.Exists(m => m.Kind == kind) is not true)
        {
            return;
        }

        var missions = this.Configuration.Missions.Where(m => m.Kind == kind && m.Required > 0).ToList();
        var gained = 0;
        this.Change(player, state =>
        {
            foreach (var mission in missions)
            {
                var key = Key(mission);
                var current = state.Missions.GetValueOrDefault(key) ?? new SeasonPassMissionState(key, 0, 0);
                if (current.Completions >= mission.Repeat)
                {
                    continue;
                }

                var (completions, progress) = (current.Completions, current.Progress + amount);
                while (progress >= mission.Required && completions < mission.Repeat)
                {
                    progress -= mission.Required;
                    completions++;
                    gained += mission.Experience;
                }

                state.Missions[key] = new SeasonPassMissionState(key, completions, completions >= mission.Repeat ? 0 : progress);
            }

            state.Experience += gained;
            return true;
        });

        if (gained > 0)
        {
            await this.ShowStateAsync(player).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Changes the state of the character in the running season and saves it.
    /// </summary>
    /// <returns><c>true</c>, if a season runs and the change happened.</returns>
    private bool Change(Player player, Func<CharacterSeasonPass, bool> change)
    {
        var now = this.UtcNow();
        var season = this.GetSeason(now);
        if (season.Number == 0 || player.SelectedCharacter is not { } character)
        {
            return false;
        }

        bool changed;
        lock (this._store.SyncRoot)
        {
            changed = change(this.GetCurrent(character.Id, season.Start, now));
        }

        if (changed)
        {
            this._store.Save(this.Configuration?.SaveInterval ?? TimeSpan.Zero, player.Logger);
        }

        return changed;
    }

    /// <summary>
    /// Gets the state of the season, starting it again for a new season and the daily missions for a new day. The caller
    /// holds the lock of the store.
    /// </summary>
    private CharacterSeasonPass GetCurrent(Guid characterId, DateTime seasonStart, DateTime now)
    {
        var state = this._store.GetOrCreate(characterId);
        if (state.SeasonStart != seasonStart)
        {
            (state.SeasonStart, state.Experience, state.Passes, state.Claimed, state.Day) = (seasonStart, 0, 1, new uint[3], now.Date);
            state.Missions.Clear();
            state.Buffs.Clear();
        }

        if (state.Day != now.Date)
        {
            state.Day = now.Date;
            foreach (var key in state.Missions.Keys.Where(k => k < SpecialKeyOffset).ToList())
            {
                state.Missions.Remove(key);
            }
        }

        return state;
    }
}
