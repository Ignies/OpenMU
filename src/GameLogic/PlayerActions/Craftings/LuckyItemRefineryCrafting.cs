// <copyright file="LuckyItemRefineryCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// Crafting which refines a lucky item into a jewel of extension.
/// </summary>
/// <remarks>
/// The settings require exactly one lucky item (<see cref="LuckyItemExtensions.IsLuckyItem"/>), which disappears
/// whatever the outcome. It succeeds with the <see cref="SimpleCraftingSettings.SuccessPercent"/> of the settings
/// for a piece at its full durability, and with <see cref="DamagedItemSuccessPercent"/> otherwise. The result is
/// always a jewel of extension, picked by its identity among the result items of the settings.
/// </remarks>
public class LuckyItemRefineryCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// The success chance in percent for a piece which lost durability.
    /// </summary>
    /// <remarks>
    /// The value of the Season 21 server data (LuckyItemRefineMixRate2), which the item guides describe as the
    /// chance of a damaged piece. It's a constant, because the settings of a crafting have one success rate only.
    /// </remarks>
    internal const byte DamagedItemSuccessPercent = 10;

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

        var pieces = items.SelectMany(link => link.Items).ToList();
        if (pieces.Count != 1 || pieces[0].Definition?.IsLuckyItem(player.GameContext.Configuration) is not true)
        {
            return CraftingResult.IncorrectMixItems;
        }

        var piece = pieces[0];
        successRate = piece.Durability >= piece.GetMaximumDurabilityOfOnePiece() ? this._settings.SuccessPercent : DamagedItemSuccessPercent;
        return default;
    }

    /// <inheritdoc />
    protected override async ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var jewel = ItemConstants.JewelOfExtension;
        if (this._settings.ResultItems.Select(r => r.ItemDefinition).FirstOrDefault(d => d?.Group == jewel.Group && d.Number == jewel.Number) is not { } definition)
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
