// <copyright file="AbilityCoreConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.AbilityCore;

/// <summary>
/// Configuration of the <see cref="AbilityCoreFeaturePlugIn"/>: the slot rules, the cards and their prices.
/// </summary>
/// <remarks>
/// The levels are Season 21 total levels (level plus master level); the progression mode of the world maps them like
/// every Season 21 requirement (<see cref="Classes.EvolutionRules.IsMet(int, int, int, int, Progression.ProgressionConfiguration?)"/>).
/// The update "Add the Ability Enhancement Core of Season 21" fills the cards from the S21Emu data.
/// </remarks>
public class AbilityCoreConfiguration
{
    /// <summary>
    /// Gets or sets the Season 21 total level at which each skill slot opens (webzen guide: 1200, 1300, 1400).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCoreConfiguration_SkillSlotLevels_Name))]
    public List<int> SkillSlotLevels { get; set; } = [1200, 1300, 1400];

    /// <summary>
    /// Gets or sets the Season 21 total level at which each row of five option slots opens: five at the fifth
    /// evolution, five more every 50 levels (webzen guide).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCoreConfiguration_OptionRowLevels_Name))]
    public List<int> OptionRowLevels { get; set; } = [1200, 1250, 1300];

    /// <summary>
    /// Gets or sets a value indicating whether a row takes only the option and elite cards of its color
    /// (blue, green, purple); the guides name a color per row but no rule, so it is a setting.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCoreConfiguration_RowsMatchColor_Name))]
    public bool RowsMatchColor { get; set; } = true;

    /// <summary>
    /// Gets or sets the highest card level (webzen guide: 5; S21Emu <c>AbilityLevelUP.xml</c> has the prices of
    /// the levels 0 to 4).
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCoreConfiguration_MaximumCardLevel_Name))]
    [Range(0, 15)]
    public byte MaximumCardLevel { get; set; } = 5;

    /// <summary>
    /// Gets or sets the cards.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCoreConfiguration_Cards_Name))]
    public List<AbilityCardDefinition> Cards { get; set; } = [];

    /// <summary>
    /// Gets or sets the prices of the level ups.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCoreConfiguration_LevelUps_Name))]
    public List<AbilityCardLevelUp> LevelUps { get; set; } = [];

    /// <summary>
    /// Gets or sets the reductions into stones.
    /// </summary>
    [Display(ResourceType = typeof(PlugInResources), Name = nameof(PlugInResources.AbilityCoreConfiguration_Reductions_Name))]
    public List<AbilityCardReduction> Reductions { get; set; } = [];

    /// <summary>
    /// Gets the card of an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The card, or <c>null</c> if the item is none.</returns>
    public AbilityCardDefinition? FindCard(Item item)
        => item.Definition is { } definition
            ? this.Cards.FirstOrDefault(c => c.ItemGroup == definition.Group && c.ItemNumber == definition.Number)
            : null;
}
