// <copyright file="LabyrinthLeaguePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The scores and weekly leagues of the Labyrinth of Dimensions (S21Emu <c>LabyrinthOfDimension_League.xml</c>): the points
/// of a floor by its first or repeated clear of the day, the first play of the day and a fast clear, a box per floor, groups
/// of five characters of a tier for a week, promotion and demotion when the next week starts and the reward of the tier on
/// the reward day. The state is kept in <see cref="LabyrinthLeagueConfiguration.StateFile"/>; days are UTC.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.LabyrinthLeaguePlugIn_Name), Description = nameof(PlugInResources.LabyrinthLeaguePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("3E7B0D52-6A9C-4F18-B2D4-81C5E0A9F617")]
public class LabyrinthLeaguePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<LabyrinthLeagueConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    private LabyrinthLeagueConfiguration? _configuration;

    private CharacterStateStore<LabyrinthLeagueEntry> _store = CharacterStateStore<LabyrinthLeagueEntry>.Get(null);

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public LabyrinthLeagueConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._store = CharacterStateStore<LabyrinthLeagueEntry>.Get(value?.StateFile);
        }
    }

    /// <summary>
    /// Gets or sets the clock; tests replace it.
    /// </summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    private LabyrinthLeagueConfiguration Settings => this._configuration ??= new LabyrinthLeagueConfiguration();

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static LabyrinthLeaguePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<LabyrinthLeaguePlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new LabyrinthLeagueConfiguration();

    /// <summary>
    /// Gives a character which enters the labyrinth the reward of its tier on the reward day, and shows its standing.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>A task.</returns>
    public async ValueTask EnteredAsync(Player player)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return;
        }

        if (this.ClaimReward(character.Id, this.UtcNow()) is { } reward)
        {
            await RewardDelivery.DeliverAsync(player, reward).ConfigureAwait(false);
        }

        await this.ShowStandingAsync(player).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the points of a cleared floor and records the clear of the day.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="floor">The floor, from 1.</param>
    /// <param name="clearTime">The time the floor took.</param>
    /// <returns>The points.</returns>
    public int ScoreFloorClear(Player player, int floor, TimeSpan clearTime)
        => player.SelectedCharacter is { } character ? this.ScoreFloorClear(character.Id, floor, clearTime, this.UtcNow()) : 0;

    /// <summary>
    /// Adds points of the labyrinth to the week of the character.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="points">The points.</param>
    public void AddPoints(Player player, int points)
    {
        if (player.SelectedCharacter is { } character)
        {
            this.AddPoints(character.Id, points, this.UtcNow());
            this._store.Save(this.Settings.SaveInterval, player.Logger);
        }
    }

    /// <summary>
    /// Shows the standing of the character to the Babylon client.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>A task.</returns>
    public async ValueTask ShowStandingAsync(Player player)
    {
        if (player.SelectedCharacter is not { } character)
        {
            return;
        }

        var now = this.UtcNow();
        var standing = this.GetStanding(character.Id, now);
        var day = this.GetDayKind(DateOnly.FromDateTime(now));
        await player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowLabyrinthLeagueAsync(standing, day)).ConfigureAwait(false);
    }

    /// <summary>
    /// Rolls the box of a cleared floor by its chances.
    /// </summary>
    /// <param name="floor">The floor, from 1.</param>
    /// <returns>The box, or <c>null</c>.</returns>
    public RewardItem? RollFloorBox(int floor)
    {
        var settings = this.Settings;
        if (settings.Floors.ElementAtOrDefault(floor - 1) is not { } score)
        {
            return null;
        }

        var roll = Rand.NextDouble();
        for (var i = 0; i < score.BoxChances.Count && i < settings.FloorBoxes.Count; i++)
        {
            roll -= score.BoxChances[i];
            if (roll < 0)
            {
                return settings.FloorBoxes[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the points of a cleared floor and records the clear of the day.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="floor">The floor, from 1.</param>
    /// <param name="clearTime">The time the floor took.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The points.</returns>
    internal int ScoreFloorClear(Guid characterId, int floor, TimeSpan clearTime, DateTime now)
    {
        var settings = this.Settings;
        if (settings.Floors.ElementAtOrDefault(floor - 1) is not { } score)
        {
            return 0;
        }

        var today = DateOnly.FromDateTime(now);
        lock (this._store.SyncRoot)
        {
            this.Progress(today);
            var entry = this.Touch(characterId, today);
            var isFirst = !entry.ClearedFloors.Contains(floor);
            var points = isFirst ? score.FirstPoints : score.RepeatPoints;
            if (entry.ClearedFloors.Count == 0)
            {
                points += settings.FirstPlayPoints;
            }

            if (isFirst)
            {
                entry.ClearedFloors.Add(floor);
            }

            return points + GetFastFinishPoints(settings, clearTime);
        }
    }

    /// <summary>
    /// Adds points to the week of the character; days which aren't days of a league week count nothing.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="points">The points.</param>
    /// <param name="now">The current time.</param>
    internal void AddPoints(Guid characterId, int points, DateTime now)
    {
        var today = DateOnly.FromDateTime(now);
        lock (this._store.SyncRoot)
        {
            this.Progress(today);
            if (points <= 0 || this.GetWeek(today) is not { } week)
            {
                return;
            }

            var entry = this.Touch(characterId, today);
            if (entry.Week != week)
            {
                entry.Group = this.ChooseGroup(characterId, week, entry.Tier);
                entry.Week = week;
                entry.Points = 0;
                entry.JoinedAt = now;
            }

            entry.Points += points;
        }
    }

    /// <summary>
    /// Gets the standing of a character.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The standing.</returns>
    internal LabyrinthStanding GetStanding(Guid characterId, DateTime now)
    {
        var today = DateOnly.FromDateTime(now);
        lock (this._store.SyncRoot)
        {
            this.Progress(today);
            if (this._store.Find(characterId) is not { } entry)
            {
                return default;
            }

            var tier = entry.Cycle == MonthOf(today) ? entry.Tier : LabyrinthLeagueTier.Bronze;
            var floors = entry.Day == today ? entry.ClearedFloors.Count : 0;
            if (entry.Week is not { } week || week != this.GetWeek(today))
            {
                return new LabyrinthStanding(tier, 0, 0, 0, floors);
            }

            var group = this.GetGroup(week, entry.Tier, entry.Group);
            var rank = group.FindIndex(e => ReferenceEquals(e, entry)) + 1;
            return new LabyrinthStanding(tier, rank, group.Count, entry.Points, floors);
        }
    }

    /// <summary>
    /// Takes the reward of the tier of the character, once a month on the reward day.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The reward, or <c>null</c>.</returns>
    internal RewardItem? ClaimReward(Guid characterId, DateTime now)
    {
        var today = DateOnly.FromDateTime(now);
        if (this.GetDayKind(today) != LabyrinthDayKind.Reward)
        {
            return null;
        }

        lock (this._store.SyncRoot)
        {
            this.Progress(today);
            if (this._store.Find(characterId) is not { } entry
                || entry.Cycle != MonthOf(today)
                || entry.RewardCycle == entry.Cycle
                || this.Settings.TierRewards.FirstOrDefault(r => r.Tier == entry.Tier) is not { } reward)
            {
                return null;
            }

            entry.RewardCycle = entry.Cycle;
            return reward.Item;
        }
    }

    /// <summary>
    /// Gets the kind of a day.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The kind.</returns>
    internal LabyrinthDayKind GetDayKind(DateOnly date)
        => this.Settings.Schedule.FirstOrDefault(d => d.Day == date.Day)?.Kind ?? LabyrinthDayKind.None;

    private static DateOnly MonthOf(DateOnly date) => new(date.Year, date.Month, 1);

    private static int GetFastFinishPoints(LabyrinthLeagueConfiguration settings, TimeSpan clearTime)
    {
        var maxSeconds = (int)settings.FastFinishMaxTime.TotalSeconds;
        var seconds = (int)clearTime.TotalSeconds;
        if (maxSeconds <= 0 || seconds >= maxSeconds)
        {
            return 0;
        }

        // S21Emu: PointDecrease = time * 100 / max; points = FastFinish - (FastFinish * PointDecrease / 100).
        var decrease = seconds * 100 / maxSeconds;
        return settings.FastFinishPoints - (settings.FastFinishPoints * decrease / 100);
    }

    /// <summary>
    /// Gets the first day of the week a day belongs to: the last start or reward day up to it, in its month.
    /// </summary>
    private DateOnly GetBoundary(DateOnly date)
    {
        var day = this.Settings.Schedule
            .Where(d => d.Day <= date.Day && d.Kind is LabyrinthDayKind.Start or LabyrinthDayKind.Reward)
            .Select(d => (int)d.Day)
            .DefaultIfEmpty(1)
            .Max();
        return new DateOnly(date.Year, date.Month, day);
    }

    /// <summary>
    /// Gets the week whose points a day counts, or <c>null</c> when it counts none.
    /// </summary>
    private DateOnly? GetWeek(DateOnly date)
    {
        if (this.GetDayKind(date) is not (LabyrinthDayKind.Start or LabyrinthDayKind.Play))
        {
            return null;
        }

        var boundary = this.GetBoundary(date);
        return this.GetDayKind(boundary) == LabyrinthDayKind.Start ? boundary : null;
    }

    /// <summary>
    /// Gets the entry of a character for a day: a new month starts it in Bronze, a new day clears its floors.
    /// </summary>
    private LabyrinthLeagueEntry Touch(Guid characterId, DateOnly today)
    {
        var entry = this._store.GetOrCreate(characterId);
        var month = MonthOf(today);
        if (entry.Cycle != month)
        {
            entry.Cycle = month;
            entry.Tier = LabyrinthLeagueTier.Bronze;
        }

        if (entry.Day != today)
        {
            entry.Day = today;
            entry.ClearedFloors.Clear();
        }

        return entry;
    }

    /// <summary>
    /// Ranks every group of a finished week: the first ranks go up a tier, the last ones down.
    /// </summary>
    private void Progress(DateOnly today)
    {
        var boundary = this.GetBoundary(today);
        var settings = this.Settings;
        var finished = this._store.GetAll()
            .Select(pair => pair.Value)
            .Where(e => e.Week < boundary)
            .GroupBy(e => (e.Week, e.Tier, e.Group))
            .ToList();
        foreach (var group in finished)
        {
            var ranked = group.OrderByDescending(e => e.Points).ThenBy(e => e.JoinedAt).ToList();
            for (var index = 0; index < ranked.Count; index++)
            {
                var entry = ranked[index];
                var rank = index + 1;
                if (rank <= settings.PromotedRanks && entry.Tier < LabyrinthLeagueTier.Legend)
                {
                    entry.Tier++;
                }
                else if (rank >= settings.DemotedRank && entry.Tier > LabyrinthLeagueTier.Bronze)
                {
                    entry.Tier--;
                }
                else
                {
                    // The middle of the group stays.
                }

                entry.Week = null;
                entry.Group = 0;
                entry.Points = 0;
            }
        }
    }

    private int ChooseGroup(Guid characterId, DateOnly week, LabyrinthLeagueTier tier)
    {
        var sizes = this._store.GetAll()
            .Where(pair => pair.Key != characterId && pair.Value.Week == week && pair.Value.Tier == tier)
            .GroupBy(pair => pair.Value.Group)
            .ToDictionary(g => g.Key, g => g.Count());
        var group = 1;
        while (sizes.GetValueOrDefault(group) >= Math.Max(this.Settings.GroupSize, 1))
        {
            group++;
        }

        return group;
    }

    private List<LabyrinthLeagueEntry> GetGroup(DateOnly week, LabyrinthLeagueTier tier, int group)
        => this._store.GetAll()
            .Select(pair => pair.Value)
            .Where(e => e.Week == week && e.Tier == tier && e.Group == group)
            .OrderByDescending(e => e.Points)
            .ThenBy(e => e.JoinedAt)
            .ToList();
}
