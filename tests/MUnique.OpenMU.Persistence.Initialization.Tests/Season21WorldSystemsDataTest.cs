// <copyright file="Season21WorldSystemsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the optional updates of the world systems of Season 21: mining, golden invasions, hunting record and ranking,
/// monster soul converter, helper plus and the season pass.
/// </summary>
[TestFixture]
internal class Season21WorldSystemsDataTest
{
    /// <summary>
    /// The mining update places the 51 spots of the six maps, configures the stages of the data and switches mining on.
    /// </summary>
    [Test]
    public async Task MiningUpdatePlacesTheSpotsAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21MiningUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var spots = gameConfiguration.Maps.SelectMany(m => m.MonsterSpawns.Where(s => s.MonsterDefinition?.Number is >= 605 and <= 607).Select(s => m.Number)).ToList();
        Assert.That(spots, Has.Count.EqualTo(51));
        Assert.That(spots.Count(m => m == 8), Is.EqualTo(12), "Tarkan.");
        Assert.That(spots.Count(m => m == 123), Is.EqualTo(9), "Kubera Mine 1.");
        Assert.That(gameConfiguration.Monsters.Single(m => m.Number == 607).ObjectKind, Is.EqualTo(NpcObjectKind.PassiveNpc));

        var configuration = GetConfiguration<MiningConfiguration>(gameConfiguration, typeof(MiningFeaturePlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Spots.Select(s => s.NpcNumber), Is.EqualTo(new short[] { 605, 606, 607 }));
        var large = configuration.Spots[2];
        Assert.That(large.Stages.Select(s => s.ItemCount), Is.EqualTo(new[] { 4, 12, 29, 65, 125 }));
        Assert.That((large.Stages[4].BonusRate, large.Stages[4].BonusCount, large.Stages[0].DurabilityCost, large.Stages[0].SuccessRate), Is.EqualTo((180, 1000, (byte)8, 4750)));

        var pick = gameConfiguration.Monsters.Single(m => m.Number == 578).MerchantStore!.Items.Single(i => i.Definition is { Group: 0, Number: 41 });
        Assert.That(pick.Durability, Is.EqualTo(pick.Definition!.Durability), "Bolo sells the pickaxe at its full durability.");
    }

    /// <summary>
    /// Applied twice, the mining update changes nothing and keeps a switch the operator turned off; it is optional after the shops.
    /// </summary>
    [Test]
    public async Task MiningUpdateTwiceChangesNothingAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21MiningUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(MiningFeaturePlugIn).GUID);
        plugIn.IsActive = false;
        var before = (plugIn.CustomConfiguration, gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count));

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That((plugIn.CustomConfiguration, gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count)), Is.EqualTo(before));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(update.IsMandatory, Is.False);
        Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21ShopsUpdatePlugIn().Key));
    }

    private static T GetConfiguration<T>(GameConfiguration gameConfiguration, Guid typeId, out PlugInConfiguration plugIn)
        where T : class
    {
        plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeId);
        return plugIn.GetConfiguration<T>(new IdIgnoringReferenceHandler())!;
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21ShopsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
