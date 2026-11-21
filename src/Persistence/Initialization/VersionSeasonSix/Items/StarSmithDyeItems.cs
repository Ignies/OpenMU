// <copyright file="StarSmithDyeItems.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Items;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Persistence.Initialization.Items;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// The dyes a Star Smith's recolour spends (GameLogic/StarSmithing/Readme.md "Dyes"): the items of
/// <see cref="StarSmithingConfiguration.DefaultDyes"/>, group 14 from number 1000, the eight basic ones on sale at Hanzo,
/// the rare, Prismatic and Radiant ones dropped by monsters from level 60 on every map.
/// </summary>
internal class StarSmithDyeItems : InitializerBase
{
    /// <summary>The item group of the dyes, the potions and other small items.</summary>
    internal const byte ItemGroup = 14;

    /// <summary>The number of the first dye.</summary>
    internal const short FirstNumber = 1000;

    /// <summary>The number of the last dye, the Radiant Dye.</summary>
    internal const short LastNumber = 1014;

    /// <summary>The first number past the basic dyes (Crimson to Ebony), which are sold and never drop.</summary>
    internal const short FirstDroppedNumber = 1008;

    /// <summary>The monster level from which the rare, Prismatic and Radiant dyes drop.</summary>
    internal const byte DropMonsterLevel = 60;

    /// <summary>How many dyes stack on one square.</summary>
    internal const byte MaximumStack = 20;

    /// <summary>The chance of the drop group: one kill in 2,500 from level 60.</summary>
    private const double DropChance = 0.0004;

    private const string DropGroupName = "Star Smith dyes";

    private static readonly string[] Names =
    [
        "Crimson Dye", "Amber Dye", "Gold Dye", "Verdant Dye", "Azure Dye", "Violet Dye", "Ivory Dye", "Ebony Dye",
        "Bloodmoon Dye", "Frostfire Dye", "Starlight Dye", "Abyssal Dye", "Royal Dye", "Prismatic Dye", "Radiant Dye",
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="StarSmithDyeItems"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public StarSmithDyeItems(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    /// <remarks>It runs after the NPCs and maps are created: Hanzo sells the basic dyes, every map drops the others.</remarks>
    public override void Initialize()
    {
        Apply(this.Context, this.GameConfiguration);
    }

    /// <summary>
    /// Creates the dyes, Hanzo's offers and the drop group, each if it doesn't exist yet.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    internal static void Apply(IContext context, GameConfiguration gameConfiguration)
    {
        var dyes = new List<ItemDefinition>();
        for (var number = FirstNumber; number <= LastNumber; number++)
        {
            var n = number;
            dyes.Add(gameConfiguration.Items.FirstOrDefault(item => item.Group == ItemGroup && item.Number == n) ?? CreateDye(context, gameConfiguration, n));
        }

        foreach (var dye in dyes.Where(d => d.Number < FirstDroppedNumber))
        {
            AddToHanzo(context, gameConfiguration, dye);
        }

        AddDropGroup(context, gameConfiguration, dyes.Where(d => d.Number >= FirstDroppedNumber).ToList());
    }

    /// <summary>
    /// Determines whether a definition is one of the dyes.
    /// </summary>
    /// <param name="definition">The item definition.</param>
    /// <returns><c>true</c> if it's a dye.</returns>
    internal static bool IsDye(ItemDefinition definition) => definition.Group == ItemGroup && definition.Number is >= FirstNumber and <= LastNumber;

    private static ItemDefinition CreateDye(IContext context, GameConfiguration gameConfiguration, short number)
    {
        var dropped = number >= FirstDroppedNumber;
        var dye = context.CreateNew<ItemDefinition>();
        dye.Group = ItemGroup;
        dye.Number = number;
        dye.Name = Names[number - FirstNumber];
        dye.Width = 1;
        dye.Height = 1;
        dye.Durability = MaximumStack;
        dye.DropsFromMonsters = dropped;
        dye.DropLevel = dropped ? DropMonsterLevel : (byte)1;
        dye.MaximumItemLevel = 0;
        dye.Value = dropped ? 2_000 : 120;
        dye.SetGuid(dye.Group, dye.Number);
        gameConfiguration.Items.Add(dye);
        new ItemPriceDefinitions(context, gameConfiguration).AssignPrice(dye);
        return dye;
    }

    private static void AddToHanzo(IContext context, GameConfiguration gameConfiguration, ItemDefinition dye)
    {
        if (gameConfiguration.Monsters.FirstOrDefault(monster => monster.Number == StarSmithHammer.HanzoNumber) is not { } hanzo)
        {
            return;
        }

        var store = hanzo.MerchantStore ??= context.CreateNew<ItemStorage>();
        if (store.Items.Any(item => item.Definition == dye))
        {
            return;
        }

        var grid = new bool[Season21ShopsInitializer.Columns, Season21ShopsInitializer.Rows];
        foreach (var item in store.Items.Where(item => item.Definition is not null))
        {
            Occupy(grid, item);
        }

        if (Season21ShopsInitializer.FindPlace(grid, dye) is not { } slot)
        {
            return;
        }

        var offer = context.CreateNew<Item>();
        offer.Definition = dye;
        offer.ItemSlot = slot;
        offer.Durability = 1;
        store.Items.Add(offer);
    }

    private static void AddDropGroup(IContext context, GameConfiguration gameConfiguration, List<ItemDefinition> dropped)
    {
        var group = gameConfiguration.DropItemGroups.FirstOrDefault(g => g.PossibleItems.Any(IsDye));
        if (group is null)
        {
            group = context.CreateNew<DropItemGroup>();
            group.SetGuid(ItemGroup, FirstDroppedNumber);
            group.Description = DropGroupName;
            group.Chance = DropChance;
            group.MinimumMonsterLevel = DropMonsterLevel;
            group.ItemType = SpecialItemType.None;
            foreach (var dye in dropped)
            {
                group.PossibleItems.Add(dye);
            }

            gameConfiguration.DropItemGroups.Add(group);
        }

        foreach (var map in gameConfiguration.Maps.Where(map => !map.DropItemGroups.Contains(group)))
        {
            map.DropItemGroups.Add(group);
        }
    }

    private static void Occupy(bool[,] grid, Item item)
    {
        var columns = grid.GetLength(0);
        var rows = grid.GetLength(1);
        var x = item.ItemSlot % columns;
        var y = item.ItemSlot / columns;
        for (var dy = 0; dy < Math.Max((int)item.Definition!.Height, 1) && y + dy < rows; dy++)
        {
            for (var dx = 0; dx < Math.Max((int)item.Definition.Width, 1) && x + dx < columns; dx++)
            {
                grid[x + dx, y + dy] = true;
            }
        }
    }
}
