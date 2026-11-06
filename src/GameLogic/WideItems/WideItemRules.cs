// <copyright file="WideItemRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.WideItems;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Views.Inventory;

/// <summary>
/// Decides which items only the wide item format (W32) carries, and whether a player's client can see an item.
/// See Readme.md.
/// </summary>
public static class WideItemRules
{
    /// <summary>
    /// The fork feature id of the per-client item visibility.
    /// </summary>
    public const string FeatureId = "wide_items";

    /// <summary>
    /// The highest item group of the Season 6 item format (4 bits).
    /// </summary>
    internal const byte MaximumSeason6Group = 15;

    /// <summary>
    /// The highest item number of the Season 6 item format (9 bits).
    /// </summary>
    internal const short MaximumSeason6Number = 511;

    /// <summary>
    /// The highest sphere level of the Season 6 item format (socket byte = level index * 50 + option).
    /// </summary>
    internal const int MaximumSeason6SphereLevel = 5;

    /// <summary>
    /// Determines whether only the wide item format carries items of the definition.
    /// </summary>
    /// <param name="definition">The item definition.</param>
    /// <returns><c>true</c>, if a Season 6 client has no such item.</returns>
    public static bool IsWideOnly(ItemDefinition definition)
        => definition.Group > MaximumSeason6Group
           || definition.Number > MaximumSeason6Number
           || (definition.PossibleItemOptions is { } options
               && options.Any(o => o.PossibleOptions?.Any(p => p.OptionType == Season21ItemOptionTypes.WideFormat) is true));

    /// <summary>
    /// Determines whether only the wide item format carries the item, by its definition or its options.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c>, if a Season 6 client can't show the item as it is.</returns>
    public static bool IsWideOnly(Item item)
    {
        if (item.Definition is { } definition && IsWideOnly(definition))
        {
            return true;
        }

        if (item.ItemOptions is not { } links)
        {
            return false;
        }

        foreach (var link in links)
        {
            if (link.ItemOption?.OptionType is not { } type)
            {
                continue;
            }

            if (Season21ItemOptionTypes.All.Contains(type)
                || (type == ItemOptionTypes.SocketOption && link.Level > MaximumSeason6SphereLevel))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the client of the player can see the item. A player without a client view (e.g. a bot)
    /// sees every item.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="item">The item.</param>
    /// <returns><c>true</c>, if the item may be sent to the player.</returns>
    public static bool CanSee(this Player player, Item item)
        => player.CarriesWideItems() || !IsWideOnly(item);

    /// <summary>
    /// Determines whether the client of the player can see items of the definition.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="definition">The item definition.</param>
    /// <returns><c>true</c>, if items of the definition may be sent to the player.</returns>
    public static bool CanSee(this Player player, ItemDefinition definition)
        => player.CarriesWideItems() || !IsWideOnly(definition);

    /// <summary>
    /// Determines whether the client of the player carries the wide items.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><c>true</c>, if it carries them or the player has no client view.</returns>
    public static bool CarriesWideItems(this Player player)
        => player.ViewPlugIns.GetPlugIn<IItemFormatViewPlugIn>()?.CarriesWideItems ?? true;
}
