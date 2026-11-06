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
    /// <remarks>40 minutes: the guardian mounts of Season 14 on, which share the Muun health (life 255, restored by a jewel of
    /// life), lose "one point every 40 minutes the item is equipped" (muonlinefanz.com/guide/items/guardian-mount); the Muun
    /// guides only say "slowly" (muonlinefanz.com/guide/items/muun, muonline.net/guides/mu-online-muun).</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_HealthDecayInterval_Name))]
    public TimeSpan HealthDecayInterval { get; set; } = TimeSpan.FromMinutes(40);

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

    /// <summary>
    /// Gets or sets the time between two attacks of a Muun with an attack skill option.
    /// </summary>
    /// <remarks>Designed: the guides say each Muun's skill has its own cooldown (muonline.webzen.com guide 61) and the
    /// additional option of these Muuns reduces it by 2 or 3 (muonlinefanz.com/guide/items/muun), without the cooldown.</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_AttackSkillCooldown_Name))]
    public TimeSpan AttackSkillCooldown { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the damage of an attack of a Muun per level of its attack skill.
    /// </summary>
    /// <remarks>Designed: no source gives the damage of the Muun skills.</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_AttackSkillDamagePerLevel_Name))]
    [Range(0, int.MaxValue)]
    public int AttackSkillDamagePerLevel { get; set; } = 100;

    /// <summary>
    /// Gets or sets the range in tiles around the owner in which a Muun attacks a monster.
    /// </summary>
    /// <remarks>Designed: no source gives the range.</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_AttackSkillRange_Name))]
    [Range(1, 20)]
    public int AttackSkillRange { get; set; } = 5;

    /// <summary>
    /// Gets or sets the movement speed of a character which rides a Muun.
    /// </summary>
    /// <remarks>15: the speed of the basic mounts of our data (dinorant, uniria; <c>MovementSpeedConstants</c>); the guides
    /// only say the mounts increase the movement speed (muonlinefanz.com/guide/items/guardian-mount).</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_RidingMovementSpeed_Name))]
    public float RidingMovementSpeed { get; set; } = 15;

    /// <summary>
    /// Gets or sets the total level (level and master level) from which the hero slot opens.
    /// </summary>
    /// <remarks>1750, the highest level of Season 21 (400 + 1350): the hero slot opens "upon reaching maximum character level"
    /// (muonlinefanz.com/guide/items/muun), at level 1750 (mueterny.com Muun guide).</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_HeroSlotLevel_Name))]
    public int HeroSlotLevel { get; set; } = 1750;

    /// <summary>
    /// Gets or sets a value indicating whether only hero Muuns go into the hero slot.
    /// </summary>
    /// <remarks>Read from the name of the slot ("hero muun"); the guides say it lets you equip three Muuns.</remarks>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.MuunConfiguration_HeroSlotHeroMuunsOnly_Name))]
    public bool HeroSlotHeroMuunsOnly { get; set; } = true;
}
