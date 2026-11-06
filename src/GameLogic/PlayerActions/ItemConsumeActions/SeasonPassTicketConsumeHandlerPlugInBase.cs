// <copyright file="SeasonPassTicketConsumeHandlerPlugInBase.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// Uses a ticket of the season pass: the <see cref="SeasonPassPlugIn"/> applies it, then the item is used up.
/// </summary>
public abstract class SeasonPassTicketConsumeHandlerPlugInBase : BaseConsumeHandlerPlugIn
{
    /// <inheritdoc />
    public override async ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        if (SeasonPassPlugIn.Get(player.GameContext) is not { } plugIn
            || !this.CheckPreconditions(player, item)
            || !await this.ApplyAsync(plugIn, player).ConfigureAwait(false))
        {
            return false;
        }

        await this.ConsumeSourceItemAsync(player, item).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Applies the ticket.
    /// </summary>
    /// <param name="plugIn">The season pass.</param>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if the ticket was applied.</returns>
    protected abstract ValueTask<bool> ApplyAsync(SeasonPassPlugIn plugIn, Player player);
}
