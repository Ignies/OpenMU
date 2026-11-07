// <copyright file="Season21MapsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
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
                Assert.That(map.MapRequirements.Single(r => r.Attribute == Stats.TotalLevel).MinimumValue, Is.EqualTo(set.Maps.Single(s => s.Number == map.Number).EntryLevel ?? set.EntryLevel), map.Name.ValueInNeutralLanguage);
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

    /// <summary>
    /// The floors of Deep Dungeon ask the levels of their gates (Gate.xml 541-565: 730, 750, 770, 780, 780), not the
    /// 770 of Move.txt; Atlans Abyss 2 asks 925 (Gate.xml 618, Webzen), not 935; Nars asks 400 (Khdoop, muonlinefanz.com),
    /// not the 300 of Acheron which the data repeats (Gate.xml 487).
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="level">The expected total level.</param>
    [TestCase(116, 730)]
    [TestCase(117, 750)]
    [TestCase(118, 770)]
    [TestCase(119, 780)]
    [TestCase(120, 780)]
    [TestCase(129, 925)]
    [TestCase(110, 400)]
    public async Task MapAsksTheLevelOfItsGateAsync(byte mapNumber, int level)
    {
        var gameConfiguration = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        var map = gameConfiguration.Maps.Single(m => m.Number == mapNumber);
        Assert.That(map.MapRequirements.Single(r => r.Attribute == Stats.TotalLevel).MinimumValue, Is.EqualTo(level));
    }

    /// <summary>
    /// Old Kethotum (700) spawns only its Lv195 monsters; the 13 elite spots of the data are Old Crimson Minos. The
    /// same 13 spots of Blaze Kethotum hold its own Crimson Minos instead of the Old one.
    /// </summary>
    [Test]
    public async Task KethotumSpotsAreNotSwappedAsync()
    {
        var gameConfiguration = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        var old = SpawnsOf(gameConfiguration, 135);
        var blaze = SpawnsOf(gameConfiguration, 136);
        Assert.That(old.Keys, Is.EquivalentTo(new short[] { 892, 893, 894 }));
        Assert.That(blaze.Keys, Has.None.EqualTo((short)893));
        Assert.That(blaze[896], Is.EqualTo(59 + 13));
    }

    /// <summary>
    /// The placeholder spawn file of Arenil Temple spreads elites over the whole map; they belong to an elite zone,
    /// which doesn't exist, so only the normal monsters spawn.
    /// </summary>
    [Test]
    public async Task ArenilTempleSpawnsNoElitesAsync()
    {
        var gameConfiguration = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        Assert.That(SpawnsOf(gameConfiguration, 133).Keys, Has.None.InRange((short)870, (short)872));
        Assert.That(SpawnsOf(gameConfiguration, 133).Values.Sum(), Is.GreaterThan(0));
    }

    /// <summary>
    /// Kubera Mine uses the newer "Kubera Mine" spawn set, which spawns all seven monsters, the Dead Shoveler too.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="total">The expected number of monsters.</param>
    [TestCase(123, 86)]
    [TestCase(124, 84)]
    [TestCase(125, 86)]
    [TestCase(126, 86)]
    [TestCase(127, 86)]
    public async Task KuberaFloorsSpawnTheNewerSetAsync(byte mapNumber, int total)
    {
        var gameConfiguration = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        var spawns = SpawnsOf(gameConfiguration, mapNumber);
        Assert.That(spawns[815], Is.EqualTo(12));
        Assert.That(spawns.Values.Sum(), Is.EqualTo(total));
    }

    /// <summary>
    /// The data swapped the rows of the Scorched Assassin and the Scorched Wizard: the ranged magic row with the
    /// broken maximum damage (6325, its defense) is the wizard's.
    /// </summary>
    [Test]
    public async Task ScorchedAssassinAndWizardHaveTheirRowsAsync()
    {
        var gameConfiguration = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        var assassin = gameConfiguration.Monsters.Single(m => m.Number == 846);
        var wizard = gameConfiguration.Monsters.Single(m => m.Number == 847);
        Assert.That(ValueOf(assassin, Stats.MaximumHealth), Is.EqualTo(2248000));
        Assert.That(ValueOf(assassin, Stats.MaximumPhysBaseDmg), Is.EqualTo(13500));
        Assert.That(assassin.AttackRange, Is.EqualTo(2));
        Assert.That(ValueOf(wizard, Stats.MaximumHealth), Is.EqualTo(2145700));
        Assert.That(ValueOf(wizard, Stats.MinimumPhysBaseDmg), Is.EqualTo(7000));
        Assert.That(ValueOf(wizard, Stats.MaximumPhysBaseDmg), Is.EqualTo(7300));
        Assert.That(wizard.AttackRange, Is.EqualTo(4));
    }

    /// <summary>
    /// The monsters of the new maps drop their items of the Season 21 data (per-map drop lists): Kubera Mine drops
    /// e.g. the Dark Soul Armor, through a drop group of the monster, so the level rule of the map groups
    /// (monster level above 255) doesn't filter it out.
    /// </summary>
    [Test]
    public async Task NewMonstersDropTheirListsAsync()
    {
        var gameConfiguration = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        var digger = gameConfiguration.Monsters.Single(m => m.Number == MineDiggerNumber);
        var items = digger.DropItemGroups.SelectMany(g => g.PossibleItems).ToList();
        Assert.That(items, Has.Some.Matches<ItemDefinition>(i => i.Group == 8 && i.Number == 22));
        Assert.That(digger.DropItemGroups.Sum(g => g.Chance), Is.EqualTo(0.3).Within(0.0001));
        Assert.That(gameConfiguration.DropItemGroups, Is.SupersetOf(digger.DropItemGroups));
        var listed = VersionSeasonSix.Season21.Season21DropData.All.SelectMany(list => list.Monsters).ToHashSet();
        var own = VersionSeasonSix.Season21.Season21MapData.All.SelectMany(set => set.Monsters).Select(row => row.Number).ToHashSet();
        foreach (var monster in gameConfiguration.Monsters.Where(m => listed.Contains(m.Number) && own.Contains(m.Number)))
        {
            Assert.That(monster.DropItemGroups, Is.Not.Empty, monster.Designation.ValueInNeutralLanguage);
        }

        var seasonSixGroups = gameConfiguration.Monsters.Where(m => !own.Contains(m.Number)).SelectMany(m => m.DropItemGroups);
        Assert.That(seasonSixGroups, Has.None.Matches<DropItemGroup>(g => g.Description.ValueInNeutralLanguage == Properties.DropGroupDescriptions.Season21MonsterDrops), "a Season 6 monster keeps its drops");
    }

    /// <summary>
    /// The placeholder rows of the data are replaced: Crimson Icarus by the rows of muonlinefanz.com (859-863; the
    /// elites 864/865 by the same factors), Under Persona 905 by 1,060,000 health, the elites of Atlans Abyss 3 (839,
    /// 840) and Tormenta (945-947) derived from their normals by the elite factors of the data.
    /// </summary>
    /// <param name="number">The monster number.</param>
    /// <param name="health">The expected health.</param>
    [TestCase(859, 2_422_700)]
    [TestCase(863, 2_667_000)]
    [TestCase(864, 2_719_000)]
    [TestCase(905, 1_060_000)]
    [TestCase(839, 2_445_000)]
    [TestCase(947, 6_397_000)]
    public async Task PlaceholderRowsAreReplacedAsync(int number, int health)
    {
        var gameConfiguration = await CreateWithSeason21MapsAsync().ConfigureAwait(false);

        Assert.That(ValueOf(gameConfiguration.Monsters.Single(m => m.Number == number), Stats.MaximumHealth), Is.EqualTo(health));
    }

    /// <summary>
    /// The maps update repairs a database which has a placeholder row, and keeps a row the operator changed.
    /// </summary>
    [Test]
    public async Task UpdateRepairsThePlaceholderRowsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var icarus = gameConfiguration.Monsters.Single(m => m.Number == 859);
        var persona = gameConfiguration.Monsters.Single(m => m.Number == 905);
        SetValue(icarus, Stats.MaximumHealth, 650000);
        SetValue(icarus, Stats.MinimumPhysBaseDmg, 4900);
        SetValue(icarus, Stats.MaximumPhysBaseDmg, 5050);
        SetValue(icarus, Stats.DefenseBase, 3600);
        SetValue(icarus, Stats.DefenseRatePvm, 2050);
        SetValue(persona, Stats.MaximumHealth, 999000);

        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(ValueOf(icarus, Stats.MaximumHealth), Is.EqualTo(2_422_700));
        Assert.That(ValueOf(icarus, Stats.DefenseRatePvm), Is.EqualTo(3_357));
        Assert.That(ValueOf(persona, Stats.MaximumHealth), Is.EqualTo(999_000), "the operator's value");
    }

    /// <summary>
    /// The maps update moves Nars, and the gates and warps into it, from the 300 of Acheron to 400; an operator's level stays.
    /// </summary>
    [Test]
    public async Task UpdateRepairsTheLevelOfNarsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var nars = gameConfiguration.Maps.Single(m => m.Number == 110);
        var requirement = nars.MapRequirements.Single(r => r.Attribute == Stats.TotalLevel);
        var gates = gameConfiguration.Maps.SelectMany(m => m.EnterGates).Where(g => g.TargetGate?.Map == nars).ToList();
        Assert.That(gates, Is.Not.Empty);
        requirement.MinimumValue = 300;
        gates.ForEach(g => g.LevelRequirement = 300);
        gates[0].LevelRequirement = 350;

        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(requirement.MinimumValue, Is.EqualTo(400));
        Assert.That(gates.Skip(1).Select(g => (int)g.LevelRequirement), Is.All.EqualTo(400));
        Assert.That(gates[0].LevelRequirement, Is.EqualTo(350), "the operator's level");
    }

    private static Dictionary<short, int> SpawnsOf(GameConfiguration gameConfiguration, byte mapNumber)
        => gameConfiguration.Maps.Single(m => m.Number == mapNumber).MonsterSpawns
            .Where(s => s.MonsterDefinition?.ObjectKind == NpcObjectKind.Monster)
            .GroupBy(s => s.MonsterDefinition!.Number)
            .ToDictionary(g => (short)g.Key, g => g.Sum(s => (int)s.Quantity));

    private static async Task<GameConfiguration> CreateWithSeason21MapsAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return gameConfiguration;
    }

    /// <summary>
    /// The level caps update keeps a higher master level cap and a master experience formula of the operator.
    /// </summary>
    [Test]
    public async Task LevelCapsUpdateKeepsTheSettingsOfTheOperatorAsync()
    {
        var (context, gameConfiguration) = await CreateSeason6Async().ConfigureAwait(false);
        gameConfiguration.MaximumMasterLevel = 2000;
        gameConfiguration.MasterExperienceFormula = "level * 1000";

        await new AddSeason21LevelCapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.MaximumMasterLevel, Is.EqualTo(2000));
        Assert.That(gameConfiguration.MasterExperienceFormula, Is.EqualTo("level * 1000"));
    }

    private static long Evaluate(string formula, int level)
    {
        var argument = new org.mariuszgromada.math.mxparser.Argument("level");
        var expression = new org.mariuszgromada.math.mxparser.Expression(formula);
        expression.addArguments(argument);
        argument.setArgumentValue(level);
        return (long)expression.calculate();
    }

    private static void SetValue(MonsterDefinition monster, AttributeSystem.AttributeDefinition attribute, float value)
        => monster.Attributes.Single(a => a.AttributeDefinition == attribute).Value = value;

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
