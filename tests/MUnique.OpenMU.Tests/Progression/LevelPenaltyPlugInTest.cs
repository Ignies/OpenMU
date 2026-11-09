// <copyright file="LevelPenaltyPlugInTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using Moq;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Progression;
using GameMapDefinition = MUnique.OpenMU.Persistence.BasicModel.GameMapDefinition;
using MonsterAttribute = MUnique.OpenMU.Persistence.BasicModel.MonsterAttribute;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;
using MonsterSpawnArea = MUnique.OpenMU.Persistence.BasicModel.MonsterSpawnArea;

/// <summary>
/// Tests the <see cref="LevelPenaltyPlugIn"/> in a game: a level 400 blade master against a level 150 monster (threshold
/// 750) of the Kardamahal temple.
/// </summary>
[TestFixture]
public class LevelPenaltyPlugInTest
{
    /// <summary>
    /// The character deals 70 % less damage to the monster and takes 70 % more from it; nothing while the plugin is off.
    /// </summary>
    [Test]
    public async ValueTask DamageChangesBothWaysAsync()
    {
        var (player, monster) = await CreatePlayerAndMonsterAsync().ConfigureAwait(false);

        Assert.That(LevelPenaltyPlugIn.ApplyToDamage(player, player, monster, 1000), Is.EqualTo(1000), "off");

        player.GameContext.FeaturePlugIns.AddPlugIn(new LevelPenaltyPlugIn { Configuration = new LevelPenaltyConfiguration() }, true);
        Assert.That(LevelPenaltyPlugIn.ApplyToDamage(player, player, monster, 1000), Is.EqualTo(300));
        Assert.That(LevelPenaltyPlugIn.ApplyToDamage(null, monster, player, 1000), Is.EqualTo(1700));
        Assert.That(LevelPenaltyPlugIn.ApplyToDamage(player, player, player, 1000), Is.EqualTo(1000), "players are no monsters");
    }

    /// <summary>
    /// The experience for a monster of the temple drops by the penalty; in the normalized mode a capped level 400 is
    /// above the mapped threshold, so it gets the full experience.
    /// </summary>
    [Test]
    public async ValueTask ExperienceFollowsTheProgressionModeAsync()
    {
        var (player, monster) = await CreatePlayerAndMonsterAsync().ConfigureAwait(false);
        var plugIn = new LevelPenaltyPlugIn { Configuration = new LevelPenaltyConfiguration() };
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);

        var args = new ExperienceCalculationArgs(monster, false, 1000);
        await plugIn.CalculateExperienceAsync(player, args).ConfigureAwait(false);
        Assert.That(args.Experience, Is.EqualTo(300).Within(1e-6));

        player.GameContext.FeaturePlugIns.AddPlugIn(new ProgressionFeaturePlugIn { Configuration = new ProgressionConfiguration { Mode = ProgressionMode.Normalized } }, true);
        args = new ExperienceCalculationArgs(monster, true, 1000);
        await plugIn.CalculateExperienceAsync(player, args).ConfigureAwait(false);
        Assert.That(args.Experience, Is.EqualTo(1000).Within(1e-6));
        Assert.That(LevelPenaltyPlugIn.ApplyToDamage(player, player, monster, 1000), Is.EqualTo(1000));
    }

    /// <summary>
    /// A monster without a level (a castle gate, a guardian statue, a life stone) takes the damage unchanged and gives
    /// the experience unchanged: the catapult impact on the gate failed on the missing level.
    /// </summary>
    [Test]
    public async ValueTask MonsterWithoutLevelKeepsTheDamageAsync()
    {
        var (player, monster) = await CreatePlayerAndMonsterAsync().ConfigureAwait(false);
        var plugIn = new LevelPenaltyPlugIn { Configuration = new LevelPenaltyConfiguration() };
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        monster.Definition.Attributes.Remove(monster.Definition.Attributes.First(a => a.AttributeDefinition == Stats.Level));

        Assert.That(LevelPenaltyPlugIn.ApplyToDamage(player, player, monster, 1000), Is.EqualTo(1000));
        var args = new ExperienceCalculationArgs(monster, false, 1000);
        await plugIn.CalculateExperienceAsync(player, args).ConfigureAwait(false);
        Assert.That(args.Experience, Is.EqualTo(1000).Within(1e-6));
    }

    /// <summary>
    /// The plugin is off by default and the fork marker names it.
    /// </summary>
    [Test]
    public void IsOffByDefaultAndInTheForkMarker()
    {
        Assert.That(new LevelPenaltyPlugIn(), Is.InstanceOf<IDisabledByDefault>());
        Assert.That(ForkInfo.Has(LevelPenaltyPlugIn.FeatureId), Is.True);
        Assert.That(ForkInfo.Has("creatable_classes"), Is.True);
    }

    private static async ValueTask<(Player Player, Monster Monster)> CreatePlayerAndMonsterAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.SelectedCharacter!.CharacterClass!.Number = 128 + 11;
        player.Attributes![Stats.Level] = 400;

        var definition = new MonsterDefinition { Id = Guid.NewGuid(), Number = 900, Designation = "Temple monster", ObjectKind = NpcObjectKind.Monster };
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.Level, Value = 150 });
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumHealth, Value = 1000 });
        var mapDefinition = new GameMapDefinition { Id = Guid.NewGuid(), Number = 143, IsActive = true, TerrainData = new byte[ushort.MaxValue + 3] };
        var spawn = new MonsterSpawnArea { MonsterDefinition = definition, GameMap = mapDefinition, X1 = 10, X2 = 20, Y1 = 10, Y2 = 20, Quantity = 1 };
        mapDefinition.MonsterSpawns.Add(spawn);
        var map = new GameMap(mapDefinition, TimeSpan.FromSeconds(60), 8);
        var monster = new Monster(spawn, definition, map, NullDropGenerator.Instance, new Mock<INpcIntelligence>().Object, player.GameContext.PlugInManager, player.GameContext.PathFinderPool);
        monster.Initialize();
        return (player, monster);
    }
}
