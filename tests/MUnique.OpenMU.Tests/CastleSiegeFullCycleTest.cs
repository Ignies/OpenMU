// <copyright file="CastleSiegeFullCycleTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using System.Collections.Immutable;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.CastleSiege;
using MUnique.OpenMU.GameLogic.CastleSiege.Actions;
using MUnique.OpenMU.GameLogic.CastleSiege.NPC;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.InMemory;
using MUnique.OpenMU.PlugIns;
using BasicModel = MUnique.OpenMU.Persistence.BasicModel;
using RuntimeGuild = MUnique.OpenMU.Interfaces.Guild;

/// <summary>
/// Runs a whole Castle Siege cycle with the <c>/siege</c> game master command: registration, mark, selection,
/// battle, crown capture, battle end and the reset of the next cycle.
/// </summary>
[TestFixture]
public class CastleSiegeFullCycleTest
{
    private const uint DefenseGuildId = 10;
    private const uint AttackGuildId = 20;

    /// <summary>
    /// The attackers register, take the crown and win: the end of the battle makes them the persisted castle owner
    /// with a fresh economy, rewards the participants and despawns the machines; the cycle end clears the
    /// registrations, the selected guilds, the sides and the castle NPCs, and opens the next registration.
    /// </summary>
    [Test]
    public async ValueTask GameMasterRunsAWholeSiegeAsync()
    {
        var fixture = await CreateFixtureAsync().ConfigureAwait(false);
        var attacker = await fixture.CreatePlayerAsync(AttackGuildId, "Attacker", 60, 60).ConfigureAwait(false);
        var defender = await fixture.CreatePlayerAsync(DefenseGuildId, "Defender", 90, 200).ConfigureAwait(false);
        attacker.SelectedCharacter!.CharacterStatus = CharacterStatus.GameMaster;
        var command = new SiegeChatCommandPlugIn(player => fixture.PlugIn.GetContext(player.GameContext));

        async ValueTask ForceAsync(string state)
        {
            await command.HandleCommandAsync(attacker, $"/siege {state}").ConfigureAwait(false);
            await fixture.TickAsync().ConfigureAwait(false);
        }

        await fixture.TickAsync().ConfigureAwait(false);
        var context = fixture.PlugIn.GetContext(fixture.Server)!;
        Assert.That(context.CurrentState, Is.EqualTo(CastleSiegeState.RegisterGuild));
        context.SiegeData.TributeMoney = 5_000;
        context.SiegeData.TaxStore = 3;

        await new CastleSiegeRegisterGuildAction().RegisterAsync(attacker, context).ConfigureAwait(false);
        Assert.That(context.RegisteredGuilds, Contains.Key(fixture.AttackPersistentGuildId));

        await ForceAsync("mark").ConfigureAwait(false);
        Assert.That(context.CurrentState, Is.EqualTo(CastleSiegeState.RegisterMark));
        await context.IncrementMarksAsync(context.RegisteredGuilds[fixture.AttackPersistentGuildId]).ConfigureAwait(false);

        await ForceAsync("notify").ConfigureAwait(false);
        Assert.That(context.FinalGuildList.Values.Select(g => (g.PersistentGuildId, g.Side)), Is.EquivalentTo(new[]
        {
            (fixture.AttackPersistentGuildId, CastleSiegeJoinSide.Attack1),
            (fixture.DefensePersistentGuildId, CastleSiegeJoinSide.Defense),
        }));

        await ForceAsync("ready").ConfigureAwait(false);
        Assert.That(context.GetPlayerJoinSide(attacker), Is.EqualTo(CastleSiegeJoinSide.Attack1));
        Assert.That(context.GetPlayerJoinSide(defender), Is.EqualTo(CastleSiegeJoinSide.Defense));

        await command.HandleCommandAsync(attacker, "/siege crown").ConfigureAwait(false);
        await ForceAsync("start").ConfigureAwait(false);
        Assert.That(context.IsEventRunning, Is.True);
        context.CrownUser = attacker;
        for (var second = 0; second <= fixture.Configuration.CrownHoldTimeSeconds; second++)
        {
            fixture.Clock.Advance(TimeSpan.FromSeconds(1));
            await fixture.TickAsync().ConfigureAwait(false);
        }

        Assert.That(context.MiddleOwnerGuildId, Is.EqualTo(fixture.AttackPersistentGuildId));
        Assert.That(context.GetPlayerJoinSide(attacker), Is.EqualTo(CastleSiegeJoinSide.Defense));

        await ForceAsync("end").ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentState, Is.EqualTo(CastleSiegeState.End));
            Assert.That(context.SiegeData.OwnerGuildId, Is.EqualTo(fixture.AttackPersistentGuildId));
            Assert.That(context.SiegeData.IsOccupied, Is.True);
            Assert.That(context.SiegeData.TributeMoney + context.SiegeData.TaxStore, Is.Zero);
            Assert.That(attacker.Inventory!.Items.Count(item => item.Definition == fixture.Configuration.RewardItemDefinition), Is.EqualTo(1));
            Assert.That(defender.Inventory!.Items.Count(item => item.Definition == fixture.Configuration.RewardItemDefinition), Is.EqualTo(1));
        });

        fixture.Clock.Advance(TimeSpan.FromMinutes(6));
        await fixture.TickAsync().ConfigureAwait(false);
        var restarted = new CastleSiegeContext(fixture.Server, fixture.Configuration);
        await restarted.InitializeAsync(fixture.Clock.GetUtcNow().UtcDateTime).ConfigureAwait(false);
        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentState, Is.EqualTo(CastleSiegeState.RegisterGuild));
            Assert.That(context.RegisteredGuilds, Is.Empty);
            Assert.That(context.FinalGuildList, Is.Empty);
            Assert.That(context.PlayerJoinSides, Is.Empty);
            Assert.That(context.GetPlayerJoinSide(attacker), Is.EqualTo(CastleSiegeJoinSide.None));
            Assert.That(context.NpcController.GetRuntimeSnapshot().Where(npc => npc.SpawnedInstance is not null), Is.Empty);
            Assert.That(context.MiddleOwnerGuildId, Is.Null);
            Assert.That(context.SwitchesWaived, Is.False);
            Assert.That(restarted.SiegeData.OwnerGuildId, Is.EqualTo(fixture.AttackPersistentGuildId));
            Assert.That(restarted.RegisteredGuilds, Is.Empty);
            Assert.That(restarted.FinalGuildList, Is.Empty);
        });
    }

    /// <summary>
    /// A game master who opens the registration again after the battle (<c>/siege end</c>, then <c>/siege register</c>)
    /// skips the cycle end: the new cycle still starts clean, without the old registrations, selected guilds, sides
    /// and castle NPCs.
    /// </summary>
    /// <param name="lastState">The state the game master leaves.</param>
    [TestCase("end")]
    [TestCase("start")]
    [TestCase("ready")]
    public async ValueTask RegistrationForcedAfterTheSelectionStartsACleanCycleAsync(string lastState)
    {
        var fixture = await CreateFixtureAsync().ConfigureAwait(false);
        var attacker = await fixture.CreatePlayerAsync(AttackGuildId, "Attacker", 60, 60).ConfigureAwait(false);
        attacker.SelectedCharacter!.CharacterStatus = CharacterStatus.GameMaster;
        var command = new SiegeChatCommandPlugIn(player => fixture.PlugIn.GetContext(player.GameContext));

        async ValueTask ForceAsync(string state)
        {
            await command.HandleCommandAsync(attacker, $"/siege {state}").ConfigureAwait(false);
            await fixture.TickAsync().ConfigureAwait(false);
        }

        await fixture.TickAsync().ConfigureAwait(false);
        var context = fixture.PlugIn.GetContext(fixture.Server)!;
        await new CastleSiegeRegisterGuildAction().RegisterAsync(attacker, context).ConfigureAwait(false);
        await ForceAsync("mark").ConfigureAwait(false);
        await context.IncrementMarksAsync(context.RegisteredGuilds[fixture.AttackPersistentGuildId]).ConfigureAwait(false);
        await ForceAsync("notify").ConfigureAwait(false);
        await ForceAsync("ready").ConfigureAwait(false);
        if (lastState != "ready")
        {
            await ForceAsync("start").ConfigureAwait(false);
        }

        if (lastState == "end")
        {
            await ForceAsync("end").ConfigureAwait(false);
        }

        await ForceAsync("register").ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(context.CurrentState, Is.EqualTo(CastleSiegeState.RegisterGuild));
            Assert.That(context.RegisteredGuilds, Is.Empty);
            Assert.That(context.FinalGuildList, Is.Empty);
            Assert.That(context.PlayerJoinSides, Is.Empty);
            Assert.That(context.NpcController.GetRuntimeSnapshot().Where(npc => npc.SpawnedInstance is not null), Is.Empty);
        });
    }

    private static async ValueTask<TestFixture> CreateFixtureAsync()
    {
        var persistenceContextProvider = new InMemoryPersistenceContextProvider();
        BasicModel.GameConfiguration gameConfiguration;
        BasicModel.CastleSiegeConfiguration configuration;
        Guid defensePersistentGuildId;
        Guid attackPersistentGuildId;
        using (var persistenceContext = persistenceContextProvider.CreateNewContext())
        {
            gameConfiguration = persistenceContext.CreateNew<BasicModel.GameConfiguration>();
            var normalMap = persistenceContext.CreateNew<BasicModel.GameMapDefinition>();
            normalMap.Number = 0;
            normalMap.TerrainData = new byte[65_539];
            gameConfiguration.Maps.Add(normalMap);
            var siegeMap = persistenceContext.CreateNew<BasicModel.GameMapDefinition>();
            siegeMap.Number = 30;
            siegeMap.TerrainData = new byte[65_539];
            gameConfiguration.Maps.Add(siegeMap);

            configuration = persistenceContext.CreateNew<BasicModel.CastleSiegeConfiguration>();
            configuration.Enabled = true;
            configuration.CastleSiegeMapDefinition = siegeMap;
            configuration.CrownHoldTimeSeconds = 3;
            configuration.RegisterMinLevel = 400;
            configuration.RegisterMinMembers = 3;
            configuration.AttackRespawnArea = CreateZone(35, 11, 40, 16);
            configuration.DefenseRespawnArea = CreateZone(74, 144, 80, 150);
            var reward = persistenceContext.CreateNew<BasicModel.ItemDefinition>();
            reward.Name = "Reward";
            reward.Width = 1;
            reward.Height = 1;
            reward.Durability = 1;
            gameConfiguration.Items.Add(reward);
            configuration.RewardItemDefinition = reward;
            AddSchedule(CastleSiegeState.Idle1, DayOfWeek.Sunday, 0, 0);
            AddSchedule(CastleSiegeState.RegisterGuild, DayOfWeek.Monday, 0, 0);
            AddSchedule(CastleSiegeState.Idle2, DayOfWeek.Tuesday, 0, 0);
            AddSchedule(CastleSiegeState.RegisterMark, DayOfWeek.Wednesday, 0, 0);
            AddSchedule(CastleSiegeState.Idle3, DayOfWeek.Thursday, 0, 0);
            AddSchedule(CastleSiegeState.Notify, DayOfWeek.Friday, 0, 0);
            AddSchedule(CastleSiegeState.Ready, DayOfWeek.Saturday, 18, 0);
            AddSchedule(CastleSiegeState.Start, DayOfWeek.Saturday, 20, 0);
            AddSchedule(CastleSiegeState.End, DayOfWeek.Saturday, 22, 0);
            AddSchedule(CastleSiegeState.EndCycle, DayOfWeek.Saturday, 22, 5);
            gameConfiguration.CastleSiegeConfiguration = configuration;
            AddNpc(CastleSiegeCrown.MonsterNumber, 1, 60, 60);
            AddNpc(CastleSiegeSwitch.FirstMonsterNumber, 1, 70, 60);
            AddNpc(CastleSiegeSwitch.SecondMonsterNumber, 2, 80, 60);

            var defenseGuild = persistenceContext.CreateNew<BasicModel.Guild>();
            defenseGuild.Name = "Defenders";
            defensePersistentGuildId = defenseGuild.Id;
            var attackGuild = persistenceContext.CreateNew<BasicModel.Guild>();
            attackGuild.Name = "Attackers";
            attackPersistentGuildId = attackGuild.Id;
            var siegeData = persistenceContext.CreateNew<BasicModel.CastleSiegeData>();
            siegeData.OwnerGuildId = defensePersistentGuildId;
            siegeData.IsOccupied = true;
            await persistenceContext.SaveChangesAsync().ConfigureAwait(false);

            void AddSchedule(CastleSiegeState state, DayOfWeek day, byte hour, byte minute)
            {
                configuration.StateSchedule.Add(new BasicModel.CastleSiegeStateScheduleEntry { State = state, DayOfWeek = day, Hour = hour, Minute = minute });
            }

            BasicModel.CastleSiegeZoneDefinition CreateZone(byte x1, byte y1, byte x2, byte y2)
            {
                var zone = persistenceContext.CreateNew<BasicModel.CastleSiegeZoneDefinition>();
                zone.X1 = x1;
                zone.Y1 = y1;
                zone.X2 = x2;
                zone.Y2 = y2;
                return zone;
            }

            void AddNpc(short monsterNumber, byte instanceId, byte x, byte y)
            {
                var monster = persistenceContext.CreateNew<BasicModel.MonsterDefinition>();
                monster.Number = monsterNumber;
                monster.ObjectKind = NpcObjectKind.PassiveNpc;
                gameConfiguration.Monsters.Add(monster);
                configuration.NpcDefinitions.Add(new BasicModel.CastleSiegeNpcDefinition
                {
                    MonsterDefinition = monster,
                    InstanceId = instanceId,
                    SpawnX = x,
                    SpawnY = y,
                    Direction = Direction.South,
                });
            }
        }

        var guildServer = new Mock<IGuildServer>();
        SetupGuild(DefenseGuildId, defensePersistentGuildId, "Defenders", "Defender");
        SetupGuild(AttackGuildId, attackPersistentGuildId, "Attackers", "Attacker");
        guildServer
            .Setup(server => server.GetAllianceGuildsAsync(It.IsAny<uint>()))
            .Returns(new ValueTask<IImmutableList<AllianceGuildEntry>>(ImmutableList<AllianceGuildEntry>.Empty));

        void SetupGuild(uint runtimeId, Guid persistentId, string name, string master)
        {
            guildServer.Setup(server => server.GetGuildIdAsync(persistentId)).Returns(new ValueTask<uint>(runtimeId));
            guildServer.Setup(server => server.GetGuildAsync(runtimeId)).Returns(new ValueTask<RuntimeGuild?>(new RuntimeGuild { Name = name }));
            guildServer.Setup(server => server.GetGuildIdByNameAsync(name)).Returns(new ValueTask<uint>(runtimeId));
            guildServer.Setup(server => server.GetPersistentGuildIdAsync(runtimeId)).Returns(new ValueTask<Guid?>(persistentId));
            guildServer.Setup(server => server.GetPersistentAllianceMasterGuildIdAsync(runtimeId)).Returns(new ValueTask<Guid?>(persistentId));
            guildServer.Setup(server => server.IsAllianceMasterAsync(runtimeId)).Returns(new ValueTask<bool>(true));
            var members = ImmutableList.Create(
                new GuildListEntry { PlayerName = master, PlayerPosition = GuildPosition.GuildMaster },
                new GuildListEntry { PlayerName = master + "2", PlayerPosition = GuildPosition.NormalMember },
                new GuildListEntry { PlayerName = master + "3", PlayerPosition = GuildPosition.NormalMember });
            guildServer.Setup(server => server.GetGuildListAsync(runtimeId)).Returns(new ValueTask<IImmutableList<GuildListEntry>>(members));
        }

        var plugInManager = new PlugInManager(null, NullLoggerFactory.Instance, null, null);
        var mapInitializer = new MapInitializer(gameConfiguration, new NullLogger<MapInitializer>(), NullDropGenerator.Instance, null);
        var server = new GameServerContext(
            new BasicModel.GameServerDefinition
            {
                GameConfiguration = gameConfiguration,
                ServerConfiguration = new BasicModel.GameServerConfiguration(),
            },
            guildServer.Object,
            new Mock<IEventPublisher>().Object,
            new Mock<ILoginServer>().Object,
            new Mock<IFriendServer>().Object,
            persistenceContextProvider,
            mapInitializer,
            NullLoggerFactory.Instance,
            plugInManager,
            NullDropGenerator.Instance,
            new ConfigurationChangeMediator());
        server.ServerTimeZone = TimeZoneInfo.Utc;
        mapInitializer.PlugInManager = server.PlugInManager;
        mapInitializer.PathFinderPool = server.PathFinderPool;

        var clock = new ManualClock(new DateTimeOffset(2026, 8, 3, 12, 0, 0, TimeSpan.Zero));
        return new(configuration, server, new CastleSiegePlugIn(clock), clock, defensePersistentGuildId, attackPersistentGuildId);
    }

    private sealed record TestFixture(
        CastleSiegeConfiguration Configuration,
        GameServerContext Server,
        CastleSiegePlugIn PlugIn,
        ManualClock Clock,
        Guid DefensePersistentGuildId,
        Guid AttackPersistentGuildId)
    {
        internal ValueTask TickAsync() => this.PlugIn.ExecuteTaskAsync(this.Server);

        internal async ValueTask<Player> CreatePlayerAsync(uint guildId, string characterName, byte x, byte y)
        {
            var player = await PlayerTestHelper.CreatePlayerAsync(this.Server).ConfigureAwait(false);
            player.SelectedCharacter!.Id = Guid.NewGuid();
            player.SelectedCharacter.Name = characterName;
            player.GuildStatus = new GuildMemberStatus(guildId, GuildPosition.GuildMaster);
            player.Attributes![Stats.Level] = 400;
            await this.Server.AddPlayerAsync(player).ConfigureAwait(false);
            await player.WarpToAsync(new ExitGate
            {
                Map = this.Configuration.CastleSiegeMapDefinition,
                X1 = x,
                X2 = x,
                Y1 = y,
                Y2 = y,
                Direction = Direction.South,
            }).ConfigureAwait(false);
            await player.ClientReadyAfterMapChangeAsync().ConfigureAwait(false);
            player.IsAlive = true;
            return player;
        }
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public ManualClock(DateTimeOffset utcNow)
        {
            this._utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => this._utcNow;

        public void Advance(TimeSpan time) => this._utcNow += time;
    }
}
