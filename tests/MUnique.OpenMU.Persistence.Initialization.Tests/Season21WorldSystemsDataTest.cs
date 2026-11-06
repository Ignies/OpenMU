// <copyright file="Season21WorldSystemsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.HuntingRecord;
using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.GameLogic.PlugIns.InvasionEvents;
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

    /// <summary>
    /// The golden invasion update defines the ten later golden monsters with their Box of Kundun and turns a golden invasion
    /// with the Season 6 roster into the one of the data: 19 kinds, every four hours from 00:25, 10 minutes.
    /// </summary>
    [Test]
    public async Task GoldenInvasionGetsTheRosterOfTheDataAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21GoldenInvasionUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var napin = gameConfiguration.Monsters.Single(m => m.Number == 500);
        Assert.That(napin.Attributes.Single(a => a.AttributeDefinition == Stats.MaximumHealth).Value, Is.EqualTo(77000));
        Assert.That(napin.DropItemGroups.Single().ItemLevel, Is.EqualTo(11), "Box of Kundun +4.");
        Assert.That(Enumerable.Range(493, 10).All(n => gameConfiguration.Monsters.Any(m => m.Number == n)), Is.True);

        var configuration = GetConfiguration<PeriodicInvasionConfiguration>(gameConfiguration, typeof(GoldenInvasionPlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Mobs, Has.Count.EqualTo(19));
        Assert.That(configuration.Mobs.Single(m => m.MonsterId == 79).MapIds, Is.EqualTo(new ushort[] { 0, 2, 3, 51 }), "The Golden Derkon also invades Elbeland.");
        Assert.That(configuration.Mobs.Single(m => m.MonsterId == 501).IsSpawnOnAllMaps, Is.True);
        Assert.That(configuration.Mobs.Single(m => m.MonsterId == 43).Count, Is.EqualTo(5));
        Assert.That(configuration.Timetable, Is.EqualTo(Enumerable.Range(0, 6).Select(i => new TimeOnly(i * 4, 25))));
        Assert.That(configuration.TaskDuration, Is.EqualTo(TimeSpan.FromMinutes(10)));
    }

    /// <summary>
    /// A golden invasion the operator changed keeps its numbers and timetable; only the missing monsters and maps are added.
    /// Applied twice, nothing changes.
    /// </summary>
    [Test]
    public async Task GoldenInvasionKeepsTheOperatorsNumbersAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(GoldenInvasionPlugIn).GUID);
        var custom = plugIn.GetConfiguration<PeriodicInvasionConfiguration>(new IdIgnoringReferenceHandler())!;
        custom.Mobs.Single(m => m.MonsterId == 43).Count = 7;
        custom.Timetable = [new TimeOnly(12, 0)];
        plugIn.SetConfiguration(custom, null);
        var update = new AddSeason21GoldenInvasionUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var configuration = GetConfiguration<PeriodicInvasionConfiguration>(gameConfiguration, typeof(GoldenInvasionPlugIn).GUID, out _);
        Assert.That(configuration.Mobs.Single(m => m.MonsterId == 43).Count, Is.EqualTo(7));
        Assert.That(configuration.Mobs.Single(m => m.MonsterId == 81).Count, Is.EqualTo(20), "The Season 6 count of the operator's roster stays.");
        Assert.That(configuration.Timetable, Is.EqualTo(new[] { new TimeOnly(12, 0) }));
        Assert.That(configuration.Mobs, Has.Count.EqualTo(19));

        var before = (plugIn.CustomConfiguration, gameConfiguration.Monsters.Count, gameConfiguration.DropItemGroups.Count);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That((plugIn.CustomConfiguration, gameConfiguration.Monsters.Count, gameConfiguration.DropItemGroups.Count), Is.EqualTo(before));
        Assert.That(update.IsMandatory, Is.False);
    }

    /// <summary>
    /// The hunting record update sets the 59 maps and the 64 excluded monsters of <c>HuntingRecord.txt</c> and switches the hunting record on.
    /// </summary>
    [Test]
    public async Task HuntingRecordUpdateSetsTheMapsOfTheDataAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21HuntingRecordUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var configuration = GetConfiguration<HuntingRecordConfiguration>(gameConfiguration, typeof(HuntingRecordPlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Maps, Has.Count.EqualTo(59));
        Assert.That(configuration.Maps.Take(4), Is.EqualTo(new short[] { 0, 3, 51, 2 }), "Lorencia, Noria, Elbeland, Davias.");
        Assert.That(configuration.Maps, Does.Contain((short)143).And.Not.Contain((short)9), "Kardamahal counts, Devil Square doesn't.");
        Assert.That(configuration.ExcludedMonsters, Has.Count.EqualTo(64));
        Assert.That(configuration.ExcludedMonsters.Take(2), Is.EqualTo(new short[] { 413, 463 }));
        Assert.That(GameLogic.ForkInfo.Has("ranking"), Is.True);
    }

    /// <summary>
    /// Applied twice, the hunting record update keeps the maps and the switch of the operator; it is optional.
    /// </summary>
    [Test]
    public async Task HuntingRecordUpdateKeepsTheOperatorsMapsAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21HuntingRecordUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(HuntingRecordPlugIn).GUID);
        var custom = plugIn.GetConfiguration<HuntingRecordConfiguration>(new IdIgnoringReferenceHandler())!;
        custom.Maps = [7];
        plugIn.SetConfiguration(custom, null);
        plugIn.IsActive = false;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(GetConfiguration<HuntingRecordConfiguration>(gameConfiguration, typeof(HuntingRecordPlugIn).GUID, out _).Maps, Is.EqualTo(new short[] { 7 }));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(update.IsMandatory, Is.False);
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
