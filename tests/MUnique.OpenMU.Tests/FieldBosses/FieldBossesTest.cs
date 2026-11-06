// <copyright file="FieldBossesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.FieldBosses;

using System.ComponentModel;
using System.IO;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.FieldBosses;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.PlugIns.ChatCommands;
using MUnique.OpenMU.Pathfinding;
using MonsterAttribute = MUnique.OpenMU.Persistence.BasicModel.MonsterAttribute;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests the <see cref="FieldBossesFeaturePlugIn"/>: the spawn, stages, kill and respawn cycle and the arena rules.
/// </summary>
[TestFixture]
public class FieldBossesTest
{
    private const short GuardNumber = 900;
    private const short MinionNumber = 901;
    private const short BossNumber = 902;
    private const int Health = 1000;

    private DateTime _now;

    /// <summary>
    /// The boss spawns at the start, its stages follow each other, the kill moves it to defeated, and it comes back after
    /// the exit delay and the random 18 to 24 hours.
    /// </summary>
    [Test]
    public async Task SpawnKillRespawnCycleAsync()
    {
        var (plugIn, gameContext, killer) = await this.CreateAsync().ConfigureAwait(false);
        var state = plugIn.GetStates(gameContext).Single();

        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        Assert.That(state.Status, Is.EqualTo(FieldBossStatus.Alive));
        Assert.That(state.StageMonster?.Definition.Number, Is.EqualTo(GuardNumber));
        Assert.That(await CountAsync(gameContext, MinionNumber).ConfigureAwait(false), Is.EqualTo(2), "The summons of the stage.");

        await state.StageMonster!.ReflectDamageAsync(killer, Health * 10).ConfigureAwait(false);
        Assert.That(state.StageMonster?.Definition.Number, Is.EqualTo(BossNumber), "The next stage spawns.");
        Assert.That(state.Status, Is.EqualTo(FieldBossStatus.Alive));

        await state.StageMonster!.ReflectDamageAsync(killer, Health * 10).ConfigureAwait(false);
        Assert.That(state.Status, Is.EqualTo(FieldBossStatus.Defeated));
        Assert.That(state.KillerName, Is.EqualTo(killer.Name));
        Assert.That(state.NextChangeAt, Is.EqualTo(this._now + TimeSpan.FromMinutes(5)), "The players leave the arena 5 minutes after the kill.");
        Assert.That(state.RespawnAt, Is.InRange(this._now + TimeSpan.FromHours(18), this._now + TimeSpan.FromHours(24)));
        Assert.That(await CountAsync(gameContext, MinionNumber).ConfigureAwait(false), Is.Zero, "The summons leave with the boss.");

        this._now += TimeSpan.FromMinutes(5);
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        Assert.That(state.Status, Is.EqualTo(FieldBossStatus.Waiting));
        Assert.That(state.NextChangeAt, Is.EqualTo(state.RespawnAt));

        this._now = state.RespawnAt - TimeSpan.FromSeconds(1);
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        Assert.That(state.Status, Is.EqualTo(FieldBossStatus.Waiting));

        this._now = state.RespawnAt;
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        Assert.That(state.Status, Is.EqualTo(FieldBossStatus.Alive));
        Assert.That(state.StageMonster?.Definition.Number, Is.EqualTo(GuardNumber));
    }

    /// <summary>
    /// The gates into the arena are closed while the boss is away; the arena takes its maximum of players and closes
    /// once the fight started.
    /// </summary>
    [Test]
    public async Task ArenaEntryRulesAsync()
    {
        var (plugIn, gameContext, player) = await this.CreateAsync(spawnOnStart: false).ConfigureAwait(false);
        var state = plugIn.GetStates(gameContext).Single();
        var map = await gameContext.GetMapAsync(0).ConfigureAwait(false);
        var gate = new ExitGate { Map = map!.Definition, X1 = 100, Y1 = 100, X2 = 100, Y2 = 100 };

        var args = new CancelEventArgs();
        await plugIn.WarpGateEnteringAsync(player, gate, args).ConfigureAwait(false);
        Assert.That(args.Cancel, Is.True, "No boss, no entry.");

        await plugIn.SpawnAsync(gameContext, state.Definition).ConfigureAwait(false);
        args = new CancelEventArgs();
        await plugIn.WarpGateEnteringAsync(player, gate, args).ConfigureAwait(false);
        Assert.That(args.Cancel, Is.False);

        player.Position = new Point(100, 100);
        Assert.That(await plugIn.CheckArenaEntryAsync(gameContext, state, false).ConfigureAwait(false), Is.EqualTo(FieldBossesFeaturePlugIn.ArenaEntry.Full));

        player.Position = new Point(10, 10);
        plugIn.AttackableGotHit(state.StageMonster!, player, default);
        await WaitUntilAsync(() => state.Status == FieldBossStatus.Engaged).ConfigureAwait(false);
        Assert.That(await plugIn.CheckArenaEntryAsync(gameContext, state, false).ConfigureAwait(false), Is.EqualTo(FieldBossesFeaturePlugIn.ArenaEntry.Closed));
        Assert.That(state.NextChangeAt, Is.EqualTo(this._now + TimeSpan.FromMinutes(10)), "The battle time starts with the first hit.");
    }

    /// <summary>
    /// A battle which runs out of time is lost: the monsters leave and the boss comes back after the short failed respawn.
    /// </summary>
    [Test]
    public async Task LostBattleRespawnsSoonAsync()
    {
        var (plugIn, gameContext, player) = await this.CreateAsync().ConfigureAwait(false);
        var state = plugIn.GetStates(gameContext).Single();
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        plugIn.AttackableGotHit(state.StageMonster!, player, default);
        await WaitUntilAsync(() => state.Status == FieldBossStatus.Engaged).ConfigureAwait(false);

        this._now += TimeSpan.FromMinutes(10);
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);

        Assert.That(state.Status, Is.EqualTo(FieldBossStatus.Waiting));
        Assert.That(state.NextChangeAt, Is.EqualTo(this._now + TimeSpan.FromMinutes(5)));
        Assert.That(await CountAsync(gameContext, GuardNumber).ConfigureAwait(false), Is.Zero);
    }

    /// <summary>
    /// A health phase spawns its monsters once, when the stage monster falls to its percentage.
    /// </summary>
    [Test]
    public async Task HealthPhaseSpawnsOnceAsync()
    {
        var (plugIn, gameContext, player) = await this.CreateAsync().ConfigureAwait(false);
        var state = plugIn.GetStates(gameContext).Single();
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        var guard = state.StageMonster!;

        await guard.ReflectDamageAsync(player, Health * 6 / 10).ConfigureAwait(false);
        plugIn.AttackableGotHit(guard, player, default);
        await WaitUntilAsync(async () => await CountAsync(gameContext, MinionNumber).ConfigureAwait(false) == 5).ConfigureAwait(false);
        plugIn.AttackableGotHit(guard, player, default);
        await Task.Delay(100).ConfigureAwait(false);

        Assert.That(await CountAsync(gameContext, MinionNumber).ConfigureAwait(false), Is.EqualTo(5));
    }

    /// <summary>
    /// In a fight, an area strike hits the players near the stage monster, and a summon keeps its maximum of monsters.
    /// </summary>
    [Test]
    public async Task AbilitiesStrikeAndSummonAsync()
    {
        var (plugIn, gameContext, player) = await this.CreateAsync().ConfigureAwait(false);
        var stage = plugIn.Configuration!.Bosses[0].Stages[0];
        stage.Abilities.Add(new FieldBossAbility { Kind = FieldBossAbilityKind.AreaStrike, Distance = 5, DamagePercent = 100, IntervalSeconds = 2 });
        stage.Abilities.Add(new FieldBossAbility { Kind = FieldBossAbilityKind.Summon, MaximumAlive = 3, IntervalSeconds = 2, Summons = [new FieldBossSummon { MonsterNumber = BossNumber, Count = 2, X1 = 95, Y1 = 95, X2 = 105, Y2 = 105 }] });
        var state = plugIn.GetStates(gameContext).Single();
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        plugIn.AttackableGotHit(state.StageMonster!, player, default);
        await WaitUntilAsync(() => state.Status == FieldBossStatus.Engaged).ConfigureAwait(false);
        player.Position = state.StageMonster!.Position;
        var health = player.Attributes![Stats.CurrentHealth];

        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        Assert.That(player.Attributes[Stats.CurrentHealth], Is.LessThan(health), "The strike hit.");
        Assert.That(await CountAsync(gameContext, BossNumber).ConfigureAwait(false), Is.EqualTo(2));

        this._now += TimeSpan.FromSeconds(1);
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        Assert.That(await CountAsync(gameContext, BossNumber).ConfigureAwait(false), Is.EqualTo(2), "Not before its interval.");

        this._now += TimeSpan.FromSeconds(1);
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        Assert.That(await CountAsync(gameContext, BossNumber).ConfigureAwait(false), Is.EqualTo(3), "Up to its maximum.");
    }

    /// <summary>
    /// A boss with grounds spawns on one of them; its stages and summons move along.
    /// </summary>
    [Test]
    public async Task BossSpawnsOnItsGroundAsync()
    {
        var (plugIn, gameContext, _) = await this.CreateAsync().ConfigureAwait(false);
        plugIn.Configuration!.Bosses[0].Grounds.Add(new FieldBossGround { X = 150, Y = 160 });
        var state = plugIn.GetStates(gameContext).Single();

        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);

        Assert.That(state.StageMonster!.IsInRange(new Point(150, 160), 2), Is.True);
        var map = await gameContext.GetMapAsync(0).ConfigureAwait(false);
        Assert.That(map!.GetAttackablesInRange(new Point(150, 160), 7).OfType<Monster>().Count(m => m.Definition.Number == MinionNumber), Is.EqualTo(2), "The summons moved along.");
    }

    /// <summary>
    /// The entry item is sold for its price and taken one at a time.
    /// </summary>
    [Test]
    public async Task EntryItemIsSoldAndTakenAsync()
    {
        var (_, gameContext, player) = await this.CreateAsync().ConfigureAwait(false);
        var shard = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = 14, Number = 390, Width = 1, Height = 1, Durability = 5, Name = "Spirit Nixie's Shard" };
        gameContext.Configuration.Items.Add(shard);
        player.Money = 150;

        Assert.That(await FieldBossEntryItems.GiveAsync(player, shard, 100).ConfigureAwait(false), Is.True);
        Assert.That(player.Money, Is.EqualTo(50));
        Assert.That(await FieldBossEntryItems.GiveAsync(player, shard, 100).ConfigureAwait(false), Is.False, "Not enough zen.");
        Assert.That(FieldBossEntryItems.Has(player, shard), Is.True);
        Assert.That(await FieldBossEntryItems.TakeAsync(player, shard).ConfigureAwait(false), Is.True);
        Assert.That(await FieldBossEntryItems.TakeAsync(player, shard).ConfigureAwait(false), Is.False);
    }

    /// <summary>
    /// The state file keeps the timers over a restart: a killed boss waits for its respawn, a living one spawns again.
    /// </summary>
    [Test]
    public async Task RestartKeepsTheRespawnTimerAsync()
    {
        var (plugIn, gameContext, killer) = await this.CreateAsync().ConfigureAwait(false);
        var state = plugIn.GetStates(gameContext).Single();
        await plugIn.ExecuteTaskAsync(gameContext).ConfigureAwait(false);
        await state.StageMonster!.ReflectDamageAsync(killer, Health * 10).ConfigureAwait(false);
        await state.StageMonster!.ReflectDamageAsync(killer, Health * 10).ConfigureAwait(false);
        var respawnAt = state.RespawnAt;

        var restarted = new FieldBossesFeaturePlugIn { UtcNow = () => this._now, Configuration = plugIn.Configuration };
        var restored = restarted.GetStates(gameContext).Single();

        Assert.That(restored.Status, Is.EqualTo(FieldBossStatus.Waiting));
        Assert.That(restored.NextChangeAt, Is.EqualTo(respawnAt));
    }

    /// <summary>
    /// The game master command spawns a boss by its name.
    /// </summary>
    [Test]
    public async Task GameMasterCommandSpawnsTheBossAsync()
    {
        var (plugIn, gameContext, player) = await this.CreateAsync(spawnOnStart: false).ConfigureAwait(false);
        player.SelectedCharacter!.CharacterStatus = CharacterStatus.GameMaster;

        await new FieldBossChatCommandPlugIn().HandleCommandAsync(player, "/fieldboss testboss").ConfigureAwait(false);

        Assert.That(plugIn.GetStates(gameContext).Single().Status, Is.EqualTo(FieldBossStatus.Alive));
    }

    private static async ValueTask<int> CountAsync(IGameContext gameContext, short number)
    {
        var map = await gameContext.GetMapAsync(0).ConfigureAwait(false);
        return map!.GetAttackablesInRange(new Point(100, 100), 30).OfType<Monster>().Count(m => m.Definition.Number == number && m.IsAlive);
    }

    private static async Task WaitUntilAsync(Func<bool> condition) => await WaitUntilAsync(() => Task.FromResult(condition())).ConfigureAwait(false);

    private static async Task WaitUntilAsync(Func<Task<bool>> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!await condition().ConfigureAwait(false) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20).ConfigureAwait(false);
        }
    }

    private static MonsterDefinition CreateMonster(short number)
    {
        var definition = new MonsterDefinition { Id = Guid.NewGuid(), Number = number, ObjectKind = NpcObjectKind.Monster, Designation = $"Monster {number}" };
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumHealth, Value = Health });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MinimumPhysBaseDmg, Value = 10 });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumPhysBaseDmg, Value = 10 });
        return definition;
    }

    private async ValueTask<(FieldBossesFeaturePlugIn PlugIn, GameContext GameContext, Player Player)> CreateAsync(bool spawnOnStart = true)
    {
        this._now = new DateTime(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);
        var gameContext = (GameContext)GameContextTestHelper.CreateGameContext();
        foreach (var number in new[] { GuardNumber, MinionNumber, BossNumber })
        {
            gameContext.Configuration.Monsters.Add(CreateMonster(number));
        }

        var boss = new FieldBossDefinition
        {
            Name = "TestBoss",
            MapNumber = 0,
            BattleMinutes = 10,
            Arena = new FieldBossArena { X1 = 90, Y1 = 90, X2 = 110, Y2 = 110, EntryX = 95, EntryY = 95, MaximumPlayers = 1, ClosesWhenEngaged = true },
            Stages =
            [
                new FieldBossStage
                {
                    MonsterNumber = GuardNumber,
                    X = 100,
                    Y = 100,
                    Summons = [new FieldBossSummon { MonsterNumber = MinionNumber, Count = 2, X1 = 95, Y1 = 95, X2 = 105, Y2 = 105 }],
                    Phases = [new FieldBossHealthPhase { HealthPercent = 50, Summons = [new FieldBossSummon { MonsterNumber = MinionNumber, Count = 3, X1 = 95, Y1 = 95, X2 = 105, Y2 = 105 }] }],
                },
                new FieldBossStage { MonsterNumber = BossNumber, X = 100, Y = 100 },
            ],
        };

        // The container activates the instance of the plugin manager, which is the one the game calls.
        gameContext.FeaturePlugIns.AddPlugIn(new FieldBossesFeaturePlugIn(), true);
        var plugIn = FieldBossesFeaturePlugIn.Get(gameContext)!;
        plugIn.UtcNow = () => this._now;
        plugIn.Configuration = new FieldBossConfiguration { SpawnOnStart = spawnOnStart, Bosses = [boss], StateFile = Path.Combine(Path.GetTempPath(), $"openmu-bosses-{Guid.NewGuid():N}.json") };
        var player = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        player.Position = new Point(10, 10);
        return (plugIn, gameContext, player);
    }
}
