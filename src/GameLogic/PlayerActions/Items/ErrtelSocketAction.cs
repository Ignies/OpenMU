// <copyright file="ErrtelSocketAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Items;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Elements;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Puts an errtel into a socket of a pentagram, or takes it out (see Elements/Readme.md, "Pentagrams and errtels").
/// </summary>
public class ErrtelSocketAction
{
    private const int RateBasis = 10_000;

    /// <summary>
    /// Handles an errtel socket request.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="pentagramSlot">The inventory slot of the pentagram.</param>
    /// <param name="errtelSlot">The inventory slot of the errtel to insert.</param>
    /// <param name="socket">The socket.</param>
    /// <returns>The result.</returns>
    public async ValueTask<ErrtelSocketResult> HandleAsync(Player player, ErrtelSocketMode mode, byte pentagramSlot, byte errtelSlot, byte socket)
    {
        var result = ErrtelSocketResult.Refused;
        if (player.PlayerState.CurrentState == PlayerState.EnteredWorld
            && ElementsFeaturePlugIn.GetConfiguration(player.GameContext) is { } configuration
            && player.Inventory?.GetItem(pentagramSlot) is { } pentagram
            && pentagram.IsPentagram()
            && socket < Math.Min(pentagram.SocketCount, Pentagrams.Sockets))
        {
            result = mode switch
            {
                ErrtelSocketMode.Insert => await InsertAsync(player, pentagram, errtelSlot, socket).ConfigureAwait(false),
                ErrtelSocketMode.Remove => await RemoveAsync(player, pentagram, socket, configuration).ConfigureAwait(false),
                _ => ErrtelSocketResult.Refused,
            };
        }

        await player.InvokeViewPlugInAsync<IPentagramViewPlugIn>(p => p.ShowErrtelSocketResultAsync(mode, result, pentagramSlot, socket)).ConfigureAwait(false);
        return result;
    }

    private static async ValueTask PentagramChangedAsync(Player player, Item pentagram)
    {
        if (ElementsFeaturePlugIn.GetConfiguration(player.GameContext) is { } configuration)
        {
            pentagram.UpdateConditionalOptions(configuration, player.PersistenceContext);
        }

        if (InventoryConstants.IsEquippedSlot(pentagram.ItemSlot) && player.Inventory?.AsInventoryStorage is { } inventory)
        {
            await inventory.RaiseEquippedItemsChangedAsync(pentagram, true).ConfigureAwait(false);
        }

        await player.InvokeViewPlugInAsync<IItemUpgradedPlugIn>(p => p.ItemUpgradedAsync(pentagram)).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IPentagramViewPlugIn>(p => p.ShowPentagramErrtelsAsync(pentagram)).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IElementInfoViewPlugIn>(p => p.ShowElementInfoAsync()).ConfigureAwait(false);
    }

    private static async ValueTask<ErrtelSocketResult> InsertAsync(Player player, Item pentagram, byte errtelSlot, byte socket)
    {
        if (player.Inventory!.GetItem(errtelSlot) is not { Definition: { } definition } errtel
            || Pentagrams.GetErrtelSocket(definition) != socket
            || errtel.GetElement() != pentagram.GetElement()
            || pentagram.GetRankOptions(socket).Any())
        {
            return ErrtelSocketResult.Refused;
        }

        foreach (var errtelLink in errtel.GetErrtelLinks().ToList())
        {
            AddLink(player, pentagram, errtelLink.ItemOption, errtelLink.Level, socket);
        }

        await player.DestroyInventoryItemAsync(errtel).ConfigureAwait(false);
        await PentagramChangedAsync(player, pentagram).ConfigureAwait(false);
        return ErrtelSocketResult.Success;
    }

    private static async ValueTask<ErrtelSocketResult> RemoveAsync(Player player, Item pentagram, byte socket, ElementsConfiguration configuration)
    {
        var rankOptions = pentagram.GetRankOptions(socket).ToList();
        if (rankOptions.Count == 0
            || player.GameContext.Configuration.FindErrtelDefinition(rankOptions[0].ItemOption) is not { } definition)
        {
            return ErrtelSocketResult.Refused;
        }

        var errtel = player.PersistenceContext.CreateNew<Item>();
        errtel.Definition = definition;
        errtel.Durability = errtel.GetMaximumDurabilityOfOnePiece();
        if (player.Inventory!.CheckInvSpace(errtel) is not { } freeSlot)
        {
            player.PersistenceContext.Detach(errtel);
            return ErrtelSocketResult.Refused;
        }

        var setLinks = pentagram.GetErrtelLinks(socket).Except(rankOptions).ToList();
        rankOptions.Concat(setLinks).ToList().ForEach(link => pentagram.ItemOptions.Remove(link));
        var rate = configuration.RemovalRates.FirstOrDefault(r => r.Level == rankOptions.GetErrtelLevel())?.Rates.ElementAtOrDefault(rankOptions.Count - 1) ?? RateBasis;
        if (Rand.NextInt(0, RateBasis) >= rate)
        {
            player.PersistenceContext.Detach(errtel);
            await PentagramChangedAsync(player, pentagram).ConfigureAwait(false);
            return ErrtelSocketResult.Failed;
        }

        AddLink(player, errtel, pentagram.ItemOptions.FirstOrDefault(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.Element)?.ItemOption, 0, 0);
        foreach (var rankOption in rankOptions)
        {
            AddLink(player, errtel, rankOption.ItemOption, rankOption.Level, rankOption.ItemOption!.SubOptionType - 1);
        }

        setLinks.ForEach(link => AddLink(player, errtel, link.ItemOption, 0, 0));

        await player.Inventory.AddItemAsync(freeSlot, errtel).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<IItemAppearPlugIn>(p => p.ItemAppearAsync(errtel)).ConfigureAwait(false);
        await PentagramChangedAsync(player, pentagram).ConfigureAwait(false);
        return ErrtelSocketResult.Success;
    }

    private static void AddLink(Player player, Item item, IncreasableItemOption? option, int level, int index)
    {
        if (option is null)
        {
            return;
        }

        var link = player.PersistenceContext.CreateNew<ItemOptionLink>();
        link.ItemOption = option;
        link.Level = level;
        link.Index = index;
        item.ItemOptions.Add(link);
    }
}
