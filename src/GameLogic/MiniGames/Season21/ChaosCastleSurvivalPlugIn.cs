// <copyright file="ChaosCastleSurvivalPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The tournament of the Chaos Castle Survival of the Fittest: a Chaos Castle on map 97 whose games are the try-outs, the
/// semi-finals or the final, by the day of the month. The points of the try-outs add up, the best of them may play the
/// semi-finals, the best of those the final, and its winner is the champion of the month. The state is kept in
/// <see cref="ChaosCastleSurvivalConfiguration.StateFile"/>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.ChaosCastleSurvivalPlugIn_Name), Description = nameof(PlugInResources.ChaosCastleSurvivalPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8D41C2A7-5E39-4B60-A1F8-3C72E9B04D15")]
public class ChaosCastleSurvivalPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<ChaosCastleSurvivalConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault
{
    private ChaosCastleSurvivalConfiguration? _configuration;

    private CharacterStateStore<ChaosCastleSurvivalEntry> _store = CharacterStateStore<ChaosCastleSurvivalEntry>.Get(null);

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public ChaosCastleSurvivalConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._store = CharacterStateStore<ChaosCastleSurvivalEntry>.Get(value?.StateFile);
        }
    }

    /// <summary>
    /// Gets or sets the clock; tests replace it.
    /// </summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    private ChaosCastleSurvivalConfiguration Settings => this._configuration ??= new ChaosCastleSurvivalConfiguration();

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static ChaosCastleSurvivalPlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<ChaosCastleSurvivalPlugIn>();

    /// <inheritdoc />
    public object CreateDefaultConfig() => new ChaosCastleSurvivalConfiguration();

    /// <summary>
    /// Gets the current time of the server of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The local time.</returns>
    public DateTime GetLocalNow(IGameContext gameContext) => TimeZoneInfo.ConvertTimeFromUtc(this.UtcNow(), gameContext.ServerTimeZone);

    /// <summary>
    /// Determines whether a player may play a stage.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="stage">The stage.</param>
    /// <returns><c>true</c>, if the player qualified for it.</returns>
    public bool IsQualified(Player player, ChaosCastleSurvivalStage stage)
        => player.SelectedCharacter is { } character && this.IsQualified(character.Id, stage, this.GetLocalNow(player.GameContext));

    /// <summary>
    /// Records the results of a game and gives the champion its reward.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="stage">The stage of the game.</param>
    /// <param name="results">The results of the players.</param>
    /// <returns>A task.</returns>
    public async ValueTask RecordAsync(IGameContext gameContext, ChaosCastleSurvivalStage stage, IReadOnlyCollection<(Player Player, ChaosCastleSurvivalResult Result)> results)
    {
        var champion = this.Record(stage, results.Select(r => r.Result).ToList(), this.GetLocalNow(gameContext));
        var logger = results.FirstOrDefault().Player?.Logger;
        if (logger is not null)
        {
            this._store.Save(TimeSpan.Zero, logger);
        }

        if (champion is { } id
            && this.Settings.ChampionReward is { } reward
            && results.FirstOrDefault(r => r.Result.CharacterId == id).Player is { } player)
        {
            await RewardDelivery.DeliverAsync(player, reward).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the stage of a day.
    /// </summary>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The stage.</returns>
    internal ChaosCastleSurvivalStage GetStage(DateTime localNow)
    {
        var settings = this.Settings;
        var day = (byte)localNow.Day;
        if (settings.FinalDays.Contains(day))
        {
            return ChaosCastleSurvivalStage.Final;
        }

        if (settings.SemiFinalDays.Contains(day))
        {
            return ChaosCastleSurvivalStage.SemiFinal;
        }

        return settings.TryOutDays.Contains(day) ? ChaosCastleSurvivalStage.TryOut : ChaosCastleSurvivalStage.None;
    }

    /// <summary>
    /// Determines whether a game may start now: on a day of the tournament, and for the final only at its time.
    /// </summary>
    /// <param name="localNow">The time of the server.</param>
    /// <returns><c>true</c>, if a game may start.</returns>
    internal bool MayStart(DateTime localNow)
    {
        var stage = this.GetStage(localNow);
        if (stage != ChaosCastleSurvivalStage.Final)
        {
            return stage != ChaosCastleSurvivalStage.None;
        }

        var time = TimeOnly.FromDateTime(localNow);
        return this.Settings.FinalTimes.Any(t => time.IsBetween(t, t.AddMinutes(1)));
    }

    /// <summary>
    /// Determines whether a character may play a stage: everybody the try-outs, the best of the stage before the others.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="stage">The stage.</param>
    /// <param name="localNow">The time of the server.</param>
    /// <returns><c>true</c>, if the character qualified.</returns>
    internal bool IsQualified(Guid characterId, ChaosCastleSurvivalStage stage, DateTime localNow)
    {
        var (previous, count) = stage switch
        {
            ChaosCastleSurvivalStage.SemiFinal => (ChaosCastleSurvivalStage.TryOut, this.Settings.SemiFinalists),
            ChaosCastleSurvivalStage.Final => (ChaosCastleSurvivalStage.SemiFinal, this.Settings.Finalists),
            _ => (ChaosCastleSurvivalStage.None, 0),
        };
        if (previous == ChaosCastleSurvivalStage.None)
        {
            return stage == ChaosCastleSurvivalStage.TryOut;
        }

        var month = MonthOf(localNow);
        lock (this._store.SyncRoot)
        {
            return this._store.GetAll()
                .Where(pair => pair.Value.Month == month && GetPoints(pair.Value, previous) > 0)
                .OrderByDescending(pair => GetPoints(pair.Value, previous))
                .ThenBy(pair => pair.Key)
                .Take(count)
                .Any(pair => pair.Key == characterId);
        }
    }

    /// <summary>
    /// Gets the points of a character in a stage of the month.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="stage">The stage.</param>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The points.</returns>
    internal int GetPoints(Guid characterId, ChaosCastleSurvivalStage stage, DateTime localNow)
    {
        lock (this._store.SyncRoot)
        {
            return this._store.Find(characterId) is { } entry && entry.Month == MonthOf(localNow) ? GetPoints(entry, stage) : 0;
        }
    }

    /// <summary>
    /// Records the results of a game of a stage.
    /// </summary>
    /// <param name="stage">The stage.</param>
    /// <param name="results">The results.</param>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The champion, when the game was the final and had a winner.</returns>
    internal Guid? Record(ChaosCastleSurvivalStage stage, IReadOnlyCollection<ChaosCastleSurvivalResult> results, DateTime localNow)
    {
        if (stage == ChaosCastleSurvivalStage.None)
        {
            return null;
        }

        var settings = this.Settings;
        var month = MonthOf(localNow);
        lock (this._store.SyncRoot)
        {
            foreach (var result in results)
            {
                var entry = this._store.GetOrCreate(result.CharacterId);
                if (entry.Month != month)
                {
                    entry.Month = month;
                    entry.TryOutPoints = 0;
                    entry.SemiFinalPoints = 0;
                    entry.FinalPoints = 0;
                }

                var points = (result.MonsterKills * settings.MonsterKillPoints)
                             + (result.PlayerKills * settings.PlayerKillPoints)
                             + (result.IsWinner ? settings.WinnerPoints : 0);
                switch (stage)
                {
                    case ChaosCastleSurvivalStage.TryOut:
                        entry.TryOutPoints += points;
                        break;
                    case ChaosCastleSurvivalStage.SemiFinal:
                        entry.SemiFinalPoints += points;
                        break;
                    default:
                        entry.FinalPoints += points;
                        break;
                }
            }
        }

        return stage == ChaosCastleSurvivalStage.Final && results.Any(r => r.IsWinner)
            ? results.First(r => r.IsWinner).CharacterId
            : null;
    }

    private static DateOnly MonthOf(DateTime date) => new(date.Year, date.Month, 1);

    private static int GetPoints(ChaosCastleSurvivalEntry entry, ChaosCastleSurvivalStage stage) => stage switch
    {
        ChaosCastleSurvivalStage.TryOut => entry.TryOutPoints,
        ChaosCastleSurvivalStage.SemiFinal => entry.SemiFinalPoints,
        ChaosCastleSurvivalStage.Final => entry.FinalPoints,
        _ => 0,
    };
}
