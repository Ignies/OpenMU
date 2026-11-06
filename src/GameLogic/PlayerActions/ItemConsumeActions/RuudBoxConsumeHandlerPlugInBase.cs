// <copyright file="RuudBoxConsumeHandlerPlugInBase.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using MUnique.OpenMU.GameLogic.Ruud;

/// <summary>
/// Opens a Ruud box: the character gets the Ruud which the configuration of the <see cref="RuudFeaturePlugIn"/> sets for the box.
/// </summary>
public abstract class RuudBoxConsumeHandlerPlugInBase : BaseConsumeHandlerPlugIn
{
    /// <inheritdoc />
    public override async ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        if (RuudFeaturePlugIn.Get(player.GameContext)?.Configuration?.BoxRewards
                .FirstOrDefault(r => r.Group == item.Definition?.Group && r.Number == item.Definition.Number) is not { Amount: > 0 } reward
            || !this.CheckPreconditions(player, item))
        {
            return false;
        }

        await this.ConsumeSourceItemAsync(player, item).ConfigureAwait(false);
        await player.AddRuudAsync(reward.Amount).ConfigureAwait(false);
        return true;
    }
}
