// <copyright file="EventContributionRule.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// The contribution points which a kind of event gives.
/// </summary>
public class EventContributionRule
{
    /// <summary>
    /// Gets or sets the kind of event.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_Type_Name))]
    public MiniGameType Type { get; set; }

    /// <summary>
    /// Gets or sets the points of a killed monster.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_MonsterKillPoints_Name))]
    public int MonsterKillPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of a killed monster on Sundays; 0 for <see cref="MonsterKillPoints"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_SundayMonsterKillPoints_Name))]
    public int SundayMonsterKillPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of a killed player.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_PlayerKillPoints_Name))]
    public int PlayerKillPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of the winner.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_WinnerPoints_Name))]
    public int WinnerPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of the ranks at the end of an event with a ranking (devil square): the first entry is rank 1.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_RankPoints_Name))]
    public List<int> RankPoints { get; set; } = [];

    /// <summary>
    /// Gets or sets the points of taking the relic of a statue (illusion temple).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_RelicClaimPoints_Name))]
    public int RelicClaimPoints { get; set; }

    /// <summary>
    /// Gets or sets the points of bringing a relic home (illusion temple).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_RelicScorePoints_Name))]
    public int RelicScorePoints { get; set; }

    /// <summary>
    /// Gets or sets the points of special monsters, instead of <see cref="MonsterKillPoints"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.EventContributionRule_SpecialMonsters_Name))]
    public List<MonsterContributionPoints> SpecialMonsters { get; set; } = [];
}
