// <copyright file="Season21RewardsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The level rewards (<c>ItemCompensationLevel.xml</c>) and the event contribution boxes (<c>EventMapPointIntegration.xml</c>)
/// of Season 21, as the configurations of their plugins, which the first application switches on.
/// </summary>
internal class Season21RewardsInitializer
{
    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21RewardsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21RewardsInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the data. A configuration which has rows already stays, with the operator's switch.
    /// </summary>
    public void Initialize()
    {
        var data = Season21Economy.Data;
        if (Season21PlugInConfigurations.GetOrCreate<LevelRewardsConfiguration>(this._context, this._gameConfiguration, typeof(LevelRewardsPlugIn).GUID, out var levelPlugIn) is { Rewards.Count: 0 })
        {
            var rewards = data.LevelRewards.Select(row => new LevelReward
            {
                TotalLevel = row.TotalLevel,
                Group = row.Group,
                Number = row.Number,
                Level = row.Level,
                Count = row.Count,
                Classes = row.Classes.Select(c => (BaseClassCode)c).ToList(),
            }).ToList();
            levelPlugIn.SetConfiguration(new LevelRewardsConfiguration { Rewards = rewards }, null);
            levelPlugIn.IsActive = true;
        }

        if (Season21PlugInConfigurations.GetOrCreate<EventContributionConfiguration>(this._context, this._gameConfiguration, typeof(EventContributionPlugIn).GUID, out var contributionPlugIn) is { Rules.Count: 0, Boxes.Count: 0 })
        {
            var configuration = new EventContributionConfiguration
            {
                Boxes = data.ContributionBoxes.Select(row => new ContributionBox { Group = row.Group, Number = row.Number, Count = row.Count, MinimumPoints = row.Minimum, MaximumPoints = row.Maximum }).ToList(),
                Rules = data.ContributionRules.Select(row => new EventContributionRule
                {
                    Type = Enum.Parse<MiniGameType>(row.Type),
                    MonsterKillPoints = row.MonsterKill,
                    SundayMonsterKillPoints = row.SundayMonsterKill,
                    WinnerPoints = row.Winner,
                    PlayerKillPoints = row.PlayerKill,
                    SpecialMonsters = row.SpecialMonsters.Select(pair => new MonsterContributionPoints { MonsterNumber = (short)pair[0], Points = pair[1] }).ToList(),
                }).ToList(),
            };
            contributionPlugIn.SetConfiguration(configuration, null);
            contributionPlugIn.IsActive = true;
        }
    }
}
