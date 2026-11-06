// <copyright file="Season21WorldDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.EliteZones;
using MUnique.OpenMU.GameLogic.FieldBosses;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.Persistence.Initialization.Updates;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Tests the optional updates which add the elite zones and the field bosses of Season 21.
/// </summary>
[TestFixture]
internal class Season21WorldDataTest
{
    private const short CrimsonIcarus = 132;
    private const short TwistedKarutan = 142;

    /// <summary>
    /// Every elite of a zone map spawns inside one of its zones, Twisted Karutan's 45 elites included, on walkable ground.
    /// </summary>
    [Test]
    public async Task ElitesSpawnInsideTheirZonesAsync()
    {
        var (context, gameConfiguration) = await CreateWithMapsAsync().ConfigureAwait(false);

        await new AddSeason21EliteZonesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var configuration = GetConfiguration<EliteZoneConfiguration>(gameConfiguration, typeof(EliteZonesFeaturePlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Zones.Where(z => z.MapNumber == TwistedKarutan).Select(z => z.Element), Is.EquivalentTo(new[] { EliteZoneElement.Fire, EliteZoneElement.Water, EliteZoneElement.Earth, EliteZoneElement.Wind, EliteZoneElement.Darkness }));
        var fire = configuration.Zones.Single(z => z.MapNumber == TwistedKarutan && z.Element == EliteZoneElement.Fire);
        Assert.That((fire.EntranceX, fire.EntranceY), Is.EqualTo(((byte)226, (byte)54)), "Webzen's entrance.");

        foreach (var group in configuration.Zones.GroupBy(z => z.MapNumber))
        {
            var map = gameConfiguration.Maps.Single(m => m.Number == group.Key);
            var terrain = new GameMapTerrain(map);
            var elites = map.MonsterSpawns.Where(s => s.MonsterDefinition!.ObjectKind == NpcObjectKind.Monster && s.MonsterDefinition.Designation.ValueInNeutralLanguage.Contains("Elite", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.That(elites, Is.Not.Empty, $"Map {group.Key} has elites.");
            foreach (var spawn in elites)
            {
                Assert.That(group.Any(z => z.X1 <= spawn.X1 && spawn.X2 <= z.X2 && z.Y1 <= spawn.Y1 && spawn.Y2 <= z.Y2), Is.True, $"{spawn.MonsterDefinition} on map {group.Key} spawns in a zone.");
                Assert.That(HasWalkableTile(terrain, spawn), Is.True, $"{spawn.MonsterDefinition} on map {group.Key} finds ground.");
            }
        }

        var karutan = gameConfiguration.Maps.Single(m => m.Number == TwistedKarutan);
        Assert.That(karutan.MonsterSpawns.Where(s => s.MonsterDefinition!.Number is >= 958 and <= 960).Sum(s => s.Quantity), Is.EqualTo(45));
        Assert.That(new GameMapTerrain(karutan).WalkMap[226, 54], Is.True, "The zone is open.");
        Assert.That(gameConfiguration.Maps.Single(m => m.Number == CrimsonIcarus).MonsterSpawns.Where(s => s.MonsterDefinition!.Number is 864 or 865).Sum(s => s.Quantity), Is.EqualTo(30), "No more elites spread over the map.");
        Assert.That(gameConfiguration.Monsters.Select(m => m.Number), Does.Contain((short)914).And.Contain((short)968).And.Contain((short)974));

        Assert.That(configuration.Debuffs, Has.Count.EqualTo(10), "Two debuffs per element (EliteMonster.txt).");
        Assert.That(configuration.Debuffs.Single(d => d.Element == EliteZoneElement.Wind && d.Kind == EliteDebuffKind.DecreaseDefense).Value, Is.EqualTo(20));
        Assert.That(configuration.DropLimits.Single(l => l.ItemGroup == 14 && l.ItemNumber == 42).MaximumPerWeek, Is.EqualTo(10), "Jewel of Harmony (7210).");
        Assert.That(karutan.MonsterSpawns.Count(s => s.MonsterDefinition!.Number == 925), Is.EqualTo(10), "The ten Elite Zone Flags.");
        Assert.That(gameConfiguration.Monsters.Single(m => m.Number == 925).ObjectKind, Is.EqualTo(NpcObjectKind.PassiveNpc));
    }

    /// <summary>
    /// The field bosses update defines the bosses, spawns them only by the plugin, gives them their item bags and
    /// configures the plugin; the progression normalizes them with their map.
    /// </summary>
    [Test]
    public async Task FieldBossesAreDefinedAndConfiguredAsync()
    {
        var (context, gameConfiguration) = await CreateWithMapsAsync().ConfigureAwait(false);

        await new AddSeason21FieldBossesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Monsters.Select(m => m.Number), Is.SupersetOf(new short[] { 561, 734, 735, 790, 791, 792, 793, 794 }));
        var bossSpawns = gameConfiguration.Maps.SelectMany(m => m.MonsterSpawns).Where(s => s.MonsterDefinition!.Number is 673 or 716 or 746 or 561 or 794).ToList();
        Assert.That(bossSpawns.Select(s => s.MonsterDefinition!.Number).Distinct().Count(), Is.EqualTo(5));
        Assert.That(bossSpawns.Select(s => s.SpawnTrigger), Has.All.EqualTo(SpawnTrigger.ManuallyForEvent), "Only the plugin spawns them.");

        var nix = gameConfiguration.Monsters.Single(m => m.Number == 746);
        Assert.That(nix.DropItemGroups.Count(g => g.Chance >= 1.0), Is.EqualTo(3), "The three item sections of the Nix box.");
        Assert.That(nix.NumberOfMaximumItemDrops, Is.GreaterThanOrEqualTo(3));

        var configuration = GetConfiguration<FieldBossConfiguration>(gameConfiguration, typeof(FieldBossesFeaturePlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.Bosses.Select(b => b.Name), Is.EquivalentTo(new[] { "Medusa", "Silvester", "Magriffy", "Ferea", "Nix", "GodOfDarkness" }));
        Assert.That((configuration.RespawnMinimum, configuration.RespawnRandom), Is.EqualTo((TimeSpan.FromHours(18), TimeSpan.FromHours(6))));
        Assert.That(configuration.Bosses.SelectMany(b => b.Stages).Select(s => s.MonsterNumber), Has.All.Matches<short>(n => gameConfiguration.Monsters.Any(m => m.Number == n)));

        var medusa = configuration.Bosses.Single(b => b.Name == "Medusa");
        Assert.That(medusa.Grounds, Has.Count.EqualTo(4), "The four grounds of BossFightSystem.xml.");
        Assert.That(configuration.Bosses.Single(b => b.Name == "GodOfDarkness").Stages[^1].Abilities.Select(a => a.Name), Is.EquivalentTo(new[] { "Flame", "Darkness" }));
        var nixBoss = configuration.Bosses.Single(b => b.Name == "Nix");
        Assert.That(gameConfiguration.Items.Any(i => i.Group == nixBoss.EntryItemGroup && i.Number == nixBoss.EntryItemNumber), Is.True, "The Spirit Nixie's Shard is in the catalog.");
        Assert.That(gameConfiguration.Maps.Single(m => m.Number == 51).MonsterSpawns.Any(s => s.MonsterDefinition!.Number == nixBoss.EntryItemNpcNumber), Is.True, "Priest James stands in Elbeland.");

        var world = new ProgressionWorld(gameConfiguration, new ProgressionConfiguration { Mode = ProgressionMode.Normalized });
        Assert.That(world.TryGetMonster(nix, out var source, out var normalized), Is.True, "Nix is normalized with Nixie Lake.");
        Assert.That(normalized.Health, Is.LessThan(source.Health));
    }

    /// <summary>
    /// Every monster a field boss spawns (stages, their summons, the summons of abilities and health phases) and every
    /// boss map is defined, e.g. the Sapi Queens (560) which Medusa calls on Swamp of Calmness.
    /// </summary>
    [Test]
    public async Task FieldBossesSpawnOnlyDefinedMonstersAsync()
    {
        var (context, gameConfiguration) = await CreateWithMapsAsync().ConfigureAwait(false);

        await new AddSeason21FieldBossesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var configuration = GetConfiguration<FieldBossConfiguration>(gameConfiguration, typeof(FieldBossesFeaturePlugIn).GUID, out _);
        foreach (var boss in configuration.Bosses)
        {
            Assert.That(gameConfiguration.Maps.Any(m => m.Number == boss.MapNumber), Is.True, $"Map {boss.MapNumber} of {boss.Name}");
            var summons = boss.Stages.SelectMany(s => s.Summons.Concat(s.Abilities.SelectMany(a => a.Summons)).Concat(s.Phases.SelectMany(p => p.Summons)));
            var numbers = boss.Stages.Select(s => s.MonsterNumber).Concat(summons.Select(s => s.MonsterNumber)).Distinct();
            Assert.That(numbers.Where(n => gameConfiguration.Monsters.All(m => m.Number != n)), Is.Empty, $"Undefined monsters of {boss.Name}");
        }

        var sapiQueen = gameConfiguration.Monsters.Single(m => m.Number == 560);
        Assert.That(sapiQueen.Designation.ValueInNeutralLanguage, Is.EqualTo("Sapi Queen"));
        Assert.That(sapiQueen.Attributes.Single(a => a.AttributeDefinition == Stats.MaximumHealth).Value, Is.EqualTo(358000), "Monster.xml of Season 21.");
    }

    /// <summary>
    /// Every monster spawn area of the maps after Season 6 has at least one tile a monster may stand on: walkable and
    /// outside the safezone. (Event maps of Season 6 open blocked areas while they run, so they are left out.)
    /// </summary>
    [Test]
    public async Task EverySpawnAreaHasASpawnPointAsync()
    {
        var (context, gameConfiguration) = await CreateWithMapsAsync().ConfigureAwait(false);

        await new AddSeason21FieldBossesUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var invalid = new List<string>();
        foreach (var map in gameConfiguration.Maps.Where(m => m.IntroducedIn > GameVersion.Season6Episode3))
        {
            var terrain = new GameMapTerrain(map);
            invalid.AddRange(map.MonsterSpawns
                .Where(s => s.MonsterDefinition!.ObjectKind == NpcObjectKind.Monster && !HasTile(s, (x, y) => terrain.WalkMap[x, y] && !terrain.SafezoneMap[x, y]))
                .Select(s => $"{s.MonsterDefinition} on map {map.Number} at {s.X1}/{s.Y1}"));
        }

        Assert.That(invalid, Is.Empty);
    }

    /// <summary>
    /// Applying the updates a second time changes nothing and keeps what the operator changed.
    /// </summary>
    [Test]
    public async Task UpdatesTwiceChangeNothingAsync()
    {
        var (context, gameConfiguration) = await CreateWithMapsAsync().ConfigureAwait(false);
        var zones = new AddSeason21EliteZonesUpdatePlugIn();
        var bosses = new AddSeason21FieldBossesUpdatePlugIn();
        await zones.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await bosses.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var configuration = GetConfiguration<FieldBossConfiguration>(gameConfiguration, typeof(FieldBossesFeaturePlugIn).GUID, out var plugIn);
        configuration.RespawnMinimum = TimeSpan.FromHours(2);
        plugIn.SetConfiguration(configuration, null);
        var first = Count(gameConfiguration);

        await zones.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await bosses.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(Count(gameConfiguration), Is.EqualTo(first));
        Assert.That(GetConfiguration<FieldBossConfiguration>(gameConfiguration, typeof(FieldBossesFeaturePlugIn).GUID, out _).RespawnMinimum, Is.EqualTo(TimeSpan.FromHours(2)));
    }

    /// <summary>
    /// Both updates are optional and come after the maps.
    /// </summary>
    [Test]
    public void UpdatesAreOptionalAfterTheMaps()
    {
        Assert.That(new AddSeason21EliteZonesUpdatePlugIn().IsMandatory, Is.False);
        Assert.That(new AddSeason21FieldBossesUpdatePlugIn().IsMandatory, Is.False);
        Assert.That(new AddSeason21EliteZonesUpdatePlugIn().DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21MapsUpdatePlugIn().Key));
        Assert.That(new AddSeason21FieldBossesUpdatePlugIn().DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21MapsUpdatePlugIn().Key).And.Contain(new AddSeason21ItemCatalogUpdatePlugIn().Key));
    }

    private static bool HasWalkableTile(GameMapTerrain terrain, MonsterSpawnArea spawn)
        => HasTile(spawn, (x, y) => terrain.WalkMap[x, y]);

    private static bool HasTile(MonsterSpawnArea spawn, Func<int, int, bool> predicate)
    {
        for (var x = spawn.X1; x <= spawn.X2; x++)
        {
            for (var y = spawn.Y1; y <= spawn.Y2; y++)
            {
                if (predicate(x, y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static T GetConfiguration<T>(GameConfiguration gameConfiguration, Guid typeId, out PlugInConfiguration plugIn)
        where T : class
    {
        plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeId);
        return plugIn.GetConfiguration<T>(new IdIgnoringReferenceHandler())!;
    }

    private static (int Monsters, int Spawns, int DropGroups, int Items, string Zones, string Bosses) Count(GameConfiguration gameConfiguration)
        => (gameConfiguration.Monsters.Count,
            gameConfiguration.Maps.Sum(m => m.MonsterSpawns.Count),
            gameConfiguration.DropItemGroups.Count,
            gameConfiguration.DropItemGroups.Sum(g => g.PossibleItems.Count),
            gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(EliteZonesFeaturePlugIn).GUID).CustomConfiguration ?? string.Empty,
            gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(FieldBossesFeaturePlugIn).GUID).CustomConfiguration ?? string.Empty);

    private static async Task<(IContext Context, GameConfiguration GameConfiguration)> CreateWithMapsAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonSix.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);
        var context = contextProvider.CreateNewContext();
        var gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).First();
        await new AddSeason21ItemCatalogUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        await new AddSeason21MapsUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        return (context, gameConfiguration);
    }
}
