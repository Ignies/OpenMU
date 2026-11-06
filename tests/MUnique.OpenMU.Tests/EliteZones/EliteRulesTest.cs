// <copyright file="EliteRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.EliteZones;

using System.ComponentModel;
using System.IO;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.EliteZones;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.Pathfinding;
using MonsterAttribute = MUnique.OpenMU.Persistence.BasicModel.MonsterAttribute;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests the debuffs of the elites and the weekly drop limits of the <see cref="EliteZonesFeaturePlugIn"/>.
/// </summary>
[TestFixture]
public class EliteRulesTest
{
    private const short InnovationNumber = 0x4D;

    private DateTime _now = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// An elite of a fire zone puts the debuffs of fire on the character it hits; a normal monster doesn't.
    /// </summary>
    [Test]
    public async Task EliteHitPutsTheDebuffsOfItsZoneAsync()
    {
        var gameContext = (GameContext)GameContextTestHelper.CreateGameContext();
        gameContext.Configuration.MagicEffects.Add(new MUnique.OpenMU.Persistence.BasicModel.MagicEffectDefinition { Number = InnovationNumber, Duration = new MUnique.OpenMU.Persistence.BasicModel.PowerUpDefinitionValue() });
        var player = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        var plugIn = this.CreatePlugIn(new EliteDebuff { Element = EliteZoneElement.Fire, Kind = EliteDebuffKind.DecreaseDefense, Value = 20, Seconds = 15 });
        var normal = await CreateMonsterAsync(gameContext, "Twisted Crypta").ConfigureAwait(false);
        var elite = await CreateMonsterAsync(gameContext, "Twisted Crypta (Elite)").ConfigureAwait(false);

        await plugIn.ApplyDebuffsAsync(player, normal, new HitInfo(100, 0, default)).ConfigureAwait(false);
        Assert.That(player.Attributes![Stats.InnovationDefDecrement], Is.Zero);

        await plugIn.ApplyDebuffsAsync(player, elite, new HitInfo(100, 0, default)).ConfigureAwait(false);
        Assert.That(player.MagicEffectList.GetActiveEffectsSnapshot().Select(e => e.Definition.Number), Does.Contain(InnovationNumber));
        Assert.That(player.Attributes[Stats.InnovationDefDecrement], Is.EqualTo(0.2f).Within(0.001));
    }

    /// <summary>
    /// A limited item drops for a character up to its weekly maximum, and again after the week's start day.
    /// </summary>
    [Test]
    public async Task DropLimitHoldsPerCharacterAndWeekAsync()
    {
        var gameContext = (GameContext)GameContextTestHelper.CreateGameContext();
        var player = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        var other = await PlayerTestHelper.CreatePlayerAsync(gameContext).ConfigureAwait(false);
        player.SelectedCharacter!.Id = Guid.NewGuid();
        other.SelectedCharacter!.Id = Guid.NewGuid();
        var plugIn = this.CreatePlugIn();
        plugIn.Configuration!.DropLimits.Add(new EliteDropLimit { ItemGroup = 14, ItemNumber = 42, MaximumPerWeek = 2 });
        var monster = await CreateMonsterAsync(gameContext, "Twisted Crypta (Elite)").ConfigureAwait(false);
        var harmony = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = 14, Number = 42 };
        var item = new MUnique.OpenMU.Persistence.BasicModel.Item { Definition = harmony };

        bool Drops(Player killer)
        {
            var args = new CancelEventArgs();
            plugIn.ItemDropping(monster, killer, item, args);
            return !args.Cancel;
        }

        Assert.That(Drops(player), Is.True);
        Assert.That(Drops(player), Is.True);
        Assert.That(Drops(player), Is.False, "The weekly maximum is reached.");
        Assert.That(Drops(other), Is.True, "Each character has its own limit.");

        this._now = new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc);
        Assert.That(Drops(player), Is.True, "Tuesday starts a new week.");
    }

    private static async ValueTask<Monster> CreateMonsterAsync(IGameContext gameContext, string name)
    {
        var definition = new MonsterDefinition { Id = Guid.NewGuid(), Number = 958, Designation = name, ObjectKind = NpcObjectKind.Monster };
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumHealth, Value = 1000 });
        var map = await gameContext.GetMapAsync(0).ConfigureAwait(false);
        var area = new MonsterSpawnArea { MonsterDefinition = definition, GameMap = map!.Definition, SpawnTrigger = SpawnTrigger.OnceAtEventStart, X1 = 15, Y1 = 15, X2 = 15, Y2 = 15, Quantity = 1 };
        var monster = new Monster(area, definition, map, NullDropGenerator.Instance, new BasicMonsterIntelligence(), gameContext.PlugInManager, gameContext.PathFinderPool);
        monster.Initialize();
        await map.AddAsync(monster).ConfigureAwait(false);
        return monster;
    }

    private EliteZonesFeaturePlugIn CreatePlugIn(params EliteDebuff[] debuffs) => new()
    {
        UtcNow = () => this._now,
        Configuration = new EliteZoneConfiguration
        {
            Zones = [new EliteZoneDefinition { MapNumber = 0, Element = EliteZoneElement.Fire, X1 = 10, Y1 = 10, X2 = 20, Y2 = 20 }],
            Debuffs = [.. debuffs],
            StateFile = Path.Combine(Path.GetTempPath(), $"openmu-elite-{Guid.NewGuid():N}.json"),
        },
    };
}
