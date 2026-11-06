// <copyright file="Season21SeasonPassInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Globalization;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Configures the <see cref="SeasonPassPlugIn"/> with the season pass of <c>Event/BattlePass/BattlePass.xml</c>.
/// </summary>
internal class Season21SeasonPassInitializer
{
    /// <summary>
    /// The buff "Season Pass - Increase Max HP", the only effect whose attribute the data names.
    /// </summary>
    private const int MaximumHealthEffect = 411;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21SeasonPassInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21SeasonPassInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the season pass.
    /// </summary>
    public void Initialize()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<SeasonPassConfiguration>(this._context, this._gameConfiguration, typeof(SeasonPassPlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Seasons.Count > 0)
        {
            // A configuration which can't be read, or one which already has its seasons, is the operator's.
            return;
        }

        var data = Season21WorldSystemsData.Instance.SeasonPass;
        configuration.GoldPassEnabled = data.Gold;
        configuration.PremiumPassEnabled = data.Premium;
        configuration.TicketExperience = data.TicketExperience;
        configuration.Seasons = data.Schedule.Select(s => new SeasonPassSeason { Number = s.Id, Start = ParseDate(s.Start), End = ParseDate(s.End) }).ToList();
        configuration.LevelExperience = [.. data.Levels];
        configuration.Missions = data.Missions.Select(m => CreateMission(m, false)).Concat(data.Special.Select(m => CreateMission(m, true))).ToList();
        configuration.Rewards = data.Rewards.Select(r => new SeasonPassReward
        {
            Id = r.Id,
            EffectIndex = r.Type == 0 ? r.Effect : -1,
            ItemGroup = (byte)Math.Max(0, r.G),
            ItemNumber = (short)Math.Max(0, r.I),
            Count = (byte)Math.Clamp(r.N, 0, byte.MaxValue),
            Name = r.Name,
        }).ToList();
        configuration.LevelRewards = data.Lists
            .SelectMany(list => list.Levels.Select((ids, i) => new SeasonPassLevelReward { Pass = (SeasonPassKind)list.Pass, Level = (byte)(i + 1), RewardIds = [.. ids] }))
            .Where(r => r.RewardIds.Count > 0)
            .ToList();
        configuration.Buffs = data.Buffs.Select(b => new SeasonPassBuff
        {
            EffectIndex = b.Effect,
            AttributeId = b.Effect == MaximumHealthEffect ? Stats.MaximumHealth.Id : null,
            Values = [.. b.Values],
            Name = data.Rewards.FirstOrDefault(r => r.Type == 0 && r.Effect == b.Effect)?.Name ?? string.Empty,
        }).ToList();

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(configuration, null);
    }

    private static SeasonPassMission CreateMission(Season21WorldSystemsData.PassMissionRow row, bool isSpecial)
        => new() { Index = row.Index, IsSpecial = isSpecial, Kind = SeasonPassMissionKind.None, Experience = row.Experience, Required = row.Required, Repeat = row.Repeat };

    private static DateTime ParseDate(string value)
        => DateTime.SpecifyKind(DateTime.ParseExact(value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), DateTimeKind.Utc);
}
