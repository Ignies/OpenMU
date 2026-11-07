// <copyright file="Season21ShopsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Properties;

/// <summary>
/// The NPC shops of Season 21 with up to five tabs: the first tab is the merchant store which every client shows,
/// the others are its <see cref="MonsterDefinition.MerchantStoreTabs"/>.
/// </summary>
internal class Season21ShopsInitializer
{
    /// <summary>
    /// The columns of a store tab, as the client shows it.
    /// </summary>
    internal const int Columns = 8;

    /// <summary>
    /// The rows of a store tab, as the client shows it.
    /// </summary>
    internal const int Rows = 15;

    private const short WizardGillardNumber = 891;

    private const short WizardGillardSpawnNumber = 8910;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21ShopsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21ShopsInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Sets the shops of the merchants which the configuration has. A tab which already has the items of the data stays.
    /// Rows of items which the configuration doesn't have are left out.
    /// </summary>
    /// <returns>The task.</returns>
    public async ValueTask InitializeAsync()
    {
        this.PlaceWizardGillard();
        foreach (var merchantRows in Season21Economy.Data.Shops.GroupBy(row => row.Npc))
        {
            if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == merchantRows.Key) is not { } merchant)
            {
                continue;
            }

            foreach (var tabRows in merchantRows.GroupBy(row => row.Tab))
            {
                var rows = tabRows.Select(row => (Row: row, Definition: this.FindDefinition(row))).Where(r => r.Definition is not null).ToList();
                if (rows.Count > 0)
                {
                    await this.SetTabAsync(merchant, tabRows.Key, rows!).ConfigureAwait(false);
                }
            }
        }
    }

    private static bool Matches(ItemStorage storage, IReadOnlyList<(Season21Economy.ShopRow Row, ItemDefinition Definition)> rows)
    {
        static string Key(ItemDefinition? definition, byte level) => $"{definition?.Group}/{definition?.Number}/{level}";
        var current = storage.Items.Select(i => Key(i.Definition, i.Level)).Order().ToList();
        var wanted = rows.Select(r => Key(r.Definition, r.Row.Level)).Order().ToList();
        return current.SequenceEqual(wanted);
    }

    private static byte? FindPlace(bool[,] grid, ItemDefinition definition)
    {
        var width = Math.Max((int)definition.Width, 1);
        var height = Math.Max((int)definition.Height, 1);
        for (var y = 0; y + height <= Rows; y++)
        {
            for (var x = 0; x + width <= Columns; x++)
            {
                if (!IsFree(grid, x, y, width, height))
                {
                    continue;
                }

                for (var dy = 0; dy < height; dy++)
                {
                    for (var dx = 0; dx < width; dx++)
                    {
                        grid[x + dx, y + dy] = true;
                    }
                }

                return (byte)((y * Columns) + x);
            }
        }

        return null;
    }

    private static bool IsFree(bool[,] grid, int x, int y, int width, int height)
    {
        for (var dy = 0; dy < height; dy++)
        {
            for (var dx = 0; dx < width; dx++)
            {
                if (grid[x + dx, y + dy])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private ItemDefinition? FindDefinition(Season21Economy.ShopRow row)
        => this._gameConfiguration.Items.FirstOrDefault(d => d.Group == row.Group && d.Number == row.Number);

    private async ValueTask SetTabAsync(MonsterDefinition merchant, byte tab, IReadOnlyList<(Season21Economy.ShopRow Row, ItemDefinition Definition)> rows)
    {
        ItemStorage storage;
        if (tab == 0)
        {
            storage = merchant.MerchantStore ??= this._context.CreateNew<ItemStorage>();
        }
        else if (merchant.MerchantStoreTabs.FirstOrDefault(t => t.Index == tab) is { Items: { } items })
        {
            storage = items;
        }
        else
        {
            var newTab = this._context.CreateNew<MerchantStoreTab>();
            newTab.Index = tab;
            newTab.Items = storage = this._context.CreateNew<ItemStorage>();
            merchant.MerchantStoreTabs.Add(newTab);
        }

        if (Matches(storage, rows))
        {
            return;
        }

        foreach (var item in storage.Items.ToList())
        {
            storage.Items.Remove(item);
            await this._context.DeleteAsync(item).ConfigureAwait(false);
        }

        var grid = new bool[Columns, Rows];
        foreach (var (row, definition) in rows)
        {
            if (FindPlace(grid, definition) is not { } slot)
            {
                continue;
            }

            storage.Items.Add(this.CreateItem(row, definition, slot));
        }
    }

    private Item CreateItem(Season21Economy.ShopRow row, ItemDefinition definition, byte slot)
    {
        var item = this._context.CreateNew<Item>();
        item.Definition = definition;
        item.ItemSlot = slot;
        item.Level = row.Level;
        item.Durability = row.Durability > 0 && definition.Durability > 0 ? Math.Min(row.Durability, definition.Durability) : definition.Durability;
        item.HasSkill = row.Skill != 0 && definition.Skill is not null;
        var options = definition.PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToList();
        if (row.Luck != 0 && options.FirstOrDefault(o => o.OptionType == ItemOptionTypes.Luck) is { } luck)
        {
            var link = this._context.CreateNew<ItemOptionLink>();
            link.ItemOption = luck;
            item.ItemOptions.Add(link);
        }

        if (row.Option > 0 && options.FirstOrDefault(o => o.OptionType == ItemOptionTypes.Option) is { } option)
        {
            var link = this._context.CreateNew<ItemOptionLink>();
            link.ItemOption = option;
            link.Level = row.Option;
            item.ItemOptions.Add(link);
        }

        return item;
    }

    /// <summary>
    /// Places Wizard Gillard, whose shop the data has, where S21Emu <c>MonsterSetBase/000 - Lorencia.xml</c> places him.
    /// </summary>
    private void PlaceWizardGillard()
    {
        Season21NpcSpawns.Place(this._context, this._gameConfiguration, WizardGillardSpawnNumber, 0, WizardGillardNumber, LocalizedString.FromResource(() => MonsterNames.WizardGillard), 119, 111, Direction.SouthEast, GameVersion.Season21Episode1);
        if (this._gameConfiguration.Monsters.FirstOrDefault(m => m.Number == WizardGillardNumber) is { NpcWindow: NpcWindow.Undefined } gillard)
        {
            gillard.NpcWindow = NpcWindow.Merchant;
        }
    }
}
