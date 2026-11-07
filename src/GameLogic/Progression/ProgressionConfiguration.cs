// <copyright file="ProgressionConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Configuration of the <see cref="ProgressionFeaturePlugIn"/>.
/// </summary>
/// <remarks>
/// The star costs (limit, required level, level after a star, zen, items) are the <see cref="Resets.ResetConfiguration"/>'s;
/// <see cref="StarResetDefaults"/> seeds them. Values outside the ranges are clamped where they are used.
/// </remarks>
public class ProgressionConfiguration
{
    /// <summary>
    /// The lowest level cap.
    /// </summary>
    internal const int MinLevelCap = 100;

    /// <summary>
    /// The highest level cap: Season 6 and Season 21 both stop the normal level at 400.
    /// </summary>
    internal const int MaxLevelCap = 400;

    /// <summary>
    /// The highest master level cap in the original mode.
    /// </summary>
    internal const int MaxOriginalMasterLevelCap = 1400;

    /// <summary>
    /// The highest master level cap in the normalized modes, the Season 6 cap.
    /// </summary>
    internal const int MaxNormalizedMasterLevelCap = 200;

    /// <summary>
    /// The Season 21 master level cap.
    /// </summary>
    internal const int OriginalMasterLevelCap = 1350;

    /// <summary>
    /// Gets or sets the progression mode.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_Mode_Name))]
    public ProgressionMode Mode { get; set; } = ProgressionMode.Stars;

    /// <summary>
    /// Gets or sets the highest normal level a requirement asks for.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_LevelCap_Name))]
    [Range(MinLevelCap, MaxLevelCap)]
    public int LevelCap { get; set; } = 400;

    /// <summary>
    /// Gets or sets the master level cap; <c>null</c> means the mode's default (1350 original, 200 normalized, 0 stars).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_MasterLevelCap_Name))]
    [Range(0, MaxOriginalMasterLevelCap)]
    public int? MasterLevelCap { get; set; }

    /// <summary>
    /// Gets or sets the normalized level of a Season 21 requirement of 400.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_Anchor400_Name))]
    [Range(150, 250)]
    public int Anchor400 { get; set; } = 190;

    /// <summary>
    /// Gets or sets the normalized level of a Season 21 requirement of 1000.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_Anchor1000_Name))]
    [Range(300, 400)]
    public int Anchor1000 { get; set; } = 400;

    /// <summary>
    /// Gets or sets the step a normalized requirement is rounded to (1, 5 or 10).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_RequirementRounding_Name))]
    [Range(1, 10)]
    public int RequirementRounding { get; set; } = 5;

    /// <summary>
    /// Gets or sets which entry levels the classic maps use in the normalized modes.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_ClassicMapEntryLevels_Name))]
    public ClassicMapEntryLevels ClassicMapEntryLevels { get; set; } = ClassicMapEntryLevels.Season6;

    /// <summary>
    /// Gets or sets the warp zen per normalized entry level.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_EntryZenPerLevel_Name))]
    [Range(0, 500)]
    public int EntryZenPerLevel { get; set; } = 50;

    /// <summary>
    /// Gets or sets a flat warp zen for every Season 21 set; <c>null</c> uses <see cref="EntryZenPerLevel"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_EntryZenFlat_Name))]
    [Range(0, int.MaxValue)]
    public int? EntryZenFlat { get; set; }

    /// <summary>
    /// Gets or sets the percentage applied to the normalized monster health.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_MonsterHealthPercent_Name))]
    [Range(25, 400)]
    public int MonsterHealthPercent { get; set; } = 100;

    /// <summary>
    /// Gets or sets the percentage applied to the normalized monster damage.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_MonsterDamagePercent_Name))]
    [Range(25, 400)]
    public int MonsterDamagePercent { get; set; } = 100;

    /// <summary>
    /// Gets or sets the percentage applied to the normalized monster defense, attack rate and defense rate.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_MonsterDefensePercent_Name))]
    [Range(25, 400)]
    public int MonsterDefensePercent { get; set; } = 100;

    /// <summary>
    /// Gets or sets the lowest health of an elite, relative to its map's normal monsters.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_EliteHealthMinimum_Name))]
    [Range(1.0, 5.0)]
    public double EliteHealthMinimum { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the highest health of an elite, relative to its map's normal monsters.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_EliteHealthMaximum_Name))]
    [Range(1.0, 5.0)]
    public double EliteHealthMaximum { get; set; } = 3.0;

    /// <summary>
    /// Gets or sets the lowest damage of an elite, relative to its map's normal monsters.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_EliteDamageMinimum_Name))]
    [Range(1.0, 5.0)]
    public double EliteDamageMinimum { get; set; } = 1.15;

    /// <summary>
    /// Gets or sets the highest damage of an elite, relative to its map's normal monsters.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_EliteDamageMaximum_Name))]
    [Range(1.0, 5.0)]
    public double EliteDamageMaximum { get; set; } = 2.0;

    /// <summary>
    /// Gets or sets the health of a map boss, relative to its map's normal monsters.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_BossHealthMultiplier_Name))]
    [Range(5.0, 40.0)]
    public double BossHealthMultiplier { get; set; } = 20;

    /// <summary>
    /// Gets or sets the damage of a map boss, relative to its map's normal monsters.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_BossDamageMultiplier_Name))]
    [Range(1.0, 3.0)]
    public double BossDamageMultiplier { get; set; } = 1.5;

    /// <summary>
    /// Gets or sets the health and damage percentage of normalized sets at power level 600, reached linearly from 100 at power level 400 (normalized mode only).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_OverCapStrengthPercent_Name))]
    [Range(50, 100)]
    public int OverCapStrengthPercent { get; set; } = 85;

    /// <summary>
    /// Gets or sets the highest monster level item drops are rolled with in the normalized modes; 0 is off.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_MonsterDropLevelCap_Name))]
    [Range(0, 255)]
    public int MonsterDropLevelCap { get; set; } = 160;

    /// <summary>
    /// Gets or sets the level event brackets are chosen by; <c>null</c> means the mode's default.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_EventBracketKey_Name))]
    public ProgressionLevelKey? EventBracketKey { get; set; }

    /// <summary>
    /// Gets or sets the level item level requirements are checked against; <c>null</c> means the mode's default.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_ItemLevelRequirementKey_Name))]
    public ProgressionLevelKey? ItemLevelRequirementKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a star needs the third class.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarRequiresMasterClass_Name))]
    public bool StarRequiresMasterClass { get; set; } = true;

    /// <summary>
    /// Gets or sets the Season 21 requirement below the first star band.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarBandStart_Name))]
    [Range(0, 1000)]
    public int StarBandStart { get; set; } = 400;

    /// <summary>
    /// Gets or sets the width of a star band in Season 21 levels.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarBandWidth_Name))]
    [Range(100, 600)]
    public int StarBandWidth { get; set; } = 300;

    /// <summary>
    /// Gets or sets the number of stars which give the first tier rewards.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarFirstTierSize_Name))]
    [Range(0, 50)]
    public int StarFirstTierSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets the power levels a star of the first tier is worth.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarWorthFirstTier_Name))]
    [Range(0, 400)]
    public int StarWorthFirstTier { get; set; } = 100;

    /// <summary>
    /// Gets or sets the power levels a star after the first tier is worth.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarWorthSecondTier_Name))]
    [Range(0, 400)]
    public int StarWorthSecondTier { get; set; } = 50;

    /// <summary>
    /// Gets or sets the levels' worth of stat points a star of the first tier gives.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarPointsFirstTier_Name))]
    [Range(0, 400)]
    public int StarPointsFirstTier { get; set; } = 100;

    /// <summary>
    /// Gets or sets the levels' worth of stat points a star after the first tier gives.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarPointsSecondTier_Name))]
    [Range(0, 400)]
    public int StarPointsSecondTier { get; set; } = 50;

    /// <summary>
    /// Gets or sets the master tree points a star gives.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarTreePoints_Name))]
    [Range(0, 50)]
    public int StarTreePoints { get; set; } = 20;

    /// <summary>
    /// Gets or sets the most master tree points a character earns from master levels, and the most the stars give; 0 is no cap.
    /// </summary>
    /// <remarks>
    /// Season 21 gives master tree points up to master level 400; the later levels (a total level above 800) feed the skill
    /// enhancement tree instead (Webzen guides 112 and 362, S21Emu MasterSkillTree.xml).
    /// </remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_MasterTreePointsCap_Name))]
    [Range(0, 2000)]
    public int MasterTreePointsCap { get; set; } = 400;

    /// <summary>
    /// Gets or sets the damage and defense percentage a star adds against monsters.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarPowerPercent_Name))]
    [Range(0, 5)]
    public int StarPowerPercent { get; set; } = 1;

    /// <summary>
    /// Gets or sets the cap of the star power percentage.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarPowerCapPercent_Name))]
    [Range(0, 50)]
    public int StarPowerCapPercent { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether the star power also counts against players.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarPowerInPvp_Name))]
    public bool StarPowerInPvp { get; set; } = false;

    /// <summary>
    /// Gets or sets the experience percentage a star takes away.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarExperiencePenaltyPercent_Name))]
    [Range(0, 20)]
    public int StarExperiencePenaltyPercent { get; set; } = 6;

    /// <summary>
    /// Gets or sets the lowest experience percentage the star penalty leaves.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.ProgressionConfiguration_StarExperienceFloorPercent_Name))]
    [Range(10, 100)]
    public int StarExperienceFloorPercent { get; set; } = 40;
}
