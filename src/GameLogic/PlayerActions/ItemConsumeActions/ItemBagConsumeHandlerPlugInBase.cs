// <copyright file="ItemBagConsumeHandlerPlugInBase.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using MUnique.OpenMU.GameLogic.PlayerActions.Items;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.GameLogic.WideItems;

/// <summary>
/// Opens an item bag of Season 21 (S21Emu <c>EventItemBag</c>), which can't be dropped to open it: every drop group of
/// the bag's definition (<c>ItemDefinition.DropItems</c>) of the bag's level rolls its chance and gives one item
/// next to the player, and the character gets the Ruud of the bag's row (<see cref="Season21ItemsConfiguration.BagRuud"/>).
/// </summary>
/// <remarks>Needs the item rules of Season 21 (<see cref="Season21ItemsFeaturePlugIn"/>).</remarks>
public abstract class ItemBagConsumeHandlerPlugInBase : BaseConsumeHandlerPlugIn
{
    /// <inheritdoc />
    public override async ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        if (Season21ItemsFeaturePlugIn.GetConfiguration(player.GameContext) is not { } configuration
            || item.Definition is not { } definition
            || player.CurrentMap is not { } map
            || !this.CheckPreconditions(player, item))
        {
            return false;
        }

        var groups = definition.DropItems.Where(g => g.SourceItemLevel == item.Level).ToList();
        var ruud = configuration.BagRuud.FirstOrDefault(r => r.Group == definition.Group && r.Number == definition.Number);
        if (groups.Count == 0 && ruud is null)
        {
            return false;
        }

        await this.ConsumeSourceItemAsync(player, item).ConfigureAwait(false);
        var randomizer = Rand.GetRandomizer();
        foreach (var group in groups.Where(g => g.Chance >= 1 || randomizer.NextRandomBool(g.Chance)))
        {
            if (player.GameContext.DropGenerator.GenerateItemDrop(group) is { } drop)
            {
                await map.AddAsync(new DroppedItem(drop, player.Position, map, player)).ConfigureAwait(false);
            }
        }

        if (ruud is not null)
        {
            await player.AddRuudAsync(randomizer.NextInt(ruud.Minimum, ruud.Maximum + 1)).ConfigureAwait(false);
        }

        if (groups.Select(g => g.DropEffect).FirstOrDefault(e => e != ItemDropEffect.Undefined) is var effect and not ItemDropEffect.Undefined)
        {
            await ItemBoxDroppedPlugIn.ShowDropEffectAsync(player, effect).ConfigureAwait(false);
        }

        return true;
    }
}
