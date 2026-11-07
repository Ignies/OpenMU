// <copyright file="TrophiesOfBattleCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.GameLogic.ArcaWar;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// The exchange of the Trophies of Battle of the Arca War at Sir Lesnar, while the <see cref="ArcaWarPlugIn"/> is on:
/// 1 to 100 trophies for one item of the result items of their tier, whose <see cref="ItemCraftingResultItem.Reference"/>
/// is the tier (1 for 1 to 10 trophies, ... 10 for 91 to 100).
/// </summary>
/// <remarks>
/// The rate starts at <see cref="SimpleCraftingSettings.SuccessPercent"/> for the first trophy of each tier and adds the
/// <see cref="ItemCraftingRequiredItem.AddPercentage"/> of the trophies for each further one of it.
/// </remarks>
public class TrophiesOfBattleCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// The number of the crafting at Sir Lesnar (no data).
    /// </summary>
    public const byte Number = 53;

    /// <summary>
    /// The trophies of one tier.
    /// </summary>
    public const int TierSize = 10;

    private readonly SimpleCraftingSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrophiesOfBattleCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public TrophiesOfBattleCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
        this._settings = settings;
    }

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        if (ArcaWarPlugIn.Get(player.GameContext) is null)
        {
            items = [];
            successRate = 0;
            return CraftingResult.IncorrectMixItems;
        }

        if (base.TryGetRequiredItems(player, out items, out successRate) is { } error)
        {
            return error;
        }

        var count = CountTrophies(items);
        if (count == 0)
        {
            return CraftingResult.IncorrectMixItems;
        }

        var addPercentage = items.First(link => link.Items.Any()).ItemRequirement.AddPercentage;
        var rate = this._settings.SuccessPercent + (addPercentage * ((count - 1) % TierSize));
        if (this._settings.MaximumSuccessPercent > 0)
        {
            rate = Math.Min(this._settings.MaximumSuccessPercent, rate);
        }

        successRate = (byte)Math.Min(100, rate);
        return default;
    }

    /// <inheritdoc />
    protected override async ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var tier = ((CountTrophies(requiredItems) - 1) / TierSize) + 1;
        if (this._settings.ResultItems.Where(r => r.Reference == tier && r.ItemDefinition is not null).SelectRandom() is not { } result)
        {
            return [];
        }

        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = result.ItemDefinition;
        item.Level = (byte)Rand.NextInt(result.RandomMinimumLevel, result.RandomMaximumLevel + 1);
        item.Durability = result.Durability ?? item.GetMaximumDurabilityOfOnePiece();
        await player.TemporaryStorage!.AddItemAsync(item).ConfigureAwait(false);
        return [item];
    }

    private static int CountTrophies(IEnumerable<CraftingRequiredItemLink> links)
        => links.SelectMany(link => link.Items).Sum(item => item.IsStackable() ? (int)item.Durability : 1);
}
