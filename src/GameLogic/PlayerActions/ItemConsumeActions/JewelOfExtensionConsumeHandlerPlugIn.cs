// <copyright file="JewelOfExtensionConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The consume handler for the jewel of extension, which repairs a lucky item (<see cref="LuckyItemExtensions.IsLuckyItem"/>)
/// to its full durability. A lucky item can't be repaired otherwise.
/// </summary>
/// <remarks>
/// As in the original, a lucky item which wore down completely can't be repaired anymore.
/// </remarks>
[Guid("CB33AA3A-3AA9-4606-BF01-30EB68324E82")]
[PlugIn]
[Display(Name = nameof(PlugInResources.JewelOfExtensionConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.JewelOfExtensionConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class JewelOfExtensionConsumeHandlerPlugIn : ItemModifyConsumeHandlerPlugIn
{
    /// <inheritdoc />
    public override ItemIdentifier Key => ItemConstants.JewelOfExtension;

    /// <inheritdoc />
    public override ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        if (targetItem?.Definition?.IsLuckyItem(player.GameContext.Configuration) is not true)
        {
            return ValueTask.FromResult(false);
        }

        return base.ConsumeItemAsync(player, item, targetItem, fruitUsage);
    }

    /// <inheritdoc />
    protected override bool ModifyItem(Item item, IContext persistenceContext)
    {
        var maximumDurability = item.GetMaximumDurabilityOfOnePiece();
        if (item.Durability <= 0 || item.Durability >= maximumDurability)
        {
            return false;
        }

        item.Durability = maximumDurability;
        return true;
    }
}
