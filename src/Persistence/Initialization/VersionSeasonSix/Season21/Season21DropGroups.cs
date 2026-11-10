// <copyright file="Season21DropGroups.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.Interfaces;

/// <summary>
/// Gives each monster of the <see cref="Season21MapData"/> its drop list of <see cref="Season21DropData"/>, as drop
/// groups of the monster: one per drop weight, whose chances share the chance of a random item drop by their weights.
/// </summary>
/// <remarks>
/// Being groups of the monster, they aren't filtered by the monster level, which is above any drop level on the later
/// maps. Only the monsters of the data get them (the map sets' and the zone elites of <see cref="Season21EliteZoneData"/>),
/// so a Season 6 monster which a later map spawns keeps its drops.
/// Items which the configuration doesn't have are left out; applying it again after more items were defined adds them
/// to the groups. A group's chance counts the active items only, as only these drop; it's set again when items join,
/// unless the operator changed it.
/// </remarks>
internal sealed class Season21DropGroups
{
    /// <summary>
    /// The chance of a random item drop of a monster, which the drop list of a monster shares, as the common random
    /// item drop group of the Season 6 data.
    /// </summary>
    private const double RandomItemDropChance = 0.3;

    /// <summary>
    /// The offset of the parent numbers of the guids of the drop groups, beyond the numbers of the Season 6 groups.
    /// </summary>
    private const short DropGroupNumberOffset = 2100;

    /// <summary>
    /// The difference below which a chance counts as unchanged.
    /// </summary>
    private const double ChanceTolerance = 1e-9;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21DropGroups"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21DropGroups(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Creates the drop groups of the monsters which the configuration has, or adds the newly defined items to them.
    /// </summary>
    public void Apply()
    {
        var items = this._gameConfiguration.Items.ToDictionary(KeyOf);
        var ownMonsters = Season21MapData.All.SelectMany(set => set.Monsters)
            .Concat(Season21EliteZoneData.Monsters.Select(elite => elite.Row))
            .Select(row => row.Number)
            .ToHashSet();
        short listNumber = 0;
        foreach (var list in Season21DropData.All)
        {
            listNumber++;
            var monsters = this._gameConfiguration.Monsters.Where(m => ownMonsters.Contains(m.Number) && list.Monsters.Contains(m.Number)).ToList();
            if (monsters.Count > 0)
            {
                this.Apply(list, (short)(DropGroupNumberOffset + listNumber), items, monsters);
            }
        }
    }

    private static short KeyOf(ItemDefinition item) => (short)((item.Group * 512) + item.Number);

    private static (short Parent, short Number)? NumbersOf(DropItemGroup group)
    {
        if (group is not IIdentifiable { Id: var id })
        {
            return null;
        }

        var bytes = id.ToByteArray();
        var (parent, number) = (BitConverter.ToInt16(bytes, 4), BitConverter.ToInt16(bytes, 6));
        return id == GuidHelper.CreateGuid<DropItemGroup>(parent, number) ? (parent, number) : null;
    }

    private static int ActiveCount(IEnumerable<ItemDefinition> items) => items.Count(item => item.IsActive);

    private static double ChanceOf(int weight, int activeItems, double totalWeight)
        => totalWeight > 0 ? RandomItemDropChance * weight * activeItems / totalWeight : 0;

    private void Apply(Season21DropList list, short guidParent, Dictionary<short, ItemDefinition> items, List<MonsterDefinition> monsters)
    {
        var existing = this._gameConfiguration.DropItemGroups.Where(g => NumbersOf(g)?.Parent == guidParent).ToList();
        var unclaimed = existing.ToList();
        var buckets = new List<(int Weight, List<ItemDefinition> Items, DropItemGroup? Group)>();
        foreach (var bucket in list.Buckets)
        {
            var bucketItems = bucket.Items.Select(i => items.GetValueOrDefault(i)).OfType<ItemDefinition>().ToList();
            if (bucketItems.Count == 0)
            {
                continue;
            }

            var keys = bucket.Items.ToHashSet();
            var group = unclaimed.FirstOrDefault(g => g.PossibleItems.Count > 0 && g.PossibleItems.All(i => keys.Contains(KeyOf(i))));
            if (group is not null)
            {
                unclaimed.Remove(group);
            }

            buckets.Add((bucket.Weight, bucketItems, group));
        }

        var oldTotal = buckets.Where(b => b.Group is not null).Sum(b => (double)b.Weight * ActiveCount(b.Group!.PossibleItems));
        var newTotal = buckets.Sum(b => (double)b.Weight * ActiveCount(b.Items));
        var usedNumbers = existing.Select(g => NumbersOf(g)!.Value.Number).ToHashSet();
        short nextNumber = 1;
        foreach (var (weight, bucketItems, found) in buckets)
        {
            var group = found;
            if (group is null)
            {
                while (usedNumbers.Contains(nextNumber))
                {
                    nextNumber++;
                }

                usedNumbers.Add(nextNumber);
                group = this._context.CreateNew<DropItemGroup>();
                group.SetGuid(guidParent, nextNumber);
                group.Description = LocalizedString.FromResource(() => DropGroupDescriptions.Season21MonsterDrops);
                group.ItemType = SpecialItemType.RandomItem;
                group.Chance = ChanceOf(weight, ActiveCount(bucketItems), newTotal);
                this._gameConfiguration.DropItemGroups.Add(group);
            }
            else if (Math.Abs(group.Chance - ChanceOf(weight, ActiveCount(group.PossibleItems), oldTotal)) < ChanceTolerance)
            {
                group.Chance = ChanceOf(weight, ActiveCount(bucketItems), newTotal);
            }

            foreach (var item in bucketItems.Where(i => !group.PossibleItems.Contains(i)))
            {
                group.PossibleItems.Add(item);
            }

            foreach (var monster in monsters.Where(m => !m.DropItemGroups.Contains(group)))
            {
                monster.DropItemGroups.Add(group);
            }
        }
    }
}
