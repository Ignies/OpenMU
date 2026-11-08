// <copyright file="Season21WorldSystemsDataTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.HelperPlus;
using MUnique.OpenMU.GameLogic.HuntingRecord;
using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.GameLogic.PlugIns.InvasionEvents;
using MUnique.OpenMU.GameLogic.SeasonPass;
using MUnique.OpenMU.GameLogic.SoulConverter;
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
    /// The update also adds the Underworld (Death King with 110 Death Bones on Lorencia or Noria, from 00:05) and the
    /// Summon invasion (Golden Goblin 652 on one of six maps, from 01:25) of S21Emu <c>InvasionManager.xml</c>.
    /// </summary>
    [Test]
    public async Task UnderworldAndSummonInvasionsAreAddedAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21GoldenInvasionUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(gameConfiguration.Monsters.Select(m => m.Number), Is.SupersetOf(new short[] { 55, 56, 652 }));
        var deathKing = gameConfiguration.Monsters.Single(m => m.Number == 55);
        Assert.That(deathKing.Attributes.Single(a => a.AttributeDefinition == Stats.MaximumHealth).Value, Is.EqualTo(30000));
        Assert.That(gameConfiguration.Monsters.Single(m => m.Number == 56).Attributes.Single(a => a.AttributeDefinition == Stats.Level).Value, Is.EqualTo(24));
        Assert.That(gameConfiguration.Monsters.Single(m => m.Number == 652).Attributes.Single(a => a.AttributeDefinition == Stats.MaximumHealth).Value, Is.EqualTo(120000));

        var underworld = GetConfiguration<PeriodicInvasionConfiguration>(gameConfiguration, typeof(UnderworldInvasionPlugIn).GUID, out var underworldPlugIn);
        Assert.That(underworldPlugIn.IsActive, Is.True);
        Assert.That(underworld.ForceSingleMap, Is.True);
        Assert.That(underworld.Mobs.Select(m => (m.MonsterId, m.Count)), Is.EqualTo(new (ushort, ushort)[] { (55, 1), (56, 110) }));
        Assert.That(underworld.Mobs[0].MapIds, Is.EqualTo(new ushort[] { 0, 3 }));
        Assert.That(underworld.Timetable[0], Is.EqualTo(new TimeOnly(0, 5)));
        Assert.That(underworld.TaskDuration, Is.EqualTo(TimeSpan.FromMinutes(10)));

        var summon = GetConfiguration<PeriodicInvasionConfiguration>(gameConfiguration, typeof(SummonInvasionPlugIn).GUID, out var summonPlugIn);
        Assert.That(summonPlugIn.IsActive, Is.True);
        Assert.That(summon.Mobs.Single().MapIds, Is.EqualTo(new ushort[] { 8, 33, 37, 80, 81, 91 }));
        Assert.That(summon.Timetable[0], Is.EqualTo(new TimeOnly(1, 25)));
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

    /// <summary>
    /// The soul converter update sets the five tiers of <c>MonsterSoulConverter/</c> with their missions and the reward bags
    /// 288-355, three activations a week, and switches the converter on.
    /// </summary>
    [Test]
    public async Task SoulConverterUpdateSetsTheTiersOfTheDataAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21SoulConverterUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var configuration = GetConfiguration<SoulConverterConfiguration>(gameConfiguration, typeof(SoulConverterPlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That(configuration.ActivationsPerWeek, Is.EqualTo(3));
        Assert.That(configuration.Tiers.Select(t => (t.Tier, t.ItemNumber, t.MinimumTotalLevel)), Is.EqualTo(new[] { (1, 460, 400), (2, 461, 800), (3, 462, 1150), (4, 463, 1250), (5, 464, 1450) }.Select(t => ((byte)t.Item1, (short)t.Item2, t.Item3))));
        var first = configuration.Tiers[0];
        Assert.That((first.ItemGroup, first.MissionCount, first.Missions.Count), Is.EqualTo(((byte)14, 2, 6)));
        Assert.That((first.Missions[0].MapNumber, first.Missions[0].MonsterNumber, first.Missions[0].RequiredSouls, first.Missions[0].AbsorbRate), Is.EqualTo(((short)33, (short)308, 500, 450_000)));
        Assert.That(first.Rewards.Select(r => r.Kind), Is.EqualTo(new[] { SoulRewardKind.Ruud, SoulRewardKind.Jewels, SoulRewardKind.Options }));
        Assert.That(configuration.Tiers[4].Rewards.Select(r => r.Kind), Does.Contain(SoulRewardKind.Accessory));
        var ruud = first.Rewards[0].Draws.Single(d => d.Items.Count == 0);
        Assert.That((ruud.Chance, ruud.MinimumRuud, ruud.MaximumRuud), Is.EqualTo((10_000, 400, 1000)));
        var jewels = first.Rewards[1].Draws[0].Items.Single();
        Assert.That((jewels.Group, jewels.Number, jewels.Count), Is.EqualTo(((byte)14, (short)18, (byte)10)), "Ten Jewels of Soul.");
        Assert.That(GameLogic.ForkInfo.Has("soul_converter"), Is.True);
    }

    /// <summary>
    /// Applied twice, the soul converter update keeps the tiers and the switch of the operator; it is optional.
    /// </summary>
    [Test]
    public async Task SoulConverterUpdateKeepsTheOperatorsTiersAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21SoulConverterUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(SoulConverterPlugIn).GUID);
        var custom = plugIn.GetConfiguration<SoulConverterConfiguration>(new IdIgnoringReferenceHandler())!;
        custom.Tiers.RemoveAt(4);
        plugIn.SetConfiguration(custom, null);
        plugIn.IsActive = false;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(GetConfiguration<SoulConverterConfiguration>(gameConfiguration, typeof(SoulConverterPlugIn).GUID, out _).Tiers, Has.Count.EqualTo(4));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(update.IsMandatory, Is.False);
    }

    /// <summary>
    /// The helper plus update sets the 31 hunting grounds with their warps and the 330 zones of <c>MuHelperPlus.xml</c> and
    /// switches the helper plus on; every ground's warp is one of the maps update and every zone lands on walkable ground.
    /// </summary>
    [Test]
    public async Task HelperPlusUpdateSetsTheZonesOfTheDataAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21HelperPlusUpdatePlugIn();

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var configuration = GetConfiguration<HelperPlusConfiguration>(gameConfiguration, typeof(HelperPlusPlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That((configuration.Grounds.Count, configuration.Zones.Count), Is.EqualTo((31, 330)));
        Assert.That((configuration.Grounds[0].WarpIndex, configuration.Grounds[0].Name), Is.EqualTo((49, "Debenter")));
        var zone = configuration.Zones[0];
        Assert.That((zone.Index, zone.Ground, zone.MapNumber, zone.Element, zone.X, zone.Y), Is.EqualTo(((short)0, (short)0, (short)95, (byte)1, (byte)190, (byte)98)));
        Assert.That(configuration.Grounds.Select(g => g.WarpIndex).Distinct().Where(w => gameConfiguration.WarpList.All(i => i.Index != w)), Is.Empty, "Every ground's warp is one of the maps update.");
        var unreachable = configuration.Zones
            .Where(z => gameConfiguration.Maps.FirstOrDefault(m => m.Number == z.MapNumber) is not { } map || !new GameLogic.GameMapTerrain(map).WalkMap[z.X, z.Y])
            .Select(z => $"zone {z.Index} on map {z.MapNumber} at {z.X}/{z.Y}")
            .ToList();
        Assert.That(unreachable, Is.Empty, "Every zone's hunting point is walkable ground of its map.");
        Assert.That(configuration.Zones.Where(z => z.Ground == 30).Select(z => z.MapNumber).Distinct(), Is.EqualTo(new short[] { 145 }), "Aquilas Temple.");
        Assert.That(update.DependsOn.Select(d => d.Key), Does.Contain(new AddSeason21MapsUpdatePlugIn().Key));
        Assert.That(GameLogic.ForkInfo.Has("helper_plus"), Is.True);
    }

    /// <summary>
    /// Applied twice, the helper plus update keeps the zones and the switch of the operator; it is optional.
    /// </summary>
    [Test]
    public async Task HelperPlusUpdateKeepsTheOperatorsZonesAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21HelperPlusUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(HelperPlusPlugIn).GUID);
        var custom = plugIn.GetConfiguration<HelperPlusConfiguration>(new IdIgnoringReferenceHandler())!;
        custom.Zones.RemoveRange(1, custom.Zones.Count - 1);
        plugIn.SetConfiguration(custom, null);
        plugIn.IsActive = false;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(GetConfiguration<HelperPlusConfiguration>(gameConfiguration, typeof(HelperPlusPlugIn).GUID, out _).Zones, Has.Count.EqualTo(1));
        Assert.That(plugIn.IsActive, Is.False);
        Assert.That(update.IsMandatory, Is.False);
    }

    /// <summary>
    /// The season pass update sets the schedule, levels, missions, rewards and buffs of <c>BattlePass.xml</c> and switches the
    /// season pass on. The missions have no kind yet (the data only numbers them); the Max HP buff raises the maximum health.
    /// </summary>
    [Test]
    public async Task SeasonPassUpdateSetsThePassOfTheDataAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);

        await new AddSeason21SeasonPassUpdatePlugIn().ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        var configuration = GetConfiguration<SeasonPassConfiguration>(gameConfiguration, typeof(SeasonPassPlugIn).GUID, out var plugIn);
        Assert.That(plugIn.IsActive, Is.True);
        Assert.That((configuration.GoldPassEnabled, configuration.PremiumPassEnabled, configuration.TicketExperience, configuration.RepeatEveryYear), Is.EqualTo((true, false, 300, true)));
        Assert.That(configuration.Seasons, Has.Count.EqualTo(12));
        Assert.That((configuration.Seasons[0].Number, configuration.Seasons[0].Start, configuration.Seasons[0].End), Is.EqualTo(((ushort)1, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 23, 59, 0, DateTimeKind.Utc))));
        Assert.That(configuration.LevelExperience, Has.Count.EqualTo(20));
        Assert.That((configuration.LevelExperience[0], configuration.LevelExperience[19]), Is.EqualTo((100, 6000)));
        Assert.That(configuration.Missions, Has.Count.EqualTo(26));
        Assert.That(configuration.Missions.Select(m => m.Kind), Has.All.EqualTo(SeasonPassMissionKind.None));
        var first = configuration.Missions[0];
        Assert.That((first.Index, first.Experience, first.Required, first.Repeat, first.IsSpecial), Is.EqualTo(((byte)0, 5, 2160, 10, false)));
        var special = configuration.Missions.Single(m => m.IsSpecial);
        Assert.That((special.Index, special.Experience, special.Required, special.Repeat), Is.EqualTo(((byte)0, 10, 21600, 2)));
        Assert.That(configuration.Rewards, Has.Count.EqualTo(14));
        var bless = configuration.Rewards.Single(r => r.Id == 5);
        Assert.That((bless.EffectIndex, bless.ItemGroup, bless.ItemNumber, bless.Count), Is.EqualTo((-1, (byte)14, (short)371, (byte)3)));
        Assert.That(configuration.LevelRewards.Single(r => r.Pass == SeasonPassKind.Free && r.Level == 1).RewardIds, Is.EqualTo(new[] { 0 }));
        Assert.That(configuration.LevelRewards.Single(r => r.Pass == SeasonPassKind.Gold && r.Level == 2).RewardIds, Is.EqualTo(new[] { 10 }));
        var health = configuration.Buffs.Single(b => b.EffectIndex == 411);
        Assert.That((health.AttributeId, health.Values[0], health.Values[19]), Is.EqualTo(((Guid?)Stats.MaximumHealth.Id, 30, 300)));
        Assert.That(configuration.Buffs.Where(b => b.EffectIndex != 411).Select(b => b.AttributeId), Has.All.Null);
        Assert.That(GameLogic.ForkInfo.Has("battle_pass"), Is.True);
    }

    /// <summary>
    /// Applied twice, the season pass update keeps the missions and the switch of the operator; it is optional.
    /// </summary>
    [Test]
    public async Task SeasonPassUpdateKeepsTheOperatorsPassAsync()
    {
        var (context, gameConfiguration) = await CreateAsync().ConfigureAwait(false);
        var update = new AddSeason21SeasonPassUpdatePlugIn();
        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);
        var plugIn = gameConfiguration.PlugInConfigurations.Single(c => c.TypeId == typeof(SeasonPassPlugIn).GUID);
        var custom = plugIn.GetConfiguration<SeasonPassConfiguration>(new IdIgnoringReferenceHandler())!;
        custom.Missions[0].Kind = SeasonPassMissionKind.PlaySeconds;
        plugIn.SetConfiguration(custom, null);
        plugIn.IsActive = false;

        await update.ApplyUpdateAsync(context, gameConfiguration).ConfigureAwait(false);

        Assert.That(GetConfiguration<SeasonPassConfiguration>(gameConfiguration, typeof(SeasonPassPlugIn).GUID, out _).Missions[0].Kind, Is.EqualTo(SeasonPassMissionKind.PlaySeconds));
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
