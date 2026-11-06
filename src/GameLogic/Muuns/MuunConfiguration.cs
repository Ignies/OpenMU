// <copyright file="MuunConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Muuns;

/// <summary>
/// The configuration of the <see cref="MuunFeaturePlugIn"/>: the Muuns, their options, the energy converter and the
/// exchange. The update "Add the Muuns of Season 21" fills it with the Season 21 data.
/// </summary>
public class MuunConfiguration
{
    /// <summary>
    /// Gets or sets the item group of the Muuns and their items.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_ItemGroup_Name))]
    public byte ItemGroup { get; set; } = 16;

    /// <summary>
    /// Gets or sets the number of the evolution stone, which evolves the Muun it names.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_EvolutionStoneNumber_Name))]
    public short EvolutionStoneNumber { get; set; } = 211;

    /// <summary>
    /// Gets or sets the number of the tempest evolution stone, which evolves any tempest Muun.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_TempestEvolutionStoneNumber_Name))]
    public short TempestEvolutionStoneNumber { get; set; } = 510;

    /// <summary>
    /// Gets or sets the number of the Muun energy converter.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_EnergyConverterNumber_Name))]
    public short EnergyConverterNumber { get; set; } = 239;

    /// <summary>
    /// Gets or sets the number of the wizardry stone, which a full energy converter becomes.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_WizardryStoneNumber_Name))]
    public short WizardryStoneNumber { get; set; } = 240;

    /// <summary>
    /// Gets or sets the energy at which the converter becomes a wizardry stone.
    /// </summary>
    /// <remarks>120: the item guide of the Muun Energy Converter (mu.lv/guides/item/ew43emi); the converter keeps its
    /// energy as its durability.</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_WizardryStoneEnergy_Name))]
    [Range(1, byte.MaxValue)]
    public int WizardryStoneEnergy { get; set; } = 120;

    /// <summary>
    /// Gets or sets the energy of an evolution stone.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_EvolutionStoneEnergy_Name))]
    public int EvolutionStoneEnergy { get; set; } = 2;

    /// <summary>
    /// Gets or sets the energy of a Muun by its rank.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_RankEnergy_Name))]
    public List<MuunEnergyPoints> RankEnergy { get; set; } = [];

    /// <summary>
    /// Gets or sets the energy of a Muun by its level, added to the one of its rank.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_LevelEnergy_Name))]
    public List<MuunEnergyPoints> LevelEnergy { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of the exchange NPC (Monica).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_ExchangeNpcNumber_Name))]
    public short ExchangeNpcNumber { get; set; } = 682;

    /// <summary>
    /// Gets or sets the offers of the exchange NPC.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_ExchangeOffers_Name))]
    public List<MuunExchangeOffer> ExchangeOffers { get; set; } = [];

    /// <summary>
    /// Gets or sets the Muuns.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_Muuns_Name))]
    public List<MuunDefinition> Muuns { get; set; } = [];

    /// <summary>
    /// Gets or sets the options of the Muuns.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_Options_Name))]
    public List<MuunOptionDefinition> Options { get; set; } = [];

    /// <summary>
    /// Gets or sets the additional options of the Muuns.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_AddOptions_Name))]
    public List<MuunAddOptionDefinition> AddOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the mini games in which a Muun can't be ridden.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_NoRidingMiniGames_Name))]
    public List<MiniGameType> NoRidingMiniGames { get; set; } = [MiniGameType.ChaosCastle, MiniGameType.IllusionTemple];

    /// <summary>
    /// Gets or sets the time after which an equipped Muun loses health.
    /// </summary>
    /// <remarks>Designed: the guides say an equipped Muun slowly loses health, without a number.</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_HealthDecayInterval_Name))]
    public TimeSpan HealthDecayInterval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets or sets the health which an equipped Muun loses after each <see cref="HealthDecayInterval"/>; 0 for none.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_HealthDecayAmount_Name))]
    [Range(0, byte.MaxValue)]
    public int HealthDecayAmount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the time after which the conditions of the options are checked again (hours and days).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_CheckInterval_Name))]
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}
