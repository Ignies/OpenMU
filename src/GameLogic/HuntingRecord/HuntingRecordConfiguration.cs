// <copyright file="HuntingRecordConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.HuntingRecord;

/// <summary>
/// Configuration of the <see cref="HuntingRecordPlugIn"/>.
/// </summary>
public class HuntingRecordConfiguration
{
    /// <summary>
    /// Gets or sets the maps on which the hunting is recorded (S21Emu <c>HuntingRecord.txt</c>, 59 maps).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_Maps_Name))]
    public List<short> Maps { get; set; } = [];

    /// <summary>
    /// Gets or sets the monsters whose kills don't count, e.g. the event and seasonal monsters (the exception list of the data).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_ExcludedMonsters_Name))]
    public List<short> ExcludedMonsters { get; set; } = [];

    /// <summary>
    /// Gets or sets the days a record is kept (design: the window shows the last week).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_DaysKept_Name))]
    [Range(1, 90)]
    public int DaysKept { get; set; } = 7;

    /// <summary>
    /// Gets or sets the day on which the weekly rankings start again (design: Tuesday, the maintenance day of the data).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_WeekStartsOn_Name))]
    public DayOfWeek WeekStartsOn { get; set; } = DayOfWeek.Tuesday;

    /// <summary>
    /// Gets or sets the characters on one ranking page (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_PageSize_Name))]
    [Range(1, 50)]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the characters a ranking lists (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_MaximumRanks_Name))]
    [Range(1, 1000)]
    public int MaximumRanks { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time a computed ranking is shown before it's computed again (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_RankingRefreshInterval_Name))]
    public TimeSpan RankingRefreshInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the minimum time between two writes of the state file (design).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_SaveInterval_Name))]
    public TimeSpan SaveInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the file which keeps the records over a restart, relative to the server directory; empty for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.HuntingRecordConfiguration_StateFile_Name))]
    public string StateFile { get; set; } = "state/hunting_records.json";
}
