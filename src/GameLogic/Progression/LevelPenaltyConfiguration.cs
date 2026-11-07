// <copyright file="LevelPenaltyConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Progression;

using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// Configuration of the <see cref="LevelPenaltyPlugIn"/>. The defaults are the values of S21Emu
/// <c>DamagePenalty.lua</c> and <c>ExperienciePenalty.lua</c>; levels are Season 21 total levels, which map through
/// the world's progression mode.
/// </summary>
public class LevelPenaltyConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether a character below a monster's level deals less damage to it.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_DamagePenaltyActive_Name))]
    public bool DamagePenaltyActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the monster deals that much more damage to the character.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_MonsterDamageBonusActive_Name))]
    public bool MonsterDamageBonusActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a character below a monster's level gets less experience for it on the
    /// <see cref="ExperienceMaps"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_ExperiencePenaltyActive_Name))]
    public bool ExperiencePenaltyActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the levels added to the monster's level: the penalty starts below this total level.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_MonsterLevelOffset_Name))]
    [Range(0, 2000)]
    public int MonsterLevelOffset { get; set; } = 600;

    /// <summary>
    /// Gets or sets the percent per level below.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_PercentPerLevel_Name))]
    [Range(0, 100)]
    public int PercentPerLevel { get; set; } = 10;

    /// <summary>
    /// Gets or sets the most levels which count.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_MaximumLevels_Name))]
    [Range(0, 100)]
    public int MaximumLevels { get; set; } = 7;

    /// <summary>
    /// Gets or sets the evolution from which the damage penalty applies outside the <see cref="DamageLevelMaps"/>
    /// (S21Emu <c>CheckEnhanceLevel</c>).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_DamageMinimumEvolution_Name))]
    public ClassEvolution DamageMinimumEvolution { get; set; } = ClassEvolution.Fourth;

    /// <summary>
    /// Gets or sets the maps on which the damage penalty applies by <see cref="DamageLevelMapMinimumLevel"/> instead of
    /// the evolution (the Labyrinth of Dimensions).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_DamageLevelMaps_Name))]
    public List<short> DamageLevelMaps { get; set; } = [115];

    /// <summary>
    /// Gets or sets the Season 21 total level from which the damage penalty applies on the <see cref="DamageLevelMaps"/>.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_DamageLevelMapMinimumLevel_Name))]
    [Range(0, 2000)]
    public int DamageLevelMapMinimumLevel { get; set; } = 400;

    /// <summary>
    /// Gets or sets the maps whose monsters give less experience (the Kardamahal Underground Temple).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_ExperienceMaps_Name))]
    public List<short> ExperienceMaps { get; set; } = [143];

    /// <summary>
    /// Gets or sets the maps without the damage penalty: the five floors of the Deep Dungeon, whose renewal removed the
    /// hunting penalty (Webzen Korea patch notes; the S21Emu Lua has no map exception).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_PenaltyFreeMaps_Name))]
    public List<short> PenaltyFreeMaps { get; set; } = [116, 117, 118, 119, 120];

    /// <summary>
    /// Gets or sets the hunting penalty level of single maps (map number, total level): on such a map the penalty counts
    /// the levels missing to it instead of to the monster level plus <see cref="MonsterLevelOffset"/>. The Webzen guides
    /// name one per Season 21 map (entry level plus 50, e.g. Tormenta 1450, Kardamahal 1550); the S21Emu Lua has none,
    /// so it's empty by default.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.LevelPenaltyConfiguration_MapPenaltyLevels_Name))]
    public Dictionary<short, int> MapPenaltyLevels { get; set; } = [];
}
