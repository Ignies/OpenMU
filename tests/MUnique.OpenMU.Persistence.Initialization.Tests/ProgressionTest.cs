// <copyright file="ProgressionTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Resets;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the progression on the data of the Season 21 maps, and <see cref="AddProgressionUpdatePlugIn"/>.
/// </summary>
[TestFixture]
internal class ProgressionTest
{
    /// <summary>
    /// On the real data, the stars mode asks Kubera Mine for a star and level 250 and Scorched Canyon for two stars and
    /// level 200, puts every monster below level 256 and the Mine Digger near the design's 254,000 health, and leaves
    /// the stored rows unchanged.
    /// </summary>
    [Test]
    public async Task StarsOnTheSeason21DataAsync()
    {
        var (_, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);
        var world = new ProgressionWorld(gameConfiguration, new ProgressionConfiguration { Mode = ProgressionMode.Stars });

        Assert.That(world.TryGetRequirement(gameConfiguration.Maps.Single(m => m.Number == 123), out var kubera), Is.True);
        Assert.That(kubera, Is.EqualTo(new MapRequirement(250, 0, 1)));
        world.TryGetRequirement(gameConfiguration.Maps.Single(m => m.Number == 131), out var scorched);
        Assert.That(scorched, Is.EqualTo(new MapRequirement(200, 0, 2)));
        Assert.That(world.TryGetRequirement(gameConfiguration.Maps.Single(m => m.Number == 0 && m.Discriminator == 0), out _), Is.False);

        var digger = gameConfiguration.Monsters.Single(m => m.Number == 810);
        Assert.That(world.TryGetMonster(digger, out var source, out var normalized), Is.True);
        Assert.That(source.Health, Is.EqualTo(1_415_000));
        Assert.That(digger[Stats.MaximumHealth], Is.EqualTo(1_415_000));
        Assert.That(normalized.Health, Is.EqualTo(254_000).Within(254_000 * 0.15));
        Assert.That(normalized.Level, Is.InRange(130, 160));

        var rows = gameConfiguration.Monsters.Select(m => world.TryGetMonster(m, out _, out var row) ? row : (MonsterStatRow?)null).OfType<MonsterStatRow>().ToList();
        Assert.That(rows, Has.Count.GreaterThan(100));
        Assert.That(rows.Select(r => r.Level), Has.All.LessThan(256));
        Assert.That(rows.Select(r => r.Health), Has.All.LessThan(4_100_000));
        Assert.That(world.TryGetMonster(gameConfiguration.Monsters.Single(m => m.Number == 3), out _, out _), Is.False, "a Season 6 monster");
    }

    /// <summary>
    /// The update turns the progression on in the stars mode and the reset feature with the star costs; a second run
    /// changes nothing.
    /// </summary>
    [Test]
    public async Task UpdateTurnsOnStarsAndTheResetsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);
        var update = new AddProgressionUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var first = ReadReset(gameConfiguration);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var progression = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ProgressionFeaturePlugIn).GUID);
        Assert.That(progression.IsActive, Is.True);
        Assert.That(progression.GetConfiguration<ProgressionConfiguration>(null)!.Mode, Is.EqualTo(ProgressionMode.Stars));
        var reset = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ResetFeaturePlugIn).GUID);
        Assert.That(reset.IsActive, Is.True);
        var (money, limit, tiers) = ReadReset(gameConfiguration);
        Assert.That(money, Is.EqualTo(5_000_000));
        Assert.That(limit, Is.EqualTo(10));
        Assert.That(tiers, Is.EqualTo(5));
        Assert.That((money, limit, tiers), Is.EqualTo(first));
        Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21MapsUpdatePlugIn().Key));
        Assert.That(update.IsMandatory, Is.False);
    }

    /// <summary>
    /// A reset value which an operator changed stays.
    /// </summary>
    [Test]
    public async Task UpdateKeepsTheOperatorsResetValuesAsync()
    {
        var (context, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);
        var reset = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ResetFeaturePlugIn).GUID);
        reset.SetConfiguration(new ResetConfiguration { RequiredMoney = 777, ResetLimit = 3 }, null);

        await new AddProgressionUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var (money, limit, _) = ReadReset(gameConfiguration);
        Assert.That(money, Is.EqualTo(777));
        Assert.That(limit, Is.EqualTo(3));
    }

    private static (int Money, int? Limit, int Tiers) ReadReset(GameConfiguration gameConfiguration)
    {
        var json = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ResetFeaturePlugIn).GUID).CustomConfiguration!;
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var tiers = root.GetProperty(nameof(ResetConfiguration.ItemCostTiers));
        if (tiers.ValueKind == JsonValueKind.Object && tiers.TryGetProperty("$values", out var values))
        {
            tiers = values;
        }

        var limit = root.GetProperty(nameof(ResetConfiguration.ResetLimit));
        return (
            root.GetProperty(nameof(ResetConfiguration.RequiredMoney)).GetInt32(),
            limit.ValueKind == JsonValueKind.Null ? null : limit.GetInt32(),
            tiers.GetArrayLength());
    }

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason21Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
