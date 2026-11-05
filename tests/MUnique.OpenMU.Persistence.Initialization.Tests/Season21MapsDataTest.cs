// <copyright file="Season21MapsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Tests the optional updates which add the maps of the later seasons to a Season 6 database, as the Season 21 server
/// data defines them, with the level caps of Season 21 which they depend on.
/// </summary>
[TestFixture]
internal class Season21MapsDataTest
{
    private const byte DeviasNumber = 2;
    private const byte KuberaMine1Number = 123;
    private const short MineDiggerNumber = 810;

    private static readonly byte[] AllMapNumbers =
    [
        123, 124, 125, 126, 127, 113, 122, 131, 91, 95, 100, 110, 112, 116, 117, 118, 119, 120,
        132, 133, 134, 135, 136, 137, 138, 140, 141, 142, 143, 144, 128, 129, 130,
    ];

    /// <summary>
    /// A new Season 6 database doesn't contain the maps, and the updates stay pending, so the operator decides.
    /// </summary>
    [Test]
    public async Task NewDatabaseLeavesTheUpdatesPendingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        Assert.That(gameConfiguration.Maps.Select(m => m.Number), Has.None.EqualTo(KuberaMine1Number));
        Assert.That(gameConfiguration.Monsters.Select(m => m.Number), Has.None.EqualTo(MineDiggerNumber));
        var installed = (await context.GetAsync<ConfigurationUpdate>().ConfigureAwait(false)).Select(u => u.Key).ToList();
        Assert.That(installed, Has.None.EqualTo(new AddSeason21MapsUpdatePlugIn().Key));
        Assert.That(installed, Has.None.EqualTo(new AddSeason21LevelCapsUpdatePlugIn().Key));
    }

    /// <summary>
    /// The update adds Kubera Mine with its terrain, entry level, monsters, spawns, gates and warp list entry.
    /// </summary>
    [Test]
    public async Task UpdateAddsKuberaMineAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var map = gameConfiguration.Maps.Single(m => m.Number == KuberaMine1Number);
        Assert.That(map.Name.ValueInNeutralLanguage, Is.EqualTo("Kubera Mine 1"));
        Assert.That(map.IntroducedIn, Is.EqualTo(GameVersion.Season14Episode1));
        Assert.That(map.SafezoneMap?.Number, Is.EqualTo(DeviasNumber));
        Assert.That(map.TerrainData, Has.Length.EqualTo(65539));
        Assert.That(map.TerrainData, Is.EqualTo(TerrainUpdateHelper.ReadTerrainResource("Season21.Terrain124.att")));
        Assert.That(map.MapRequirements.Single(r => r.Attribute == Stats.TotalLevel).MinimumValue, Is.EqualTo(850));
        Assert.That(map.MonsterSpawns.Where(s => s.MonsterDefinition?.Number == MineDiggerNumber).Sum(s => s.Quantity), Is.GreaterThan(0));
        Assert.That(map.DropItemGroups, Is.EquivalentTo(gameConfiguration.Maps.Single(m => m.Number == DeviasNumber).DropItemGroups));

        var digger = gameConfiguration.Monsters.Single(m => m.Number == MineDiggerNumber);
        Assert.That(digger.Designation.ValueInNeutralLanguage, Is.EqualTo("Mine Digger"));
        Assert.That(digger.IntroducedIn, Is.EqualTo(GameVersion.Season14Episode1));
        Assert.That(ValueOf(digger, Stats.Level), Is.EqualTo(846));
        Assert.That(ValueOf(digger, Stats.MaximumHealth), Is.EqualTo(1415000));
        Assert.That(ValueOf(digger, Stats.MinimumPhysBaseDmg), Is.EqualTo(5471));
        Assert.That(ValueOf(digger, Stats.LightningResistance), Is.EqualTo(2f / 255));

        var entrance = gameConfiguration.Maps.Single(m => m.Number == DeviasNumber).EnterGates.Single(g => g.Number == 612);
        Assert.That(entrance.TargetGate?.Map, Is.SameAs(map));
        Assert.That(entrance.LevelRequirement, Is.EqualTo(400));

        var warp = gameConfiguration.WarpList.Single(w => w.Index == 58);
        Assert.That(warp.Name.ValueInNeutralLanguage, Is.EqualTo("Kubera Mine"));
        Assert.That(warp.Costs, Is.EqualTo(50000));
        Assert.That(warp.LevelRequirement, Is.EqualTo(400));
        Assert.That(warp.Gate?.Map, Is.SameAs(map));

        var servers = (await context.GetAsync<GameServerConfiguration>().ConfigureAwait(false)).Where(s => s.Maps.Any(m => m.Number == DeviasNumber)).ToList();
        Assert.That(servers, Is.Not.Empty);
        Assert.That(servers.Select(s => s.Maps.Contains(map)), Has.All.True);
    }

    /// <summary>
    /// The update adds every map, each with its terrain and its version; the gates lead to the maps there are.
    /// </summary>
    [Test]
    public async Task UpdateAddsEveryMapAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var maps = gameConfiguration.Maps.Where(m => AllMapNumbers.Contains((byte)m.Number)).ToList();
        Assert.That(maps.Select(m => (byte)m.Number), Is.EquivalentTo(AllMapNumbers));
        Assert.That(maps.Select(m => m.TerrainData?.Length), Has.All.EqualTo(65539));
        Assert.That(maps.Select(m => m.IntroducedIn), Has.All.GreaterThan(GameVersion.Season6Episode3));
        Assert.That(maps.Single(m => m.Number == 91).IntroducedIn, Is.EqualTo(GameVersion.Season7));
        Assert.That(maps.Single(m => m.Number == 95).IntroducedIn, Is.EqualTo(GameVersion.Season8Episode1));
        Assert.That(maps.Single(m => m.Number == 122).IntroducedIn, Is.EqualTo(GameVersion.Season13Episode1));
        Assert.That(maps.Select(m => m.SafezoneMap), Has.None.Null);
        Assert.That(gameConfiguration.Maps.SelectMany(m => m.EnterGates).Select(g => g.TargetGate?.Map), Has.None.Null);

        var monsters = gameConfiguration.Monsters.Where(m => m.Number >= 586 && m.Number <= 980 && m.Number is not (>= 658 and <= 668)).ToList();
        Assert.That(monsters.Select(m => m.IntroducedIn), Has.All.GreaterThan(GameVersion.Season6Episode3));
        Assert.That(gameConfiguration.Monsters.Single(m => m.Number == 748).ObjectKind, Is.EqualTo(NpcObjectKind.PassiveNpc));
    }

    /// <summary>
    /// Applying the update a second time changes nothing.
    /// </summary>
    [Test]
    public async Task UpdateTwiceChangesNothingAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21MapsUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var first = Count(gameConfiguration);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(Count(gameConfiguration), Is.EqualTo(first));
    }

    /// <summary>
    /// The maps update depends on the level caps update, so the maps never land on the caps of Season 6.
    /// </summary>
    [Test]
    public void MapsDependOnTheLevelCaps()
    {
        Assert.That(new AddSeason21MapsUpdatePlugIn().DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21LevelCapsUpdatePlugIn().Key));
    }

    /// <summary>
    /// After both updates, every map asks the entry level of the Season 21 data and every monster has its values,
    /// e.g. Kubera Mine asks a total level of 850 on the caps of 400 and 1350.
    /// </summary>
    [Test]
    public async Task MapsKeepTheValuesOfTheSeason21DataAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);

        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.MaximumLevel + gameConfiguration.MaximumMasterLevel, Is.EqualTo(1750));
        Assert.That(gameConfiguration.Maps.Single(m => m.Number == KuberaMine1Number).MapRequirements.Single(r => r.Attribute == Stats.TotalLevel).MinimumValue, Is.EqualTo(850));
        Assert.That(ValueOf(gameConfiguration.Monsters.Single(m => m.Number == MineDiggerNumber), Stats.MaximumHealth), Is.EqualTo(1415000));
        foreach (var set in VersionSeasonSix.Season21.Season21MapData.All)
        {
            foreach (var map in gameConfiguration.Maps.Where(m => set.Maps.Any(s => s.Number == m.Number)))
            {
                Assert.That(map.MapRequirements.Single(r => r.Attribute == Stats.TotalLevel).MinimumValue, Is.EqualTo(set.EntryLevel), map.Name.ValueInNeutralLanguage);
            }

            foreach (var row in set.Monsters)
            {
                var monster = gameConfiguration.Monsters.Single(m => m.Number == row.Number);
                var name = monster.Designation.ValueInNeutralLanguage;
                Assert.That(ValueOf(monster, Stats.Level), Is.EqualTo(row.Level), name);
                Assert.That(ValueOf(monster, Stats.MaximumHealth), Is.EqualTo(row.Health), name);
                Assert.That(ValueOf(monster, Stats.MinimumPhysBaseDmg), Is.EqualTo(row.MinimumDamage), name);
                Assert.That(ValueOf(monster, Stats.MaximumPhysBaseDmg), Is.EqualTo(row.MaximumDamage), name);
                Assert.That(ValueOf(monster, Stats.DefenseBase), Is.EqualTo(row.Defense), name);
                Assert.That(ValueOf(monster, Stats.AttackRatePvm), Is.EqualTo(row.AttackRate), name);
                Assert.That(ValueOf(monster, Stats.DefenseRatePvm), Is.EqualTo(row.DefenseRate), name);
            }
        }
    }

    /// <summary>
    /// The level caps update sets the master level cap of Season 21 and its master experience curve: the one of
    /// Season 6 up to master level 400, and twice its step above.
    /// </summary>
    [Test]
    public async Task LevelCapsUpdateSetsTheSeason21CapsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        var update = new AddSeason21LevelCapsUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.MaximumLevel, Is.EqualTo(400));
        Assert.That(gameConfiguration.MaximumMasterLevel, Is.EqualTo(1350));
        Assert.That(Evaluate(gameConfiguration.MasterExperienceFormula!, 400), Is.EqualTo(82918600000));
        Assert.That(Evaluate(gameConfiguration.MasterExperienceFormula!, 401), Is.EqualTo(83840498100));
        Assert.That(Evaluate(gameConfiguration.MasterExperienceFormula!, 1350), Is.EqualTo(3328536125000));
    }

    private static long Evaluate(string formula, int level)
    {
        var argument = new org.mariuszgromada.math.mxparser.Argument("level");
        var expression = new org.mariuszgromada.math.mxparser.Expression(formula);
        expression.addArguments(argument);
        argument.setArgumentValue(level);
        return (long)expression.calculate();
    }

    private static float ValueOf(MonsterDefinition monster, AttributeSystem.AttributeDefinition attribute)
        => monster.Attributes.Single(a => a.AttributeDefinition == attribute).Value;

    private static (int Maps, int Monsters, int Spawns, int EnterGates, int ExitGates, int Warps) Count(GameConfiguration gameConfiguration)
        => (gameConfiguration.Maps.Count,
            gameConfiguration.Monsters.Count,
            gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count),
            gameConfiguration.Maps.Sum(m => m.EnterGates.Count),
            gameConfiguration.Maps.Sum(m => m.ExitGates.Count),
            gameConfiguration.WarpList.Count);

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateSeason6Async()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        return (context, gameConfiguration);
    }
}
