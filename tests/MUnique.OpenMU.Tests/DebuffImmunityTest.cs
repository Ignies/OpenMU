// <copyright file="DebuffImmunityTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MagicEffectDefinition = MUnique.OpenMU.Persistence.BasicModel.MagicEffectDefinition;
using MonsterAttribute = MUnique.OpenMU.Persistence.BasicModel.MonsterAttribute;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;

/// <summary>
/// Tests <see cref="Stats.IsImmuneToDebuffs"/>: a monster with it takes no magic effect of its attackers.
/// </summary>
[TestFixture]
public class DebuffImmunityTest
{
    private const short EffectNumber = 61;

    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up a fresh game context before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._gameContext = GameContextTestHelper.CreateGameContext();
    }

    /// <summary>
    /// An immune monster takes no effect.
    /// </summary>
    [Test]
    public async ValueTask ImmuneMonsterTakesNoEffectAsync()
    {
        var monster = await this.CreateMonsterAsync(true).ConfigureAwait(false);

        await monster.MagicEffectList.AddEffectAsync(CreateStun()).ConfigureAwait(false);

        Assert.That(monster.MagicEffectList.ContainsEffect(EffectNumber), Is.False);
        Assert.That(monster.Attributes[Stats.IsStunned], Is.Zero);
    }

    /// <summary>
    /// A monster without the attribute takes the effect, as before.
    /// </summary>
    [Test]
    public async ValueTask OtherMonsterTakesTheEffectAsync()
    {
        var monster = await this.CreateMonsterAsync(false).ConfigureAwait(false);

        await monster.MagicEffectList.AddEffectAsync(CreateStun()).ConfigureAwait(false);

        Assert.That(monster.MagicEffectList.ContainsEffect(EffectNumber), Is.True);
    }

    private static MagicEffect CreateStun()
    {
        var definition = new MagicEffectDefinition { Number = EffectNumber };
        return new MagicEffect(TimeSpan.FromSeconds(10), definition, new MagicEffect.ElementWithTarget(new ConstantElement(1), Stats.IsStunned));
    }

    private async ValueTask<Monster> CreateMonsterAsync(bool isImmune)
    {
        var definition = new MonsterDefinition { Id = Guid.NewGuid(), ObjectKind = NpcObjectKind.Monster };
        definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumHealth, Value = 1000 });
        if (isImmune)
        {
            definition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.IsImmuneToDebuffs, Value = 1 });
        }

        var map = await this._gameContext.GetMapAsync(0).ConfigureAwait(false);
        var spawnArea = new MonsterSpawnArea { MonsterDefinition = definition, GameMap = map!.Definition, X1 = 100, Y1 = 100, X2 = 100, Y2 = 100, Quantity = 1 };
        var monster = new Monster(spawnArea, definition, map, NullDropGenerator.Instance, new Mock<INpcIntelligence>().Object, this._gameContext.PlugInManager, this._gameContext.PathFinderPool);
        monster.Initialize();
        return monster;
    }
}
