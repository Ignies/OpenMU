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
    private static readonly Lazy<Task<(IContext Context, GameConfiguration GameConfiguration)>> SharedSeason21 = new(CreateSeason21Async);

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
        Assert.That(normalized.Health, Is.EqualTo(253_500), "the design's 254,000 with the Dead Shoveler, which the data now spawns, in the median");
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
        Assert.That(progression.GetConfiguration<ProgressionConfiguration>(new IdIgnoringReferenceHandler())!.Mode, Is.EqualTo(ProgressionMode.Stars));
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
    /// A new database has the progression row, off and with the default (stars) configuration, so the plugin only does
    /// nothing until it's turned on; a database without the row gets it from the update.
    /// </summary>
    [Test]
    public async Task NewDatabaseHasTheRowAndTheUpdateAddsAMissingOneAsync()
    {
        var (context, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);
        var row = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ProgressionFeaturePlugIn).GUID);
        Assert.That(row.IsActive, Is.False);
        Assert.That(row.GetConfiguration<ProgressionConfiguration>(new IdIgnoringReferenceHandler())!.Mode, Is.EqualTo(ProgressionMode.Stars));

        gameConfiguration.PlugInConfigurations.Remove(row);
        await new AddProgressionUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var added = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ProgressionFeaturePlugIn).GUID);
        Assert.That(added.IsActive, Is.True);
        Assert.That(added.GetConfiguration<ProgressionConfiguration>(new IdIgnoringReferenceHandler())!.Mode, Is.EqualTo(ProgressionMode.Stars));
    }

    /// <summary>
    /// The update fills the Season 21 levels of the classic warps and gates (S21Emu Move.txt and Gate.xml) and keeps a
    /// table the operator filled.
    /// </summary>
    [Test]
    public async Task UpdateFillsTheClassicEntryLevelsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason21Async().ConfigureAwait(false);

        await new AddProgressionUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var row = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(ProgressionFeaturePlugIn).GUID);
        var configuration = row.GetConfiguration<ProgressionConfiguration>(new IdIgnoringReferenceHandler())!;
        Assert.That(configuration.Season21ClassicWarpLevels[34], Is.EqualTo(380), "Raklion");
        Assert.That(configuration.Season21ClassicWarpLevels[2], Is.EqualTo(1), "Lorencia");
        Assert.That(configuration.Season21ClassicWarpLevels.ContainsKey(48), Is.False, "La Cleon is the Event Square in the data");
        Assert.That(configuration.Season21ClassicGateLevels[274], Is.EqualTo(380), "Swamp of Calmness");
        Assert.That(configuration.Season21ClassicGateLevels[129], Is.EqualTo(300), "Kanturu Relics");

        configuration.Season21ClassicWarpLevels = new() { [34] = 300 };
        row.SetConfiguration(configuration, null);
        await new AddProgressionUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        Assert.That(row.GetConfiguration<ProgressionConfiguration>(new IdIgnoringReferenceHandler())!.Season21ClassicWarpLevels, Is.EquivalentTo(new Dictionary<int, int> { [34] = 300 }));
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

    /// <summary>
    /// The real data of three representative maps, in the stars mode: Ferea (400, the low end), Kubera Mine (850, a
    /// star) and the Swamp of Doom (1550, three stars). Ferea and the Swamp of Doom match the design table exactly; the
    /// Kubera rows are a little higher (Mine Digger 253,500 instead of 254,000) because the data now spawns the Dead
    /// Shoveler, which the design's spawn rows left out, so the set's median moves.
    /// </summary>
    /// <param name="map">The map number.</param>
    /// <param name="monster">The monster number.</param>
    /// <param name="level">The expected level.</param>
    /// <param name="health">The expected health.</param>
    /// <param name="damageMin">The expected minimum damage.</param>
    /// <param name="damageMax">The expected maximum damage.</param>
    /// <param name="defense">The expected defense.</param>
    /// <param name="attackRate">The expected attack rate.</param>
    /// <param name="defenseRate">The expected defense rate.</param>
    [TestCase(112, 730, 105, 63100, 636, 662, 510, 981, 321)]
    [TestCase(112, 731, 106, 58900, 683, 729, 491, 949, 319)]
    [TestCase(112, 732, 107, 72200, 569, 593, 564, 917, 341)]
    [TestCase(123, 810, 144, 253500, 1941, 2070, 757, 1968, 1321)]
    [TestCase(123, 811, 144, 254400, 1987, 2125, 783, 2092, 1367)]
    [TestCase(123, 812, 145, 255300, 2033, 2181, 810, 2056, 1412)]
    [TestCase(123, 813, 143, 256200, 2079, 2236, 836, 2276, 1458)]
    [TestCase(123, 814, 145, 257100, 2292, 2449, 863, 2680, 1504)]
    [TestCase(123, 815, 145, 258900, 2338, 2504, 889, 2790, 1550)]
    [TestCase(123, 816, 146, 258000, 2175, 2323, 840, 2834, 1479)]
    [TestCase(144, 969, 184, 546900, 3169, 3298, 1316, 3147, 2302)]
    [TestCase(144, 970, 185, 550100, 3039, 3164, 1334, 3347, 2246)]
    [TestCase(144, 971, 185, 548500, 3104, 3231, 1325, 3297, 2350)]
    public async Task RealDataGoldenRowsAsync(short map, short monster, int level, int health, int damageMin, int damageMax, int defense, int attackRate, int defenseRate)
    {
        var (_, gameConfiguration) = await GetSeason21Async().ConfigureAwait(false);
        var world = new ProgressionWorld(gameConfiguration, new ProgressionConfiguration { Mode = ProgressionMode.Stars });
        var definition = gameConfiguration.Maps.Single(m => m.Number == map).MonsterSpawns.Select(s => s.MonsterDefinition).First(m => m?.Number == monster)!;

        Assert.That(world.TryGetMonster(definition, out _, out var row), Is.True);
        Assert.That(row, Is.EqualTo(new MonsterStatRow(level, health, damageMin, damageMax, defense, attackRate, defenseRate)));
    }

    /// <summary>
    /// The requirements of the four maps in each mode.
    /// </summary>
    /// <param name="map">The map number.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="level">The expected level.</param>
    /// <param name="masterLevel">The expected master level.</param>
    /// <param name="stars">The expected stars.</param>
    [TestCase(112, ProgressionMode.Normalized, 190, 0, 0)]
    [TestCase(112, ProgressionMode.Stars, 190, 0, 0)]
    [TestCase(123, ProgressionMode.Normalized, 350, 0, 0)]
    [TestCase(123, ProgressionMode.Stars, 250, 0, 1)]
    [TestCase(144, ProgressionMode.Normalized, 400, 170, 0)]
    [TestCase(144, ProgressionMode.Stars, 270, 0, 3)]
    [TestCase(144, ProgressionMode.Original, 400, 1150, 0)]
    [TestCase(145, ProgressionMode.Normalized, 400, 185, 0)]
    [TestCase(145, ProgressionMode.Stars, 185, 0, 4)]
    [TestCase(145, ProgressionMode.Original, 400, 1200, 0)]
    public async Task RealDataGoldenRequirementsAsync(short map, ProgressionMode mode, int level, int masterLevel, int stars)
    {
        var (_, gameConfiguration) = await GetSeason21Async().ConfigureAwait(false);
        var configuration = new ProgressionConfiguration { Mode = mode };
        var definition = gameConfiguration.Maps.Single(m => m.Number == map);
        var requirement = ProgressionMapping.GetRequirement(ProgressionWorld.GetSeason21Requirement(definition)!.Value, configuration);

        Assert.That(requirement, Is.EqualTo(new MapRequirement(level, masterLevel, stars)));
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

    /// <summary>
    /// Gets a shared Season 21 configuration for the tests which only read it.
    /// </summary>
    private static Task<(IContext Context, GameConfiguration GameConfiguration)> GetSeason21Async() => SharedSeason21.Value;

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
