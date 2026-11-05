// <copyright file="LuckyItemRefineryCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// Crafting which refines a lucky item into a jewel of extension or a jewel of elevation.
/// </summary>
/// <remarks>
/// The settings require exactly one lucky item (<see cref="LuckyItemExtensions.IsLuckyItem"/>), which always
/// disappears. The first result item of the settings (the jewel of extension) is given with a chance of
/// <see cref="FirstResultChance"/>; otherwise the second one (the jewel of elevation) with a chance of
/// <see cref="SecondResultChance"/>; otherwise nothing.
/// </remarks>
public class LuckyItemRefineryCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// The chance in percent of the first result item.
    /// </summary>
    /// <remarks>
    /// The Season 6 client only says that the material disappears. The value is the one of the Season 21 server data
    /// (LuckyItemRefineMixRate1). It's a constant, because a result item of a crafting has no chance of its own.
    /// </remarks>
    internal const int FirstResultChance = 60;

    /// <summary>
    /// The chance in percent of the second result item, when the first one wasn't given.
    /// </summary>
    /// <remarks>
    /// The value of the Season 21 server data (LuckyItemRefineMixRate2), see <see cref="FirstResultChance"/>.
    /// </remarks>
    internal const int SecondResultChance = 10;

    private readonly SimpleCraftingSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="LuckyItemRefineryCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public LuckyItemRefineryCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
        this._settings = settings;
    }

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        if (base.TryGetRequiredItems(player, out items, out successRate) is { } error)
        {
            return error;
        }

        if (items.Sum(link => link.Items.Count()) != 1
            || items.SelectMany(link => link.Items).Any(item => item.Definition?.IsLuckyItem(player.GameContext.Configuration) is not true))
        {
            return CraftingResult.IncorrectMixItems;
        }

        return default;
    }

    /// <inheritdoc />
    protected override async ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var resultItems = this._settings.ResultItems.ToList();
        var index = Rand.NextRandomBool(FirstResultChance) ? 0 : Rand.NextRandomBool(SecondResultChance) ? 1 : -1;
        if (index < 0 || index >= resultItems.Count || resultItems[index].ItemDefinition is not { } definition)
        {
            return [];
        }

        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Durability = item.GetMaximumDurabilityOfOnePiece();
        await player.TemporaryStorage!.AddItemAsync(item).ConfigureAwait(false);
        return [item];
    }
}
