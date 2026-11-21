// <copyright file="StarSmithDyesDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// The Star Smith dyes (GameLogic/StarSmithing/Readme.md "Dyes"): fifteen items of group 14 from number 1000, the eight
/// basic ones sold by Hanzo, the rare, Prismatic and Radiant ones dropped by monsters from level 60 on every map.
/// </summary>
[TestFixture]
internal class StarSmithDyesDataTest
{
    private const short HanzoNumber = 251;
    private const string UpdateTypeName = "MUnique.OpenMU.Persistence.Initialization.Updates.AddStarSmithDyesUpdatePlugIn, MUnique.OpenMU.Persistence.Initialization";

    private static readonly string[] Names =
    [
        "Crimson Dye", "Amber Dye", "Gold Dye", "Verdant Dye", "Azure Dye", "Violet Dye", "Ivory Dye", "Ebony Dye",
        "Bloodmoon Dye", "Frostfire Dye", "Starlight Dye", "Abyssal Dye", "Royal Dye", "Prismatic Dye", "Radiant Dye",
    ];

    /// <summary>
    /// A new database has the dyes, the ones of the configuration's defaults, Hanzo's basic dyes and the drop group.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async Task NewDatabaseHasTheDyesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        AssertDyes(gameConfiguration);
    }

    /// <summary>
    /// The update adds what is missing, once.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async Task UpdateAddsTheDyesOnceAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        var store = GetHanzo(gameConfiguration).MerchantStore!;
        foreach (var offer in store.Items.Where(i => i.Definition is { } d && IsDye(d)).ToList())
        {
            store.Items.Remove(offer);
        }

        foreach (var map in gameConfiguration.Maps)
        {
            foreach (var group in map.DropItemGroups.Where(IsDyeGroup).ToList())
            {
                map.DropItemGroups.Remove(group);
            }
        }

        foreach (var group in gameConfiguration.DropItemGroups.Where(IsDyeGroup).ToList())
        {
            gameConfiguration.DropItemGroups.Remove(group);
        }

        foreach (var definition in gameConfiguration.Items.Where(IsDye).ToList())
        {
            gameConfiguration.Items.Remove(definition);
        }

        var update = CreateUpdate();
        Assert.That(update.IsMandatory, Is.True);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var itemCount = gameConfiguration.Items.Count;
        var groups = gameConfiguration.DropItemGroups.Count;
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        AssertDyes(gameConfiguration);
        Assert.That(gameConfiguration.Items, Has.Count.EqualTo(itemCount));
        Assert.That(gameConfiguration.DropItemGroups, Has.Count.EqualTo(groups));
    }

    /// <summary>
    /// The Season 21 shops, which refill Hanzo's first tab, keep the dyes beside the hammer.
    /// </summary>
    /// <returns>The task.</returns>
    [Test]
    public async Task Season21ShopsKeepTheDyesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6ConfigurationAsync().ConfigureAwait(false);
        using var _ = context;
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ShopsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var store = GetHanzo(gameConfiguration).MerchantStore!;
        Assert.That(store.Items.Count(i => i.Definition is { } d && IsDye(d)), Is.EqualTo(8));
    }

    private static void AssertDyes(GameConfiguration gameConfiguration)
    {
        var dyes = gameConfiguration.Items.Where(IsDye).OrderBy(d => d.Number).ToList();
        var configured = StarSmithingConfiguration.DefaultDyes();
        var store = GetHanzo(gameConfiguration).MerchantStore!;
        var group = gameConfiguration.DropItemGroups.Single(IsDyeGroup);
        Assert.Multiple(() =>
        {
            Assert.That(dyes.Select(d => d.Number), Is.EqualTo(configured.Select(d => d.Number)));
            Assert.That(dyes.Select(d => d.Name.ValueInNeutralLanguage), Is.EqualTo(Names));
            Assert.That(dyes.All(d => d is { Width: 1, Height: 1, Durability: 20 }), Is.True, "1x1, stacking to 20");
            Assert.That(dyes.Where(d => d.Number < 1008).All(d => !d.DropsFromMonsters), Is.True, "the basic dyes are bought");
            Assert.That(dyes.Where(d => d.Number >= 1008).All(d => d.DropsFromMonsters), Is.True, "the others drop");
            Assert.That(dyes.All(d => d.PriceDefinition is not null || d.Value > 0), Is.True);

            var sold = store.Items.Where(i => i.Definition is { } d && IsDye(d)).Select(i => i.Definition!.Number).OrderBy(n => n);
            Assert.That(sold, Is.EqualTo(Enumerable.Range(1000, 8).Select(n => (short)n)));
            Assert.That(store.Items.Where(i => i.Definition is { } d && IsDye(d)).All(i => i.Durability == 1), Is.True);

            Assert.That(group.MinimumMonsterLevel, Is.EqualTo(60));
            Assert.That(group.PossibleItems.Select(d => d.Number).OrderBy(n => n), Is.EqualTo(Enumerable.Range(1008, 7).Select(n => (short)n)));
            Assert.That(gameConfiguration.Maps.All(m => m.DropItemGroups.Contains(group)), Is.True, "every map");
        });
    }

    private static bool IsDye(ItemDefinition definition) => definition.Group == 14 && definition.Number is >= 1000 and <= 1014;

    private static bool IsDyeGroup(DropItemGroup group) => group.PossibleItems.Any(IsDye);

    private static MonsterDefinition GetHanzo(GameConfiguration gameConfiguration)
        => gameConfiguration.Monsters.Single(monster => monster.Number == HanzoNumber);

    private static UpdatePlugInBase CreateUpdate()
    {
        var type = Type.GetType(UpdateTypeName);
        Assert.That(type, Is.Not.Null, "The update AddStarSmithDyesUpdatePlugIn is missing.");
        return (UpdatePlugInBase)Activator.CreateInstance(type!)!;
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason6ConfigurationAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);

        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
