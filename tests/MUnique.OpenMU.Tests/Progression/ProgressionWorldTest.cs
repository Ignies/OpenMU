// <copyright file="ProgressionWorldTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlayerActions;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.PlugIns;
using AttributeRequirement = MUnique.OpenMU.Persistence.BasicModel.AttributeRequirement;
using ExitGate = MUnique.OpenMU.Persistence.BasicModel.ExitGate;
using GameConfiguration = MUnique.OpenMU.Persistence.BasicModel.GameConfiguration;
using GameMapDefinition = MUnique.OpenMU.Persistence.BasicModel.GameMapDefinition;
using MonsterAttribute = MUnique.OpenMU.Persistence.BasicModel.MonsterAttribute;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;
using MonsterSpawnArea = MUnique.OpenMU.Persistence.BasicModel.MonsterSpawnArea;
using WarpInfo = MUnique.OpenMU.Persistence.BasicModel.WarpInfo;

/// <summary>
/// Tests for <see cref="ProgressionWorld"/> and the runtime of <see cref="ProgressionFeaturePlugIn"/>: the layer which
/// applies the progression to the Season 21 maps of a game configuration without changing its rows.
/// </summary>
[TestFixture]
public class ProgressionWorldTest
{
    private GameConfiguration _configuration = null!;
    private GameMapDefinition _devias = null!;
    private GameMapDefinition _kubera1 = null!;
    private GameMapDefinition _kubera2 = null!;
    private GameMapDefinition _acheron = null!;
    private GameMapDefinition _urk = null!;
    private MonsterDefinition _season6Monster = null!;
    private MonsterDefinition _digger = null!;
    private MonsterDefinition _carrier = null!;
    private MonsterDefinition _fairy = null!;
    private MonsterDefinition _urkFairy = null!;

    /// <summary>
    /// Sets up a configuration with Devias, Kubera Mine 1 and 2 (850, sharing the diggers) and Acheron and Urk
    /// Mountain (300, same safezone, own monsters); a Season 6 monster spawns in Devias and in Kubera Mine 1.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._configuration = new GameConfiguration { Id = Guid.NewGuid(), MaximumLevel = 400, MaximumMasterLevel = 1350 };
        this._devias = this.AddMap(2, null, null);
        this._kubera1 = this.AddMap(123, this._devias, 850);
        this._kubera2 = this.AddMap(124, this._devias, 850);
        this._acheron = this.AddMap(91, this._devias, 300);
        this._urk = this.AddMap(100, this._devias, 300);
        this._season6Monster = CreateMonster(26, "Goblin", 9, 12_000, 300, 320, 400, 900, 300);
        this._digger = CreateMonster(810, "Mine Digger", 846, 1_415_000, 5471, 5835, 4097, 2680, 1875);
        this._carrier = CreateMonster(811, "Mine Carrier", 848, 1_420_000, 5601, 5991, 4240, 2780, 1940);
        this._fairy = CreateMonster(586, "Devil Fairy", 201, 29_000, 408, 443, 315, 600, 190);
        this._urkFairy = CreateMonster(670, "Urk Devil Fairy", 331, 68_000, 730, 760, 630, 1000, 290);
        Spawn(this._devias, this._season6Monster);
        Spawn(this._kubera1, this._season6Monster);
        Spawn(this._kubera1, this._digger);
        Spawn(this._kubera2, this._digger);
        Spawn(this._kubera2, this._carrier);
        Spawn(this._acheron, this._fairy);
        Spawn(this._urk, this._urkFairy);
        this._configuration.WarpList.Add(new WarpInfo { Index = 50, Costs = 50_000, LevelRequirement = 400, Gate = new ExitGate { Map = this._kubera1 } });
    }

    /// <summary>
    /// The requirements of a Season 21 map follow the mode; other maps have none.
    /// </summary>
    [Test]
    public void RequirementsFollowTheMode()
    {
        var stars = new ProgressionWorld(this._configuration, new ProgressionConfiguration { Mode = ProgressionMode.Stars });
        var normalized = new ProgressionWorld(this._configuration, new ProgressionConfiguration { Mode = ProgressionMode.Normalized });

        Assert.That(stars.TryGetRequirement(this._kubera1, out var starRequirement), Is.True);
        Assert.That(starRequirement, Is.EqualTo(new MapRequirement(250, 0, 1)));
        Assert.That(normalized.TryGetRequirement(this._kubera1, out var normalizedRequirement), Is.True);
        Assert.That(normalizedRequirement, Is.EqualTo(new MapRequirement(350, 0, 0)));
        Assert.That(stars.TryGetRequirement(this._devias, out _), Is.False);
        var warp = this._configuration.WarpList.Single();
        Assert.That(stars.GetMoveLevelRequirement(warp.Gate!.Map, warp.LevelRequirement), Is.EqualTo(250));
        Assert.That(stars.GetMoveLevelRequirement(this._devias, 20), Is.EqualTo(20));
        Assert.That(stars.GetWarpCosts(warp), Is.EqualTo(18_500));
        Assert.That(warp.Costs, Is.EqualTo(50_000));
    }

    /// <summary>
    /// The monsters which spawn only on Season 21 maps are normalized per set, at the set's requirement; a Season 6
    /// monster keeps its numbers; the stored rows stay.
    /// </summary>
    [Test]
    public void MonstersAreNormalizedPerSet()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var world = new ProgressionWorld(this._configuration, configuration);

        Assert.That(world.TryGetMonster(this._season6Monster, out _, out _), Is.False);
        Assert.That(world.TryGetMonster(this._digger, out var source, out var digger), Is.True);
        Assert.That(source.Health, Is.EqualTo(1_415_000));
        Assert.That(this._digger[Stats.MaximumHealth], Is.EqualTo(1_415_000));

        var kubera = MonsterNormalizer.Normalize(
            850,
            [Row(this._digger), Row(this._carrier)],
            configuration);
        Assert.That(digger, Is.EqualTo(kubera[0]));

        var acheron = MonsterNormalizer.Normalize(300, [Row(this._fairy), Row(this._urkFairy)], configuration);
        world.TryGetMonster(this._urkFairy, out _, out var urkFairy);
        Assert.That(urkFairy, Is.EqualTo(acheron[1]));
        Assert.That(urkFairy.Health, Is.GreaterThan(acheron[0].Health));
    }

    /// <summary>
    /// Items drop at the normalized level, capped at 160.
    /// </summary>
    [Test]
    public void DropLevelIsNormalizedAndCapped()
    {
        var world = new ProgressionWorld(this._configuration, new ProgressionConfiguration { Mode = ProgressionMode.Stars, MonsterDropLevelCap = 140 });
        world.TryGetMonster(this._digger, out _, out var digger);

        Assert.That(world.GetDropLevel(this._digger), Is.EqualTo(Math.Min(digger.Level, 140)));
        Assert.That(world.GetDropLevel(this._season6Monster), Is.EqualTo(9));
    }

    /// <summary>
    /// The plugin sets the caps of the mode on the running configuration and puts them back in the original mode.
    /// </summary>
    [Test]
    public void CapsFollowTheModeAndComeBack()
    {
        var plugIn = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars } };

        Assert.That(plugIn.GetWorld(this._configuration), Is.Not.Null);
        Assert.That(this._configuration.MaximumMasterLevel, Is.Zero);

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        plugIn.GetWorld(this._configuration);
        Assert.That(this._configuration.MaximumMasterLevel, Is.EqualTo(200));

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original };
        Assert.That(plugIn.GetWorld(this._configuration), Is.Null);
        Assert.That(this._configuration.MaximumMasterLevel, Is.EqualTo(1350));
        Assert.That(this._configuration.MaximumLevel, Is.EqualTo(400));
    }

    /// <summary>
    /// The map check asks for the stars of a map instead of its stored total level.
    /// </summary>
    [Test]
    public async Task MapCheckAsksForStarsAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars } };
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        player.GameContext.Configuration.Maps.Add(this._kubera1);
        player.Attributes![Stats.Level] = 260;

        Assert.That(this._kubera1.TryGetRequirementError(player, out _), Is.True, "no star");

        player.Attributes[Stats.Resets] = 1;
        Assert.That(this._kubera1.TryGetRequirementError(player, out _), Is.False);

        player.Attributes[Stats.Level] = 240;
        Assert.That(this._kubera1.TryGetRequirementError(player, out _), Is.True);

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original };
        player.Attributes[Stats.Level] = 400;
        Assert.That(this._kubera1.TryGetRequirementError(player, out _), Is.True, "the original total level 850");
    }

    /// <summary>
    /// A spawned Season 21 monster gets the normalized numbers; its definition keeps the stored ones.
    /// </summary>
    [Test]
    public async Task SpawnedMonsterGetsTheNormalizedNumbersAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars } };
        var world = plugIn.GetWorld(this._configuration)!;
        world.TryGetMonster(this._digger, out _, out var expected);
        this._kubera1.TerrainData = new byte[ushort.MaxValue + 3];
        var map = new GameMap(this._kubera1, TimeSpan.FromSeconds(60), 8);
        var spawn = this._kubera1.MonsterSpawns.First(s => s.MonsterDefinition == this._digger);
        var monster = new Monster(spawn, this._digger, map, NullDropGenerator.Instance, new Mock<INpcIntelligence>().Object, player.GameContext.PlugInManager, player.GameContext.PathFinderPool);
        monster.Initialize();

        await plugIn.ObjectAddedToMapAsync(map, monster).ConfigureAwait(false);

        Assert.That(monster.Attributes[Stats.MaximumHealth], Is.EqualTo(expected.Health).Within(1));
        Assert.That(monster.Attributes[Stats.Level], Is.EqualTo(expected.Level).Within(0.01));
        Assert.That(monster.Attributes[Stats.DefenseBase], Is.EqualTo(expected.Defense).Within(0.5));
        Assert.That(monster.Health, Is.EqualTo(expected.Health).Within(1));
        Assert.That(this._digger[Stats.MaximumHealth], Is.EqualTo(1_415_000));

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original };
        plugIn.GetWorld(this._configuration);
        Assert.That(monster.Attributes[Stats.MaximumHealth], Is.EqualTo(1_415_000).Within(1));

        await plugIn.ObjectRemovedFromMapAsync(map, monster).ConfigureAwait(false);
        Assert.That(monster.Attributes[Stats.MaximumHealth], Is.EqualTo(1_415_000));
    }

    /// <summary>
    /// Turning the plugin off puts the stored caps back at once; turning it on again applies the mode again.
    /// </summary>
    [Test]
    public void DeactivationRestoresTheCapsAtOnce()
    {
        var plugInConfiguration = new PlugInConfiguration { TypeId = typeof(ProgressionFeaturePlugIn).GUID, IsActive = true };
        plugInConfiguration.SetConfiguration(new ProgressionConfiguration { Mode = ProgressionMode.Stars }, null);
        var gameContext = GameContextTestHelper.CreateGameContext([plugInConfiguration], maximumLevel: 400);
        gameContext.Configuration.MaximumMasterLevel = 1350;

        Assert.That(ProgressionFeaturePlugIn.GetWorld(gameContext), Is.Not.Null);
        Assert.That(gameContext.Configuration.MaximumMasterLevel, Is.Zero);

        gameContext.PlugInManager.DeactivatePlugIn<ProgressionFeaturePlugIn>();
        Assert.That(gameContext.Configuration.MaximumMasterLevel, Is.EqualTo(1350));
        Assert.That(ProgressionFeaturePlugIn.GetWorld(gameContext), Is.Null);

        gameContext.PlugInManager.ActivatePlugIn<ProgressionFeaturePlugIn>();
        ProgressionFeaturePlugIn.GetWorld(gameContext);
        Assert.That(gameContext.Configuration.MaximumMasterLevel, Is.Zero);
    }

    /// <summary>
    /// Two game servers share one game configuration: the second one does not take the caps of the first as the
    /// stored ones, the caps stay lowered while one server still runs the progression, and the last one off puts the
    /// stored caps back.
    /// </summary>
    [Test]
    public void TwoServersOnOneConfigurationRestoreTheStoredCaps()
    {
        var first = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars } };
        var second = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars } };
        first.GetWorld(this._configuration);
        second.GetWorld(this._configuration);

        first.OnDeactivated();
        Assert.That(this._configuration.MaximumMasterLevel, Is.Zero, "the second server still runs the stars");

        second.OnDeactivated();
        Assert.That(this._configuration.MaximumMasterLevel, Is.EqualTo(1350));
        Assert.That(this._configuration.MaximumLevel, Is.EqualTo(400));
    }

    /// <summary>
    /// The attack speed limit replaces the stored maximum of the attack and magic speed (0 is none, as S21Emu
    /// DisableAttackSpeedLimit), and the stored one comes back when the plugin is off; no limit keeps the database's.
    /// </summary>
    [Test]
    public void AttackSpeedLimitReplacesTheStoredOne()
    {
        var attackSpeed = new Persistence.BasicModel.AttributeDefinition(Stats.AttackSpeed.Id, "Attack Speed", string.Empty) { MaximumValue = 200 };
        var magicSpeed = new Persistence.BasicModel.AttributeDefinition(Stats.MagicSpeed.Id, "Magic Speed", string.Empty) { MaximumValue = 200 };
        this._configuration.Attributes.Add(attackSpeed);
        this._configuration.Attributes.Add(magicSpeed);
        var plugIn = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original } };

        plugIn.GetWorld(this._configuration);
        Assert.That(attackSpeed.MaximumValue, Is.EqualTo(200), "no limit keeps the database's");

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original, AttackSpeedLimit = 0 };
        plugIn.GetWorld(this._configuration);
        Assert.That(attackSpeed.MaximumValue, Is.Null);
        Assert.That(magicSpeed.MaximumValue, Is.Null);

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original, AttackSpeedLimit = 450 };
        plugIn.GetWorld(this._configuration);
        Assert.That(attackSpeed.MaximumValue, Is.EqualTo(450));

        plugIn.OnDeactivated();
        Assert.That(attackSpeed.MaximumValue, Is.EqualTo(200));
        Assert.That(magicSpeed.MaximumValue, Is.EqualTo(200));
    }

    /// <summary>
    /// The stars give 1 % more damage against monsters and 1 % less damage from them per star, capped at 10 %, and
    /// nothing against players by default; it goes away with the map or the plugin.
    /// </summary>
    [Test]
    public async Task StarPowerIsCappedAndPvmOnlyAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars } };
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        var damageIncrease = player.Attributes![Stats.AttackDamageIncrease];
        player.Attributes[Stats.Resets] = 3;

        await plugIn.ObjectAddedToMapAsync(player.CurrentMap!, player).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.FinalDamageIncreasePvm], Is.EqualTo(0.03f).Within(1e-6));
        Assert.That(player.Attributes[Stats.FinalDamageReceiveDecrementPvm], Is.EqualTo(0.03f).Within(1e-6));
        Assert.That(player.Attributes[Stats.AttackDamageIncrease], Is.EqualTo(damageIncrease));

        player.Attributes[Stats.Resets] = 15;
        await plugIn.ObjectAddedToMapAsync(player.CurrentMap!, player).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.FinalDamageIncreasePvm], Is.EqualTo(0.10f).Within(1e-6));

        plugIn.OnDeactivated();
        Assert.That(player.Attributes[Stats.FinalDamageIncreasePvm], Is.Zero);

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars, StarPowerInPvp = true };
        await plugIn.ObjectAddedToMapAsync(player.CurrentMap!, player).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.FinalDamageIncreasePvm], Is.Zero);
        Assert.That(player.Attributes[Stats.AttackDamageIncrease], Is.EqualTo(damageIncrease + 0.10f).Within(1e-6));

        await plugIn.ObjectRemovedFromMapAsync(player.CurrentMap!, player).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.AttackDamageIncrease], Is.EqualTo(damageIncrease).Within(1e-6));
    }

    /// <summary>
    /// In the stars mode, item level requirements check the power level: a one star level 300 character wears level
    /// 400 gear, a fresh one star level 10 character does not.
    /// </summary>
    [Test]
    public async Task ItemsCheckThePowerLevelInStarsAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        var plugIn = new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars } };
        player.Attributes![Stats.Level] = 300;
        player.Attributes[Stats.Resets] = 1;

        Assert.That(player.GetItemRequirementLevel(), Is.Null, "the feature is off");

        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        Assert.That(player.GetItemRequirementLevel(), Is.EqualTo(400));
        player.Attributes[Stats.Level] = 10;
        Assert.That(player.GetItemRequirementLevel(), Is.EqualTo(110));

        plugIn.Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        Assert.That(player.GetItemRequirementLevel(), Is.Null);
    }

    private static SetMonster Row(MonsterDefinition monster) => new(monster.Number, monster.Designation, ProgressionWorld.GetSourceRow(monster), true);

    private static MonsterDefinition CreateMonster(short number, string name, int level, int health, int damageMin, int damageMax, int defense, int attackRate, int defenseRate)
    {
        var monster = new MonsterDefinition { Id = Guid.NewGuid(), Number = number, Designation = name, ObjectKind = NpcObjectKind.Monster };
        (AttributeSystem.AttributeDefinition Stat, float Value)[] values =
        [
            (Stats.Level, level), (Stats.MaximumHealth, health), (Stats.MinimumPhysBaseDmg, damageMin), (Stats.MaximumPhysBaseDmg, damageMax),
            (Stats.DefenseBase, defense), (Stats.AttackRatePvm, attackRate), (Stats.DefenseRatePvm, defenseRate),
        ];
        foreach (var (stat, value) in values)
        {
            monster.Attributes.Add(new MonsterAttribute { AttributeDefinition = stat, Value = value });
        }

        return monster;
    }

    private static void Spawn(GameMapDefinition map, MonsterDefinition monster)
    {
        map.MonsterSpawns.Add(new MonsterSpawnArea { MonsterDefinition = monster, GameMap = map, X1 = 10, X2 = 20, Y1 = 10, Y2 = 20, Quantity = 5 });
    }

    private GameMapDefinition AddMap(short number, GameMapDefinition? safezone, int? season21Requirement)
    {
        var map = new GameMapDefinition { Id = Guid.NewGuid(), Number = number, SafezoneMap = safezone, IsActive = true };
        if (season21Requirement is { } requirement)
        {
            map.MapRequirements.Add(new AttributeRequirement { Attribute = Stats.TotalLevel, MinimumValue = requirement });
        }

        this._configuration.Maps.Add(map);
        return map;
    }
}
