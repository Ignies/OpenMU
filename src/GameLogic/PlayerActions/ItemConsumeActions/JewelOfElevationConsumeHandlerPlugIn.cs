// <copyright file="JewelOfElevationConsumeHandlerPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The consume handler for the jewel of elevation, which gives a lucky item (<see cref="LuckyItemExtensions.IsLuckyItem"/>)
/// a harmony option, as a jewel of harmony would, but always successfully.
/// </summary>
[Guid("98F8942F-B692-4621-BCB4-4D945B1E852D")]
[PlugIn]
[Display(Name = nameof(PlugInResources.JewelOfElevationConsumeHandlerPlugIn_Name), Description = nameof(PlugInResources.JewelOfElevationConsumeHandlerPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class JewelOfElevationConsumeHandlerPlugIn : ItemUpgradeConsumeHandlerPlugIn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JewelOfElevationConsumeHandlerPlugIn" /> class.
    /// </summary>
    public JewelOfElevationConsumeHandlerPlugIn()
        : base(new ItemUpgradeConfiguration(ItemOptionTypes.HarmonyOption, true, false, 1.0, ItemFailResult.None))
    {
    }

    /// <inheritdoc />
    public override ItemIdentifier Key => ItemConstants.JewelOfElevation;

    /// <inheritdoc />
    public override ValueTask<bool> ConsumeItemAsync(Player player, Item item, Item? targetItem, FruitUsage fruitUsage)
    {
        if (targetItem?.Definition?.IsLuckyItem(player.GameContext.Configuration) is not true)
        {
            return ValueTask.FromResult(false);
        }

        return base.ConsumeItemAsync(player, item, targetItem, fruitUsage);
    }
}
