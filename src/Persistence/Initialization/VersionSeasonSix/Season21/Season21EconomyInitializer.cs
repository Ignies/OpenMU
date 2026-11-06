// <copyright file="Season21EconomyInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.GremoryCase;
using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.GameLogic.Ruud;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence.Initialization.Properties;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The gremory case, Ruud and the Ruud shop of Season 21: Priest James in Elbeland and Loren Market,
/// the configuration of the Ruud shop and of the Ruud sources, and the plugins switched on.
/// </summary>
internal class Season21EconomyInitializer
{
    /// <summary>
    /// The number of Priest James, who keeps the Ruud shop.
    /// </summary>
    public const short PriestJames = 725;

    private const short FirstSpawnNumber = 7250;

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21EconomyInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21EconomyInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Applies the data. Existing rows and a configuration the operator changed stay.
    /// </summary>
    public void Initialize()
    {
        this.PlacePriestJames();

        // The first application turns the features on; later ones leave the operator's switches and rows alone.
        if (this.ConfigureRuud())
        {
            this.Activate<GremoryCaseConfiguration>(typeof(GremoryCaseFeaturePlugIn).GUID);
            this.Activate<ItemExpiryConfiguration>(typeof(ItemExpiryPlugIn).GUID);
        }
    }

    private static RuudConfiguration CreateRuudConfiguration()
    {
        var data = Season21Economy.Data;
        return new RuudConfiguration
        {
            ShopNpcNumber = PriestJames,
            ShopEntries = data.RuudShop.Select(row => new RuudShopEntry
            {
                Class = row.Class is { } baseClass ? (BaseClassCode)baseClass : null,
                Group = row.Group,
                Number = row.Number,
                Level = row.Level,
                Count = row.Count,
                LuckChance = row.LuckChance,
                OptionChance = row.OptionChance,
                OptionLevel = row.OptionLevel,
                Price = row.Price,
            }).ToList(),
            MonsterRewards = data.RuudMonsters.Select(row => new RuudMonsterReward { MonsterNumber = row.Monster, Minimum = row.Minimum, Maximum = row.Maximum }).ToList(),
            BoxRewards = data.RuudBoxes.Select(row => new RuudBoxReward { Group = row.Group, Number = row.Number, Amount = row.Amount }).ToList(),
        };
    }

    private void PlacePriestJames()
    {
        // The same places as the field bosses' update (the spawn files of the data); a place which has him already stays.
        var number = FirstSpawnNumber;
        foreach (var (map, npc, x, y, direction) in Season21FieldBossData.Npcs.Where(n => n.Npc == PriestJames))
        {
            Season21NpcSpawns.Place(this._context, this._gameConfiguration, ++number, map, npc, LocalizedString.FromResource(() => MonsterNames.PriestJames), x, y, direction);
        }
    }

    private bool ConfigureRuud()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<RuudConfiguration>(this._context, this._gameConfiguration, typeof(RuudFeaturePlugIn).GUID, out var plugIn);
        if (configuration is null
            || configuration.ShopEntries.Count > 0 || configuration.MonsterRewards.Count > 0 || configuration.BoxRewards.Count > 0)
        {
            return false;
        }

        plugIn.SetConfiguration(CreateRuudConfiguration(), null);
        plugIn.IsActive = true;
        return true;
    }

    private void Activate<T>(Guid typeId)
        where T : class, new()
    {
        if (Season21PlugInConfigurations.GetOrCreate<T>(this._context, this._gameConfiguration, typeId, out var plugIn) is not { } configuration)
        {
            return;
        }

        if (string.IsNullOrEmpty(plugIn.CustomConfiguration))
        {
            plugIn.SetConfiguration(configuration, null);
        }

        plugIn.IsActive = true;
    }
}
