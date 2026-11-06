// <copyright file="ErrtelRankUpCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// The rank up of an errtel in the elemental refinery: an errtel whose highest rank option is high enough gets a
/// random option of the next rank, at level 0 (S21Emu ElementalUpgrade.xml, ErrtelUpgradeRateInfo; the money is
/// ElementalItemRefinemenMixRate.xml, RankUpgrade). It takes no other material: S21Emu names none.
/// </summary>
public class ErrtelRankUpCrafting : SimpleItemCraftingHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrtelRankUpCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public ErrtelRankUpCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
    }

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        var result = base.TryGetRequiredItems(player, out items, out successRate);
        if (result != default)
        {
            return result;
        }

        var errtel = items.Single(i => i.ItemRequirement.Reference == ErrtelLevelUpCrafting.ErrtelReference).Items.Single();
        var ranks = errtel.GetRankOptions().ToList();
        if (!errtel.IsErrtel()
            || ranks.Count == 0
            || GetNextRankOptions(errtel, ranks.Count).Count == 0
            || ElementsFeaturePlugIn.GetConfiguration(player.GameContext)?.RankUps.FirstOrDefault(r => r.Rank == ranks.Count) is not { } rankUp
            || rankUp.Rates.ElementAtOrDefault(ranks.GetErrtelLevel()) is not (> 0 and var rate))
        {
            return CraftingResult.IncorrectMixItems;
        }

        successRate = (byte)Math.Clamp(rate, 0, 100);
        return null;
    }

    /// <inheritdoc />
    protected override ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var errtel = requiredItems.Single(i => i.ItemRequirement.Reference == ErrtelLevelUpCrafting.ErrtelReference).Items.Single();
        var rank = errtel.GetRankOptions().Count();
        if (GetNextRankOptions(errtel, rank).SelectRandom() is { } option)
        {
            var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
            link.ItemOption = option;
            link.Index = rank;
            errtel.ItemOptions.Add(link);
        }

        return ValueTask.FromResult(new List<Item> { errtel });
    }

    private static List<IncreasableItemOption> GetNextRankOptions(Item errtel, int rank)
        => errtel.Definition!.PossibleItemOptions
            .SelectMany(o => o.PossibleOptions)
            .Where(o => o.OptionType == Season21ItemOptionTypes.ErrtelRank && o.SubOptionType == rank + 1)
            .Distinct()
            .ToList();
}
