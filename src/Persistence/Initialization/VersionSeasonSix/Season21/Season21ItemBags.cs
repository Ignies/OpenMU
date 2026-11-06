// <copyright file="Season21ItemBags.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.Interfaces;

/// <summary>
/// Creates the drop item groups of the item bags of the Season 21 events (S21Emu <c>EventItemBag</c>): one group per bag,
/// one random item of it. An item is given as <c>group * 512 + number</c>, as in the data.
/// </summary>
internal static class Season21ItemBags
{
    /// <summary>
    /// The first number of the groups: a bag is <c>(BagNumberOffset, bag)</c>, apart from the field boss groups (2200+).
    /// </summary>
    private const short BagNumberOffset = 2121;

    /// <summary>
    /// Gets or creates the group of a bag, with the items of the catalog which exist.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    /// <param name="bag">The number of the bag.</param>
    /// <param name="items">The items, as <c>group * 512 + number</c>.</param>
    /// <returns>The group.</returns>
    public static DropItemGroup GetOrCreate(IContext context, GameConfiguration gameConfiguration, short bag, IEnumerable<int> items)
    {
        var id = GuidHelper.CreateGuid<DropItemGroup>(BagNumberOffset, bag);
        if (gameConfiguration.DropItemGroups.FirstOrDefault(g => g.GetId() == id) is not { } group)
        {
            group = context.CreateNew<DropItemGroup>();
            group.SetGuid(BagNumberOffset, bag);
            group.Description = LocalizedString.FromResource(() => DropGroupDescriptions.Season21EventRewards);
            group.ItemType = SpecialItemType.RandomItem;
            group.Chance = 1.0;
            gameConfiguration.DropItemGroups.Add(group);
        }

        foreach (var code in items)
        {
            if (gameConfiguration.Items.FirstOrDefault(i => i.Group == code / 512 && i.Number == code % 512) is { } item
                && !group.PossibleItems.Contains(item))
            {
                group.PossibleItems.Add(item);
            }
        }

        return group;
    }
}
