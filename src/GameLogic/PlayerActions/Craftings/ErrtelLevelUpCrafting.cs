// <copyright file="ErrtelLevelUpCrafting.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Craftings;

using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Views.NPC;

/// <summary>
/// The level up of an errtel in the elemental refinery: the errtel and the elemental runes of its rank give its
/// highest rank option one level more (S21Emu ElementalItemRefinemenMixRate.xml, LevelUp). There is one crafting per
/// level, since the money depends on it; the item level range of its errtel requirement is the errtel level.
/// </summary>
public class ErrtelLevelUpCrafting : SimpleItemCraftingHandler
{
    private const int MaximumLevel = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrtelLevelUpCrafting"/> class.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public ErrtelLevelUpCrafting(SimpleCraftingSettings settings)
        : base(settings)
    {
    }

    /// <summary>
    /// Gets the reference of the errtel.
    /// </summary>
    public static byte ErrtelReference { get; } = 0x79;

    /// <inheritdoc />
    public override CraftingResult? TryGetRequiredItems(Player player, out IList<CraftingRequiredItemLink> items, out byte successRate)
    {
        var result = base.TryGetRequiredItems(player, out items, out successRate);
        if (result != default)
        {
            return result;
        }

        var errtel = items.Single(i => i.ItemRequirement.Reference == ErrtelReference).Items.Single();
        var ranks = errtel.GetRankOptions().ToList();
        var level = ranks.GetErrtelLevel();
        var runes = items.Where(i => i.ItemRequirement.Reference != ErrtelReference).Sum(i => i.Items.Sum(r => r.IsStackable() ? r.Durability : 1));
        if (ranks.Count == 0
            || level >= MaximumLevel
            || ElementsFeaturePlugIn.GetConfiguration(player.GameContext)?.LevelUps.FirstOrDefault(l => l.Level == level) is not { } levelUp
            || levelUp.Runes.ElementAtOrDefault(ranks.Count - 1) is not (> 0 and var neededRunes)
            || runes != neededRunes)
        {
            return CraftingResult.IncorrectMixItems;
        }

        successRate = (byte)Math.Clamp(levelUp.Rates.ElementAtOrDefault(ranks.Count - 1), 0, 100);
        return null;
    }

    /// <inheritdoc />
    protected override bool RequiredItemMatches(Item item, ItemCraftingRequiredItem requiredItem)
    {
        if (requiredItem.Reference != ErrtelReference)
        {
            return base.RequiredItemMatches(item, requiredItem);
        }

        var level = item.GetRankOptions().GetErrtelLevel();
        return item.IsErrtel()
               && (!requiredItem.PossibleItems.Any() || requiredItem.PossibleItems.Contains(item.Definition!))
               && level >= requiredItem.MinimumItemLevel
               && level <= requiredItem.MaximumItemLevel;
    }

    /// <inheritdoc />
    protected override ValueTask<List<Item>> CreateOrModifyResultItemsAsync(IList<CraftingRequiredItemLink> requiredItems, Player player, byte socketSlot, byte successRate)
    {
        var errtel = requiredItems.Single(i => i.ItemRequirement.Reference == ErrtelReference).Items.Single();
        errtel.GetRankOptions().Last().Level++;
        return ValueTask.FromResult(new List<Item> { errtel });
    }
}
