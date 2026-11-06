// <copyright file="SeasonPassConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// Configuration of the <see cref="SeasonPassPlugIn"/>.
/// </summary>
public class SeasonPassConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the gold pass can be unlocked (data: yes).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_GoldPassEnabled_Name))]
    public bool GoldPassEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the premium pass can be unlocked, on a character with the gold pass (data: no).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_PremiumPassEnabled_Name))]
    public bool PremiumPassEnabled { get; set; }

    /// <summary>
    /// Gets or sets the pass experience of an Exp Pass Ticket (data: 300).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_TicketExperience_Name))]
    public int TicketExperience { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the schedule repeats every year, ignoring the years of its dates (design: the
    /// data schedules the twelve months of one year).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_RepeatEveryYear_Name))]
    public bool RepeatEveryYear { get; set; } = true;

    /// <summary>
    /// Gets or sets the seasons.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_Seasons_Name))]
    public List<SeasonPassSeason> Seasons { get; set; } = [];

    /// <summary>
    /// Gets or sets the pass experience each level needs, from level 1.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_LevelExperience_Name))]
    public List<int> LevelExperience { get; set; } = [];

    /// <summary>
    /// Gets or sets the missions.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_Missions_Name))]
    public List<SeasonPassMission> Missions { get; set; } = [];

    /// <summary>
    /// Gets or sets the rewards.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_Rewards_Name))]
    public List<SeasonPassReward> Rewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the rewards of the levels of the passes.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_LevelRewards_Name))]
    public List<SeasonPassLevelReward> LevelRewards { get; set; } = [];

    /// <summary>
    /// Gets or sets the buffs.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_Buffs_Name))]
    public List<SeasonPassBuff> Buffs { get; set; } = [];

    /// <summary>
    /// Gets or sets the minimum time between two writes of the state file (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_SaveInterval_Name))]
    public TimeSpan SaveInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the file which keeps the passes over a restart, relative to the server directory; empty for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.SeasonPassConfiguration_StateFile_Name))]
    public string StateFile { get; set; } = "state/season_passes.json";
}
