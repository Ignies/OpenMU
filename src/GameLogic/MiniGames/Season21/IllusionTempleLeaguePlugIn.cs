// <copyright file="IllusionTempleLeaguePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Illusion Temple League: a monthly guild tournament on the renewed Illusion Temple (<see cref="IllusionTempleContext"/>
/// of <see cref="MiniGameType.IllusionTempleLeague"/>, maps 98 and 99). Parties of one guild play the eliminations, whose
/// results add up; the best four play the semi-finals, their winners the final; the players of the champion who played the
/// final get its reward from the reward day on, when they enter the world. The state is kept in the files of
/// <see cref="IllusionTempleLeagueConfiguration"/>.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.IllusionTempleLeaguePlugIn_Name), Description = nameof(PlugInResources.IllusionTempleLeaguePlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("4C7E2B91-6A3D-4E58-B1F0-92D8A5C3E716")]
public class IllusionTempleLeaguePlugIn : IFeaturePlugIn, ISupportCustomConfiguration<IllusionTempleLeagueConfiguration>, ISupportDefaultCustomConfiguration, IDisabledByDefault,
    IPlayerDamageFactorPlugIn, IPlayerStateChangedPlugIn
{
    private const int SemiFinalists = 4;

    private IllusionTempleLeagueConfiguration? _configuration;

    private CharacterStateStore<IllusionTempleLeagueGuildEntry> _guilds = CharacterStateStore<IllusionTempleLeagueGuildEntry>.Get(null);

    private CharacterStateStore<IllusionTempleLeaguePlayerEntry> _players = CharacterStateStore<IllusionTempleLeaguePlayerEntry>.Get(null);

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public IllusionTempleLeagueConfiguration? Configuration
    {
        get => this._configuration;
        set
        {
            this._configuration = value;
            this._guilds = CharacterStateStore<IllusionTempleLeagueGuildEntry>.Get(value?.GuildStateFile);
            this._players = CharacterStateStore<IllusionTempleLeaguePlayerEntry>.Get(value?.PlayerStateFile);
        }
    }

    /// <summary>
    /// Gets the settings, the defaults when nothing is configured.
    /// </summary>
    public IllusionTempleLeagueConfiguration Settings => this._configuration ??= new IllusionTempleLeagueConfiguration();

    /// <summary>
    /// Gets or sets the clock; tests replace it.
    /// </summary>
    internal Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

    /// <summary>
    /// Gets the active plugin of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The plugin, or <c>null</c> when the feature is off.</returns>
    public static IllusionTempleLeaguePlugIn? Get(IGameContext gameContext) => gameContext.FeaturePlugIns.GetPlugIn<IllusionTempleLeaguePlugIn>();

    /// <summary>
    /// Gets the key of a guild without a persistent id (a world without a guild server).
    /// </summary>
    /// <param name="guildId">The id of the guild on the guild server.</param>
    /// <returns>The key.</returns>
    public static Guid GetFallbackGuildKey(uint guildId) => new(guildId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <inheritdoc />
    public object CreateDefaultConfig() => new IllusionTempleLeagueConfiguration();

    /// <summary>
    /// Gets the current time of the server of a game context.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <returns>The local time.</returns>
    public DateTime GetLocalNow(IGameContext gameContext) => TimeZoneInfo.ConvertTimeFromUtc(this.UtcNow(), gameContext.ServerTimeZone);

    /// <summary>
    /// Gets the persistent key and the name of the guild of a player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The key and name, or <c>null</c> when the player has no guild.</returns>
    public async ValueTask<(Guid Key, string Name)?> GetGuildAsync(Player player)
    {
        if (player.GuildStatus is not { } status)
        {
            return null;
        }

        if (player.GameContext is IGameServerContext serverContext)
        {
            try
            {
                var key = await serverContext.GuildServer.GetPersistentGuildIdAsync(status.GuildId).ConfigureAwait(false);
                var guild = await serverContext.GuildServer.GetGuildAsync(status.GuildId).ConfigureAwait(false);
                return (key ?? GetFallbackGuildKey(status.GuildId), guild?.Name ?? string.Empty);
            }
            catch (Exception ex)
            {
                player.Logger.LogWarning(ex, "Illusion Temple League: couldn't get the guild {guildId}.", status.GuildId);
            }
        }

        return (GetFallbackGuildKey(status.GuildId), string.Empty);
    }

    /// <summary>
    /// Checks the party of a player: at least <see cref="IllusionTempleLeagueConfiguration.MinimumGuildPartyMembers"/>
    /// members of his guild, him included.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the party may play the league.</returns>
    public bool HasGuildParty(Player player)
    {
        if (player.GuildStatus is not { } status)
        {
            return false;
        }

        var minimum = this.Settings.MinimumGuildPartyMembers;
        if (minimum <= 1)
        {
            return true;
        }

        return player.Party is { } party
               && party.PartyList.OfType<Player>().Count(member => member.GuildStatus?.GuildId == status.GuildId) >= minimum;
    }

    /// <summary>
    /// Records the results of a match and saves the league.
    /// </summary>
    /// <param name="gameContext">The game context.</param>
    /// <param name="stage">The stage of the match.</param>
    /// <param name="results">The results of the two guilds.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The winner, or <c>null</c> on a draw.</returns>
    public Guid? Record(IGameContext gameContext, IllusionTempleLeagueStage stage, IReadOnlyList<IllusionTempleLeagueResult> results, ILogger logger)
    {
        var winner = this.Record(stage, results, this.GetLocalNow(gameContext));
        this._guilds.Save(TimeSpan.Zero, logger);
        this._players.Save(TimeSpan.Zero, logger);
        return winner;
    }

    /// <inheritdoc />
    public void ModifyDamageFactor(Player attacker, Player defender, PlayerDamageFactorArgs args)
    {
        if (this.Settings.DamageReduction > 0
            && attacker.CurrentMiniGame is { Definition.Type: MiniGameType.IllusionTempleLeague } game
            && defender.CurrentMiniGame == game)
        {
            args.Factor *= Math.Max(0, 100 - this.Settings.DamageReduction) / 100.0;
        }
    }

    /// <inheritdoc />
    public async ValueTask PlayerStateChangedAsync(Player player, State previousState, State currentState)
    {
        if (currentState != PlayerState.EnteredWorld
            || player.SelectedCharacter is not { } character
            || this.ClaimReward(character.Id, this.GetLocalNow(player.GameContext)) is not { } reward)
        {
            return;
        }

        this._players.Save(TimeSpan.Zero, player.Logger);
        await RewardDelivery.DeliverAsync(player, reward).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the winner of a match by the order of S21Emu: occupied stones, then kills, then the level sum.
    /// </summary>
    /// <param name="results">The results of the guilds.</param>
    /// <returns>The winner, or <c>null</c> on a draw.</returns>
    internal static IllusionTempleLeagueResult? GetWinner(IReadOnlyList<IllusionTempleLeagueResult> results)
    {
        var ordered = results.OrderByDescending(r => (r.Stones, r.Kills, r.LevelSum)).ToList();
        return ordered.Count switch
        {
            0 => null,
            1 => ordered[0],
            _ => ordered[0].CompareTo(ordered[1]) > 0 ? ordered[0] : null,
        };
    }

    /// <summary>
    /// Gets the stage of a day.
    /// </summary>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The stage.</returns>
    internal IllusionTempleLeagueStage GetStage(DateTime localNow)
    {
        var settings = this.Settings;
        var day = (byte)localNow.Day;
        if (day == settings.SemiFinalDay)
        {
            return IllusionTempleLeagueStage.SemiFinal;
        }

        if (day == settings.FinalDay)
        {
            return IllusionTempleLeagueStage.Final;
        }

        if (day == settings.RewardDay)
        {
            return IllusionTempleLeagueStage.Reward;
        }

        return settings.EliminationDays.Contains(day) ? IllusionTempleLeagueStage.Elimination : IllusionTempleLeagueStage.None;
    }

    /// <summary>
    /// Determines whether the temples may open now: on an elimination day at every time of the timetable, for the
    /// semi-finals and the final only at their times.
    /// </summary>
    /// <param name="localNow">The time of the server.</param>
    /// <returns><c>true</c>, if the temples may open.</returns>
    internal bool MayStart(DateTime localNow)
    {
        var times = this.GetStage(localNow) switch
        {
            IllusionTempleLeagueStage.Elimination => null,
            IllusionTempleLeagueStage.SemiFinal => this.Settings.SemiFinalTimes,
            IllusionTempleLeagueStage.Final => this.Settings.FinalTimes,
            _ => [],
        };
        if (times is null)
        {
            return true;
        }

        var time = TimeOnly.FromDateTime(localNow);
        return times.Any(t => time.IsBetween(t, t.AddMinutes(1)));
    }

    /// <summary>
    /// Checks whether a character of a guild may enter a temple today: the entries of the eliminations, the qualification
    /// of the semi-finals (by the temple of its slot) and of the final (the first temple).
    /// </summary>
    /// <param name="guildKey">The key of the guild.</param>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="gameLevel">The game level of the temple, 1 or 2.</param>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The result of the check.</returns>
    internal IllusionTempleLeagueEntryCheck CheckEntry(Guid guildKey, Guid characterId, int gameLevel, DateTime localNow)
    {
        var settings = this.Settings;
        var month = MonthOf(localNow);
        lock (this._guilds.SyncRoot)
        {
            switch (this.GetStage(localNow))
            {
                case IllusionTempleLeagueStage.Elimination:
                    if (settings.MaximumGuildEntries > 0 && this.FindEntry(guildKey, month)?.Entries >= settings.MaximumGuildEntries)
                    {
                        return IllusionTempleLeagueEntryCheck.GuildEntriesUsed;
                    }

                    lock (this._players.SyncRoot)
                    {
                        return settings.MaximumPlayerEntries > 0
                               && this._players.Find(characterId) is { } player
                               && player.Month == month
                               && player.Entries >= settings.MaximumPlayerEntries
                            ? IllusionTempleLeagueEntryCheck.PlayerEntriesUsed
                            : IllusionTempleLeagueEntryCheck.Allowed;
                    }

                case IllusionTempleLeagueStage.SemiFinal:
                    return this.GetSemiFinalLevel(guildKey, localNow) == gameLevel ? IllusionTempleLeagueEntryCheck.Allowed : IllusionTempleLeagueEntryCheck.NotQualified;
                case IllusionTempleLeagueStage.Final:
                    return gameLevel == 1 && this.FindEntry(guildKey, month)?.IsFinalist is true ? IllusionTempleLeagueEntryCheck.Allowed : IllusionTempleLeagueEntryCheck.NotQualified;
                default:
                    return IllusionTempleLeagueEntryCheck.NotOpen;
            }
        }
    }

    /// <summary>
    /// Records the results of a match of a stage: the eliminations add up, the winner of a semi-final goes to the final,
    /// the winner of the final is the champion; a draw of the final isn't kept, so it can be played again.
    /// </summary>
    /// <param name="stage">The stage.</param>
    /// <param name="results">The results of the guilds.</param>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The winner, or <c>null</c> on a draw.</returns>
    internal Guid? Record(IllusionTempleLeagueStage stage, IReadOnlyList<IllusionTempleLeagueResult> results, DateTime localNow)
    {
        var winner = GetWinner(results);
        var month = MonthOf(localNow);
        lock (this._guilds.SyncRoot)
        {
            foreach (var result in results)
            {
                var entry = this.GetOrCreateEntry(result.GuildKey, month);
                if (!string.IsNullOrEmpty(result.GuildName))
                {
                    entry.Name = result.GuildName;
                }

                switch (stage)
                {
                    case IllusionTempleLeagueStage.Elimination:
                        entry.Entries++;
                        entry.Stones += result.Stones;
                        entry.Kills += result.Kills;
                        entry.LevelSum += result.LevelSum;
                        this.CountPlayers(result, month, null);
                        break;
                    case IllusionTempleLeagueStage.SemiFinal:
                        entry.IsFinalist |= result == winner;
                        break;
                    case IllusionTempleLeagueStage.Final when winner is not null:
                        entry.IsChampion |= result == winner;
                        this.CountPlayers(result, month, result.GuildKey);
                        break;
                    default:
                        break;
                }
            }
        }

        return winner?.GuildKey;
    }

    /// <summary>
    /// Gets the guilds of the eliminations of the month, best first: by occupied stones, kills and level sum.
    /// </summary>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The keys of the guilds.</returns>
    internal IReadOnlyList<Guid> GetRanking(DateTime localNow)
    {
        var month = MonthOf(localNow);
        lock (this._guilds.SyncRoot)
        {
            return [.. this._guilds.GetAll()
                .Where(pair => pair.Value.Month == month && pair.Value.Entries > 0)
                .OrderByDescending(pair => (pair.Value.Stones, pair.Value.Kills, pair.Value.LevelSum))
                .ThenBy(pair => pair.Key)
                .Select(pair => pair.Key)];
        }
    }

    /// <summary>
    /// Gets the temple of a guild in the semi-finals: its slot by its rank (<see cref="IllusionTempleLeagueConfiguration.SemiFinalSlots"/>),
    /// slots 1 and 2 in the first, 3 and 4 in the second.
    /// </summary>
    /// <param name="guildKey">The key of the guild.</param>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The game level, 0 when the guild isn't one of the best four.</returns>
    internal int GetSemiFinalLevel(Guid guildKey, DateTime localNow)
    {
        var rank = this.GetRanking(localNow).Take(SemiFinalists).ToList().IndexOf(guildKey);
        if (rank < 0)
        {
            return 0;
        }

        var slots = this.Settings.SemiFinalSlots;
        var slot = rank < slots.Count ? slots[rank] : rank + 1;
        return slot <= 2 ? 1 : 2;
    }

    /// <summary>
    /// Takes the reward of a character who played the final for the champion, once, from the reward day to the end of the
    /// month.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <param name="localNow">The time of the server.</param>
    /// <returns>The reward, or <c>null</c>.</returns>
    internal RewardItem? ClaimReward(Guid characterId, DateTime localNow)
    {
        if (localNow.Day < this.Settings.RewardDay || this.Settings.ChampionReward is not { } reward)
        {
            return null;
        }

        var month = MonthOf(localNow);
        lock (this._guilds.SyncRoot)
        {
            lock (this._players.SyncRoot)
            {
                if (this._players.Find(characterId) is not { FinalGuild: { } guild, IsRewarded: false } player
                    || player.Month != month
                    || this.FindEntry(guild, month)?.IsChampion is not true)
                {
                    return null;
                }

                this._players.GetOrCreate(characterId).IsRewarded = true;
                return reward;
            }
        }
    }

    private static DateOnly MonthOf(DateTime date) => new(date.Year, date.Month, 1);

    private IllusionTempleLeagueGuildEntry? FindEntry(Guid guildKey, DateOnly month)
        => this._guilds.Find(guildKey) is { } entry && entry.Month == month ? entry : null;

    private IllusionTempleLeagueGuildEntry GetOrCreateEntry(Guid guildKey, DateOnly month)
    {
        var entry = this._guilds.GetOrCreate(guildKey);
        if (entry.Month != month)
        {
            (entry.Month, entry.Entries, entry.Stones, entry.Kills, entry.LevelSum, entry.IsFinalist, entry.IsChampion)
                = (month, 0, 0, 0, 0, false, false);
        }

        return entry;
    }

    private void CountPlayers(IllusionTempleLeagueResult result, DateOnly month, Guid? finalGuild)
    {
        lock (this._players.SyncRoot)
        {
            foreach (var characterId in result.CharacterIds)
            {
                var player = this._players.GetOrCreate(characterId);
                if (player.Month != month)
                {
                    (player.Month, player.Entries, player.FinalGuild, player.IsRewarded) = (month, 0, null, false);
                }

                if (finalGuild is { } guild)
                {
                    player.FinalGuild = guild;
                }
                else
                {
                    player.Entries++;
                }
            }
        }
    }
}
