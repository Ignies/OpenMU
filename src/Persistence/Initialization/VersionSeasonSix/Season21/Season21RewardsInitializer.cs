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
        var levels = Season21PlugInConfigurations.GetOrCreate<LevelRewardsConfiguration>(this._context, this._gameConfiguration, typeof(LevelRewardsPlugIn).GUID, out var levelPlugIn);
        if (levels is { Rewards.Count: > 0 } && levels.Rewards.Any(r => r.Id == 0))
        {
            // Version 1 had no ids: the rows of the data get theirs, the operator's own rows the next free ones.
            var used = levels.Rewards.Where(r => r.Id != 0).Select(r => r.Id).ToHashSet();
            foreach (var reward in levels.Rewards.Where(r => r.Id == 0))
            {
                var row = data.LevelRewards.FirstOrDefault(d => d.TotalLevel == reward.TotalLevel && d.Group == reward.Group && d.Number == reward.Number && !used.Contains(d.Id));
                reward.Id = row?.Id ?? (byte)Enumerable.Range(1, LevelReward.MaximumId).FirstOrDefault(id => !used.Contains((byte)id));
                used.Add(reward.Id);
            }

            levelPlugIn.SetConfiguration(levels, null);
        }

        if (levels is { Rewards.Count: 0 })
        {
            var rewards = data.LevelRewards.Select(row => new LevelReward
            {
                Id = row.Id,
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

        var contribution = Season21PlugInConfigurations.GetOrCreate<EventContributionConfiguration>(this._context, this._gameConfiguration, typeof(EventContributionPlugIn).GUID, out var contributionPlugIn);
        if (contribution is { Rules.Count: > 0 } && this.AddRanksAndRelics(contribution))
        {
            contributionPlugIn.SetConfiguration(contribution, null);
        }

        if (contribution is { Rules.Count: 0, Boxes.Count: 0 })
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
                    RankPoints = row.RankPoints.ToList(),
                    RelicClaimPoints = row.RelicClaim,
                    RelicScorePoints = row.RelicScore,
                    SpecialMonsters = row.SpecialMonsters.Select(pair => new MonsterContributionPoints { MonsterNumber = (short)pair[0], Points = pair[1] }).ToList(),
                }).ToList(),
            };
            contributionPlugIn.SetConfiguration(configuration, null);
            contributionPlugIn.IsActive = true;
        }
    }

    /// <summary>
    /// Adds what version 1 lacked: the rank points (devil square, whose rank 1 was its winner) and the relic points
    /// (illusion temple), where the rule has none yet.
    /// </summary>
    private bool AddRanksAndRelics(EventContributionConfiguration contribution)
    {
        var changed = false;
        foreach (var row in Season21Economy.Data.ContributionRules)
        {
            if (contribution.Rules.FirstOrDefault(r => r.Type == Enum.Parse<MiniGameType>(row.Type)) is not { } rule)
            {
                continue;
            }

            if (row.RankPoints.Count > 0 && rule.RankPoints.Count == 0)
            {
                rule.RankPoints.AddRange(row.RankPoints);
                if (rule.WinnerPoints == row.RankPoints[0])
                {
                    rule.WinnerPoints = row.Winner;
                }

                changed = true;
            }

            if ((row.RelicClaim > 0 || row.RelicScore > 0) && rule is { RelicClaimPoints: 0, RelicScorePoints: 0 })
            {
                rule.RelicClaimPoints = row.RelicClaim;
                rule.RelicScorePoints = row.RelicScore;
                changed = true;
            }
        }

        return changed;
    }
}
