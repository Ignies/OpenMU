// <copyright file="Season21MuunsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Muuns;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the optional update of the Muuns of Season 21.
/// </summary>
[TestFixture]
internal class Season21MuunsDataTest
{
    /// <summary>
    /// The update configures the Muun system with the data and switches it on, gives the evolution stones their
    /// target option and places Monica in Elbeland.
    /// </summary>
    [Test]
    public async Task MuunsUpdateConfiguresTheMuunSystemAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21MuunsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(MuunFeaturePlugIn).GUID);
        var configuration = plugIn.GetConfiguration<MuunConfiguration>(new IdIgnoringReferenceHandler())!;
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That((configuration.Muuns.Count, configuration.Options.Count, configuration.AddOptions.Count, configuration.ExchangeOffers.Count), Is.EqualTo((386, 78, 10, 6)));

        var pinocchio = configuration.Muuns.Single(m => m.Number == 0);
        Assert.That((pinocchio.Grade, pinocchio.Rank, pinocchio.MaximumLevel, pinocchio.EvolvesTo, pinocchio.AddOptionDays), Is.EqualTo((MuunGrade.Premium, (byte)4, (byte)5, (short?)1, 30)));
        Assert.That(configuration.Muuns.Single(m => m.Number == 1).IsEvolved, Is.True);
        Assert.That(configuration.Muuns.Single(m => m.Number == 231).Kind, Is.EqualTo(MuunKind.Mount), "Tibetton");
        Assert.That(configuration.Muuns.Count(m => m.IsEvolved), Is.EqualTo(178));

        var option = configuration.Options.Single(o => o.Group == 42);
        Assert.That((option.Type, option.Values, option.Condition.Type, option.Condition.Value1), Is.EqualTo((MuunOptionType.MaximumAttackPower, new List<int> { 24, 14, 16, 18, 20, 22 }, MuunConditionType.Weekdays, 79)));
        Assert.That(configuration.AddOptions.Single(a => a.Type == 2), Has.Property(nameof(MuunAddOptionDefinition.Kind)).EqualTo(MuunAddOptionKind.Multiplier));
        Assert.That(configuration.ExchangeOffers.Select(o => o.TargetNumber), Is.EqualTo(new short?[] { 411, 413, 158, 326, 328, 282 }));
        Assert.That(configuration.LevelEnergy.Single(e => e.Key == 5).Points, Is.EqualTo(15));

        foreach (var number in new short[] { 211, 510 })
        {
            var stone = gameConfiguration.Items.Single(i => i.Group == 16 && i.Number == number);
            Assert.That(stone.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Select(o => o.OptionType), Does.Contain(Season21ItemOptionTypes.MuunEvolutionTarget));
        }

        var elbeland = gameConfiguration.Maps.Single(m => m.Number == 51);
        var monica = elbeland.MonsterSpawns.Single(s => s.MonsterDefinition?.Number == 682);
        Assert.That((monica.X1, monica.Y1, monica.Direction), Is.EqualTo(((byte)24, (byte)229, Direction.SouthWest)));
    }

    /// <summary>
    /// Applied twice, the update changes nothing and keeps a switch the operator turned off; it is optional and
    /// comes after the activation of the wide items.
    /// </summary>
    [Test]
    public async Task MuunsUpdateTwiceChangesNothingAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21MuunsUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(MuunFeaturePlugIn).GUID);
        plugIn.IsActive = false;
        var before = (plugIn.CustomConfiguration, gameConfiguration.ItemOptions.Count, gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count), gameConfiguration.Monsters.Count);

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That((plugIn.CustomConfiguration, gameConfiguration.ItemOptions.Count, gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count), gameConfiguration.Monsters.Count), Is.EqualTo(before));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(update.IsMandatory, Is.False);
        Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new ActivateSeason21WideItemsUpdatePlugIn().Key));
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        UpdatePlugInBase[] updates = [new AddSeason21GearUpdatePlugIn(), new AddSeason21ItemCatalogUpdatePlugIn(), new ActivateSeason21WideItemsUpdatePlugIn()];
        foreach (var update in updates)
        {
            await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        }

        return (context, gameConfiguration);
    }
}
