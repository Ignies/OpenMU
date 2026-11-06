// <copyright file="HuntingRecordPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HuntingRecord;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The hunting record of Season 21: per character, map and day the time spent, the kills, the experience and the damage on
/// the hunting maps; and the in-game ranking by total level and by the kills and experience of the week.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.HuntingRecordPlugIn_Name), Description = nameof(PlugInResources.HuntingRecordPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E5B83A16-0C7D-4F29-8A4E-61D9B2C7F035")]
public class HuntingRecordPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<HuntingRecordConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IAttackableGotKilledPlugIn, IPlayerGainedExperiencePlugIn, IAttackableGotHitPlugIn, IObjectAddedToMapPlugIn, IObjectRemovedFromMapPlugIn
{
    private readonly ConditionalWeakTable<Player, MapVisit> _visits = new();

    private readonly Dictionary<RankingKind, (DateTime At, List<(Guid Id, RankingEntry Entry)> Entries)> _rankings = [];

    private HuntingRecordConfiguration? _configuration;

    private CharacterStateStore<CharacterHuntingRecord> _store = CharacterStateStore<CharacterHuntingRecord>.Get(null);

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public HuntingRecordConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._store = CharacterStateStore<CharacterHuntingRecord>.Get(value?.StateFile);
            lock (this._rankings)
            {
                this._rankings.Clear();
            }
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
    public static HuntingRecordPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<HuntingRecordPlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new HuntingRecordConfiguration();

    /// <inheritdoc />
    public ValueTask AttackableGotKilledAsync(IAttackable killed, IAttacker? killer)
    {
        if (killer is Player player && killed is Monster monster && this.Counts(player, monster))
        {
            this.Update(player, day => day.Kills++);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask PlayerGainedExperienceAsync(Player player, int experience, IAttackable killedObject, bool isMasterExperience)
    {
        if (experience > 0 && killedObject is Monster monster && this.Counts(player, monster))
        {
            this.Update(player, day => day.Experience += experience);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo)
    {
        if (attacker is Player player && attackable is Monster monster && hitInfo.TotalDamage > 0 && this.Counts(player, monster))
        {
            this.Update(player, day => day.Damage += hitInfo.TotalDamage);
        }
    }

    /// <inheritdoc />
    public ValueTask ObjectAddedToMapAsync(GameMap map, ILocateable addedObject)
    {
        if (addedObject is Player player && this.IsTracked(map.Definition.Number))
        {
            this._visits.AddOrUpdate(player, new MapVisit(map.Definition.Number, this.UtcNow()));
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask ObjectRemovedFromMapAsync(GameMap map, ILocateable removedObject)
    {
        if (removedObject is Player player && this._visits.TryGetValue(player, out var visit) && visit.Map == map.Definition.Number)
        {
            this._visits.Remove(player);
            var seconds = (long)(this.UtcNow() - visit.Since).TotalSeconds;
            if (seconds > 0)
            {
                this.Update(player, visit.Map, day => day.Seconds += seconds);
            }

            this._store.Save(this.Configuration?.SaveInterval ?? TimeSpan.Zero, player.Logger);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the hunting record of a character on a map, newest day first; today includes the time of the current visit.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="mapNumber">The map number.</param>
    /// <returns>The days.</returns>
    public IReadOnlyList<HuntingDay> GetDays(Player player, short mapNumber)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return [];
        }

        List<HuntingDay> days;
        lock (this._store.SyncRoot)
        {
            days = this._store.Find(character.Id)?.Days.Where(d => d.Map == mapNumber)
                .Select(d => new HuntingDay { Map = d.Map, Day = d.Day, Seconds = d.Seconds, Kills = d.Kills, Experience = d.Experience, Damage = d.Damage })
                .OrderByDescending(d => d.Day)
                .ToList() ?? [];
        }

        if (this._visits.TryGetValue(player, out var visit) && visit.Map == mapNumber)
        {
            var today = this.UtcNow().Date;
            if (days.FirstOrDefault(d => d.Day == today) is not { } day)
            {
                day = new HuntingDay { Map = mapNumber, Day = today };
                days.Insert(0, day);
            }

            day.Seconds += (long)(this.UtcNow() - visit.Since).TotalSeconds;
        }

        return days;
    }

    /// <summary>
    /// Gets a page of a ranking and the rank of the player's character.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="page">The page, from 0.</param>
    /// <returns>The characters of the page, the number of pages and the own rank (0 when not ranked).</returns>
    public (IReadOnlyList<RankingEntry> Entries, int PageCount, int OwnRank) GetRankingPage(Player player, RankingKind kind, int page)
    {
        var configuration = this.Configuration ?? new HuntingRecordConfiguration();
        var ranking = this.GetRanking(kind, configuration);
        var pageSize = Math.Max(1, configuration.PageSize);
        var pageCount = Math.Max(1, (ranking.Count + pageSize - 1) / pageSize);
        var ownId = player.SelectedCharacter?.Id;
        var ownIndex = ranking.FindIndex(r => r.Id == ownId);
        var entries = ranking.Skip(Math.Clamp(page, 0, pageCount - 1) * pageSize).Take(pageSize).Select(r => r.Entry).ToList();
        return (entries, pageCount, ownIndex + 1);
    }

    /// <summary>
    /// Shows the hunting record of a map to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="mapNumber">The map number; <c>null</c> for the current map.</param>
    /// <returns>The task.</returns>
    public async ValueTask ShowHuntingRecordAsync(Player player, short? mapNumber)
    {
        var map = mapNumber ?? player.CurrentMap?.Definition.Number ?? 0;
        var days = this.GetDays(player, map);
        await player.InvokeViewPlugInAsync<IHuntingRecordViewPlugIn>(p => p.ShowHuntingRecordAsync(map, days)).ConfigureAwait(false);
    }

    /// <summary>
    /// Shows a page of a ranking to the player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="page">The page, from 0.</param>
    /// <returns>The task.</returns>
    public async ValueTask ShowRankingAsync(Player player, RankingKind kind, int page)
    {
        var (entries, pageCount, ownRank) = this.GetRankingPage(player, kind, page);
        await player.InvokeViewPlugInAsync<IHuntingRecordViewPlugIn>(p => p.ShowRankingPageAsync(kind, Math.Clamp(page, 0, pageCount - 1), pageCount, ownRank, entries)).ConfigureAwait(false);
    }

    private static DateTime WeekStart(DateTime now, DayOfWeek startsOn) => now.Date.AddDays(-(((int)now.DayOfWeek - (int)startsOn + 7) % 7));

    private bool IsTracked(short mapNumber) => this.Configuration?.Maps.Contains(mapNumber) ?? false;

    private bool Counts(Player player, Monster monster)
        => player.CurrentMap?.Definition.Number is { } map
           && this.IsTracked(map)
           && !(this.Configuration?.ExcludedMonsters.Contains(monster.Definition.Number) ?? false);

    private void Update(Player player, Action<HuntingDay> change)
    {
        if (player.CurrentMap?.Definition.Number is { } map)
        {
            this.Update(player, map, change);
        }
    }

    private void Update(Player player, short map, Action<HuntingDay> change)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return;
        }

        var today = this.UtcNow().Date;
        var oldest = today.AddDays(1 - Math.Max(1, this.Configuration?.DaysKept ?? 7));
        lock (this._store.SyncRoot)
        {
            var record = this._store.GetOrCreate(character.Id);
            record.Name = character.Name;
            record.ClassNumber = character.CharacterClass?.Number ?? 0;
            record.TotalLevel = (int)((player.Attributes?[Stats.Level] ?? 0) + (player.Attributes?[Stats.MasterLevel] ?? 0));
            record.Days.RemoveAll(d => d.Day < oldest);
            if (record.Days.FirstOrDefault(d => d.Map == map && d.Day == today) is not { } day)
            {
                day = new HuntingDay { Map = map, Day = today };
                record.Days.Add(day);
            }

            change(day);
        }
    }

    private List<(Guid Id, RankingEntry Entry)> GetRanking(RankingKind kind, HuntingRecordConfiguration configuration)
    {
        var now = this.UtcNow();
        lock (this._rankings)
        {
            if (this._rankings.TryGetValue(kind, out var cached) && now - cached.At < configuration.RankingRefreshInterval)
            {
                return cached.Entries;
            }
        }

        var weekStart = WeekStart(now, configuration.WeekStartsOn);
        List<(Guid Id, string Name, byte Class, long Value)> values;
        lock (this._store.SyncRoot)
        {
            values = this._store.GetAll().Select(pair => (pair.Key, pair.Value.Name, pair.Value.ClassNumber, kind switch
            {
                RankingKind.WeeklyKills => pair.Value.Days.Where(d => d.Day >= weekStart).Sum(d => d.Kills),
                RankingKind.WeeklyExperience => pair.Value.Days.Where(d => d.Day >= weekStart).Sum(d => d.Experience),
                _ => (long)pair.Value.TotalLevel,
            })).ToList();
        }

        var ranking = values.Where(v => v.Value > 0)
            .OrderByDescending(v => v.Value)
            .ThenBy(v => v.Name, StringComparer.Ordinal)
            .Take(configuration.MaximumRanks)
            .Select((v, index) => (v.Id, new RankingEntry(index + 1, v.Name, v.Class, v.Value)))
            .ToList();
        lock (this._rankings)
        {
            this._rankings[kind] = (now, ranking);
        }

        return ranking;
    }

    private sealed record MapVisit(short Map, DateTime Since);
}
