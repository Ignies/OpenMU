// <copyright file="AbilityOptionEffectsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.AbilityCore;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// The combat effects of the Ability Enhancement Core's card options which OpenMU had no attribute for.
/// </summary>
[TestFixture]
public class AbilityOptionEffectsTest
{
    /// <summary>
    /// The "Mastery DMG / Wizardry / Curse Increase" options (25, 26, 27) add to the damage of a master skill of their
    /// damage type, and not to a skill without a master definition or of another type.
    /// </summary>
    /// <param name="damageType">The damage type of the skill.</param>
    /// <param name="attribute">The name of the attribute of the option.</param>
    [TestCase(DamageType.Physical, "MasterySkillDamageBonus")]
    [TestCase(DamageType.Wizardry, "MasterySkillWizardryBonus")]
    [TestCase(DamageType.Curse, "MasterySkillCurseBonus")]
    public async ValueTask MasteryOptionAddsToMasterSkillsOfItsTypeAsync(DamageType damageType, string attribute)
    {
        var bonus = typeof(Stats).GetProperty(attribute)?.GetValue(null) as AttributeDefinition;
        Assert.That(bonus, Is.Not.Null, attribute);

        var master = await DamageAsync(damageType, true, bonus!).ConfigureAwait(false);
        var plain = await DamageAsync(damageType, false, bonus!).ConfigureAwait(false);
        var withoutBonus = await DamageAsync(damageType, true, null).ConfigureAwait(false);

        Assert.That(master - withoutBonus, Is.EqualTo(300), $"master {master}, without {withoutBonus}, plain {plain}");
        Assert.That(plain, Is.EqualTo(withoutBonus));
    }

    private static async ValueTask<uint> DamageAsync(DamageType damageType, bool isMasterSkill, AttributeDefinition? bonus)
    {
        var values = new Dictionary<AttributeDefinition, float>
        {
            [Stats.AttackRatePvm] = 1_000_000,
            [Stats.MinimumPhysBaseDmg] = 1000,
            [Stats.MaximumPhysBaseDmg] = 1000,
            [Stats.MinimumWizBaseDmg] = 1000,
            [Stats.MaximumWizBaseDmg] = 1000,
            [Stats.MinimumCurseBaseDmg] = 1000,
            [Stats.MaximumCurseBaseDmg] = 1000,
            [Stats.WizardryAttackDamageIncrease] = 1,
            [Stats.CurseAttackDamageIncrease] = 1,
            [Stats.Level] = 100,
            [Stats.AttackDamageIncrease] = 1,
            [Stats.SkillMultiplier] = 1,
        };
        if (bonus is not null)
        {
            values[bonus] = 300;
        }

        var attacker = new Mock<IAttacker>();
        attacker.Setup(a => a.Attributes).Returns(Attributes(values));
        var defender = new Mock<IAttackable>();
        defender.Setup(d => d.Attributes).Returns(Attributes(new()
        {
            [Stats.DefenseDecrement] = 1,
            [Stats.DamageReceiveDecrement] = 1,
        }));

        var skillMock = new Mock<Skill> { CallBase = true };
        skillMock.Setup(s => s.AttributeRelationships).Returns(new List<AttributeRelationship>());
        var skill = skillMock.Object;
        skill.Number = 2091;
        skill.DamageType = damageType;
        if (isMasterSkill)
        {
            skill.MasterDefinition = new MasterSkillDefinition();
        }

        var hit = await attacker.Object.CalculateDamageAsync(defender.Object, new SkillEntry { Skill = skill }, false).ConfigureAwait(false);
        return hit.HealthDamage;
    }

    private static IAttributeSystem Attributes(Dictionary<AttributeDefinition, float> values)
    {
        var attributes = new Mock<IAttributeSystem>();
        attributes.Setup(a => a[It.IsAny<AttributeDefinition>()]).Returns<AttributeDefinition>(d => values.GetValueOrDefault(d));
        return attributes.Object;
    }
}
