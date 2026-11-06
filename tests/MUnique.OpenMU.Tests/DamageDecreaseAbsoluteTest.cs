// <copyright file="DamageDecreaseAbsoluteTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Tests the flat damage decrease of the later seasons ("Decrease DMG +45" of the mastery armors).
/// </summary>
[TestFixture]
public class DamageDecreaseAbsoluteTest
{
    /// <summary>
    /// A flat damage decrease takes its value off every hit, down to the minimum damage of the attacker's level.
    /// </summary>
    [Test]
    public async ValueTask FlatDecreaseIsSubtractedAsync()
    {
        var attacker = CreateAttacker();
        var withoutDecrease = await attacker.CalculateDamageAsync(CreateDefender(0), null, false).ConfigureAwait(false);
        var withDecrease = await attacker.CalculateDamageAsync(CreateDefender(45), null, false).ConfigureAwait(false);
        var aboveTheDamage = await attacker.CalculateDamageAsync(CreateDefender(5000), null, false).ConfigureAwait(false);

        Assert.That(withoutDecrease.HealthDamage - withDecrease.HealthDamage, Is.EqualTo(45));
        Assert.That(aboveTheDamage.HealthDamage, Is.EqualTo(10), "level 100 / 10");
    }

    private static IAttacker CreateAttacker()
    {
        var attacker = new Mock<IAttacker>();
        attacker.Setup(a => a.Attributes).Returns(CreateAttributes(new()
        {
            [Stats.AttackRatePvm] = 1_000_000,
            [Stats.MinimumPhysBaseDmg] = 1000,
            [Stats.MaximumPhysBaseDmg] = 1000,
            [Stats.Level] = 100,
            [Stats.AttackDamageIncrease] = 1,
        }));
        return attacker.Object;
    }

    private static IAttackable CreateDefender(float decrease)
    {
        var defender = new Mock<IAttackable>();
        defender.Setup(d => d.Attributes).Returns(CreateAttributes(new()
        {
            [Stats.DefenseDecrement] = 1,
            [Stats.DamageReceiveDecrement] = 1,
            [Stats.DamageDecreaseAbsolute] = decrease,
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
