// <copyright file="ImpenetrableDefenseTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Tests the impenetrable defense of the Season 21 enhance tree: a hit which ignores the defense still meets it
/// (Webzen guide 42: it "cannot be penetrated with options such as Enemy DEF Ignore, to the extent of that option's value").
/// </summary>
[TestFixture]
public class ImpenetrableDefenseTest
{
    /// <summary>
    /// A hit ignoring the defense meets the impenetrable part of it, never more than the defense itself.
    /// </summary>
    [Test]
    public async ValueTask DefenseIgnoreMeetsTheImpenetrableDefenseAsync()
    {
        var ignoring = CreateAttacker(defenseIgnoreChance: 1);
        var withoutImpenetrable = await ignoring.CalculateDamageAsync(CreateDefender(400, 0), null, false).ConfigureAwait(false);
        var withImpenetrable = await ignoring.CalculateDamageAsync(CreateDefender(400, 150), null, false).ConfigureAwait(false);
        var aboveTheDefense = await ignoring.CalculateDamageAsync(CreateDefender(100, 150), null, false).ConfigureAwait(false);
        var regular = await CreateAttacker(defenseIgnoreChance: 0).CalculateDamageAsync(CreateDefender(400, 150), null, false).ConfigureAwait(false);

        Assert.That(withoutImpenetrable.HealthDamage - withImpenetrable.HealthDamage, Is.EqualTo(150));
        Assert.That(withoutImpenetrable.HealthDamage - aboveTheDefense.HealthDamage, Is.EqualTo(100), "at most the defense");
        Assert.That(withoutImpenetrable.HealthDamage - regular.HealthDamage, Is.EqualTo(400), "a regular hit meets the whole defense");
    }

    private static IAttacker CreateAttacker(float defenseIgnoreChance)
    {
        var attacker = new Mock<IAttacker>();
        attacker.Setup(a => a.Attributes).Returns(CreateAttributes(new()
        {
            [Stats.AttackRatePvm] = 1_000_000,
            [Stats.MinimumPhysBaseDmg] = 1000,
            [Stats.MaximumPhysBaseDmg] = 1000,
            [Stats.Level] = 100,
            [Stats.AttackDamageIncrease] = 1,
            [Stats.DefenseIgnoreChance] = defenseIgnoreChance,
        }));
        return attacker.Object;
    }

    private static IAttackable CreateDefender(float defense, float impenetrable)
    {
        var defender = new Mock<IAttackable>();
        defender.Setup(d => d.Attributes).Returns(CreateAttributes(new()
        {
            [Stats.DefensePvm] = defense,
            [Stats.DefenseDecrement] = 1,
            [Stats.DamageReceiveDecrement] = 1,
            [Stats.ImpenetrableDefense] = impenetrable,
        }));
        return defender.Object;
    }

    private static IAttributeSystem CreateAttributes(Dictionary<AttributeDefinition, float> values)
    {
        var attributes = new Mock<IAttributeSystem>();
        attributes.Setup(a => a[It.IsAny<AttributeDefinition>()]).Returns<AttributeDefinition>(d => values.GetValueOrDefault(d));
        return attributes.Object;
    }
}
