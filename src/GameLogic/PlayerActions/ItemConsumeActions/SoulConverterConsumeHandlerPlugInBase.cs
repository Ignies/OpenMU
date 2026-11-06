// <copyright file="SoulConverterConsumeHandlerPlugInBase.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using MUnique.OpenMU.GameLogic.SoulConverter;

/// <summary>
/// Uses a monster soul converter: the <see cref="SoulConverterPlugIn"/> activates it, then the item is used up.
/// </summary>
public abstract class SoulConverterConsumeHandlerPlugInBase : BaseConsumeHandlerPlugIn
{
    /// <inheritdoc />
    public override async ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        if (SoulConverterPlugIn.Get(player.GameContext) is not { } plugIn
            || plugIn.FindTier(item) is not { } tier
            || !this.CheckPreconditions(player, item))
        {
            return false;
        }

        switch (await plugIn.ActivateAsync(player, item).ConfigureAwait(false))
        {
            case SoulConverterActivation.Activated:
                await this.ConsumeSourceItemAsync(player, item).ConfigureAwait(false);
                return true;
            case SoulConverterActivation.LevelTooLow:
                await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.SoulConverterLevelTooLow), tier.MinimumTotalLevel).ConfigureAwait(false);
                return false;
            case SoulConverterActivation.AlreadyActive:
                await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.SoulConverterAlreadyActive)).ConfigureAwait(false);
                return false;
            case SoulConverterActivation.WeeklyLimitReached:
                await player.ShowLocalizedBlueMessageAsync(nameof(PlayerMessage.SoulConverterWeeklyLimitReached)).ConfigureAwait(false);
                return false;
            default:
                return false;
        }
    }
}
