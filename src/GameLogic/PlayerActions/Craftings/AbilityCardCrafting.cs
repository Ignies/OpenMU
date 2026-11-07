// <copyright file="AbilityCardCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.AbilityCore;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// The crafting of the cards of the Ability Enhancement Core of Season 21 at the chaos goblin from Ability Crystals,
/// while the <see cref="AbilityCoreFeaturePlugIn"/> is on.
/// </summary>
/// <remarks>
/// The result is one of the result items which fit the class of the player (<see cref="AbilityCardDefinition.BaseClasses"/>;
/// an item without a card definition, like the stones, fits every class), with one card option by the option weights.
/// </remarks>
public class AbilityCardCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// The crafting number of a skill card of the class.
    /// </summary>
    public const byte SkillCardNumber = 180;

    /// <summary>
    /// The crafting number of an option card of type 1; types 2 to 5 follow.
    /// </summary>
    public const byte FirstOptionCardNumber = 181;

    /// <summary>
    /// The crafting number of the blue elite card; green and purple follow.
    /// </summary>
    public const byte FirstEliteCardNumber = 186;

    /// <summary>
    /// The crafting number of the Ability Upgrade Stones.
    /// </summary>
    public const byte UpgradeStonesNumber = 189;

    private readonly SimpleCraftingSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilityCardCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public AbilityCardCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
        this._settings = settings;
    }

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        if (this.GetResults(player).Count == 0)
        {
            items = [];
            successRate = 0;
            return CraftingResult.IncorrectMixItems;
        }

        return base.TryGetRequiredItems(player, out items, out successRate);
    }

    /// <inheritdoc />
    protected override async ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        if (this.GetResults(player).SelectRandom() is not { ItemDefinition: { } definition } result)
        {
            return [];
        }

        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Durability = result.Durability ?? item.GetMaximumDurabilityOfOnePiece();
        var options = definition.PossibleItemOptions.SelectMany(o => o.PossibleOptions)
            .Where(o => o.OptionType == Season21ItemOptionTypes.AbilityCardOption)
            .ToList();
        if (options.SelectWeightedRandom(options.Select(o => (int)o.Weight)) is { } option)
        {
            var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
            link.ItemOption = option;
            item.ItemOptions.Add(link);
        }

        await player.TemporaryStorage!.AddItemAsync(item).ConfigureAwait(false);
        return [item];
    }

    private static bool Fits(AbilityCoreConfiguration configuration, ItemDefinition definition, BaseClassCode baseClass)
        => configuration.Cards.FirstOrDefault(c => c.ItemGroup == definition.Group && c.ItemNumber == definition.Number) is not { } card
           || card.IsFor(baseClass);

    private List<ItemCraftingResultItem> GetResults(Player player)
    {
        if (AbilityCoreFeaturePlugIn.Get(player.GameContext)?.GetConfiguration() is not { } configuration
            || player.SelectedCharacter?.CharacterClass is not { } characterClass
            || ClassCodes.GetBaseClass(characterClass.Number) is not { } baseClass)
        {
            return [];
        }

        return this._settings.ResultItems.Where(r => r.ItemDefinition is { } definition && Fits(configuration, definition, baseClass)).ToList();
    }
}
