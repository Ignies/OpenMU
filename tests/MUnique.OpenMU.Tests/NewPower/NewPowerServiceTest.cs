// <copyright file="NewPowerServiceTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.NewPower;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.NewPower;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// The rules of the fifth class "New Power" (Nuke, Bolt, Wide): when it triggers, how much it deals and whom it hits.
/// </summary>
[TestFixture]
public class NewPowerServiceTest
{
    private static readonly NewPowerConfiguration Configuration = new();

    /// <summary>
    /// The extra damage card value takes the kind's rate (Nuke 100 %, Bolt 2/3, Wide 1/3), the kind's ATK increase adds
    /// to it as it is.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="expected">The expected damage of 900 extra damage and 100 ATK increase.</param>
    [TestCase(NewPowerKind.Nuke, 1000u)]
    [TestCase(NewPowerKind.Bolt, 700u)]
    [TestCase(NewPowerKind.Wide, 400u)]
    public void DamageIsTheRatedExtraDamagePlusTheAttackIncrease(NewPowerKind kind, uint expected)
    {
        var stats = NewPowerStats.Of(kind);
        var attributes = Attributes(new() { [stats.ExtraDamage] = 900, [stats.AttackIncrease] = 100 });

        var (damage, flags) = NewPowerService.RollDamage(attributes, Configuration.Get(kind)!, _ => false);

        Assert.That((damage, flags), Is.EqualTo((expected, DamageAttributes.Undefined)));
    }

    /// <summary>
    /// The card rates of the kind make a hit excellent (x1.2 plus the excellent damage bonus) or critical (plus the
    /// critical damage bonus), and triple or double it; triple rolls first, as option 60 does.
    /// </summary>
    [Test]
    public void RatesOfTheKindMultiplyTheHit()
    {
        var stats = NewPowerStats.Of(NewPowerKind.Nuke);
        var attributes = Attributes(new()
        {
            [stats.ExtraDamage] = 1000,
            [stats.CriticalChance] = 0.5f,
            [stats.ExcellentChance] = 0.25f,
            [stats.DoubleChance] = 0.125f,
            [stats.TripleChance] = 0.0625f,
            [Stats.CriticalDamageBonus] = 50,
            [Stats.ExcellentDamageBonus] = 20,
        });
        var nuke = Configuration.Get(NewPowerKind.Nuke)!;

        Assert.That(NewPowerService.RollDamage(attributes, nuke, c => c is 0.5), Is.EqualTo((1050u, DamageAttributes.Critical)));
        Assert.That(NewPowerService.RollDamage(attributes, nuke, c => c is 0.25 or 0.5), Is.EqualTo((1220u, DamageAttributes.Excellent)));
        Assert.That(NewPowerService.RollDamage(attributes, nuke, c => c is 0.0625 or 0.125), Is.EqualTo((3000u, DamageAttributes.Triple)));
        Assert.That(NewPowerService.RollDamage(attributes, nuke, c => c is 0.125), Is.EqualTo((2000u, DamageAttributes.Double)));
    }

    /// <summary>
    /// Without extra damage nothing is dealt.
    /// </summary>
    [Test]
    public void NoDamageWithoutValues()
    {
        Assert.That(NewPowerService.RollDamage(Attributes(new()), Configuration.Get(NewPowerKind.Wide)!, _ => true).Damage, Is.Zero);
    }

    /// <summary>
    /// Nuke hits the target only, Bolt it and the nearest other within 2 tiles, Wide it and up to 9 others within 3 tiles
    /// (Webzen guide 340: at most 1, 2 and 10 targets); dead ones don't count.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="expected">The targets hit, by their x coordinate.</param>
    [TestCase(NewPowerKind.Nuke, new[] { 10 })]
    [TestCase(NewPowerKind.Bolt, new[] { 10, 11 })]
    [TestCase(NewPowerKind.Wide, new[] { 10, 11, 12, 13 })]
    public void TargetsFollowTheKind(NewPowerKind kind, int[] expected)
    {
        var first = Target(10, true);
        var around = new[] { Target(13, true), Target(11, true), Target(12, true), Target(14, true), Target(11, false), first };

        var targets = NewPowerService.SelectTargets(first, around, Configuration.Get(kind)!);

        Assert.That(targets.Select(t => (int)t.Position.X), Is.EqualTo(expected));
    }

    /// <summary>
    /// Wide stops at 10 targets.
    /// </summary>
    [Test]
    public void WideHitsAtMostTenTargets()
    {
        var first = Target(20, true);
        var around = Enumerable.Range(18, 5).Select(x => Target((byte)x, true)).ToList();
        around.AddRange(Enumerable.Range(18, 5).Select(x => Target((byte)x, true, 21)));

        Assert.That(NewPowerService.SelectTargets(first, around, Configuration.Get(NewPowerKind.Wide)!), Has.Count.EqualTo(10));
    }

    /// <summary>
    /// The power of the extra damage card in the core (its skill) or of a trial's effect is the one which works; the card
    /// comes first. Without either there is none.
    /// </summary>
    [Test]
    public void ActiveKindComesFromTheCardSkillOrTheTrialEffect()
    {
        Assert.That(NewPowerService.GetActiveKind(Configuration, s => s == 2087, _ => false)?.Kind, Is.EqualTo(NewPowerKind.Bolt));
        Assert.That(NewPowerService.GetActiveKind(Configuration, _ => false, e => e == 423)?.Kind, Is.EqualTo(NewPowerKind.Wide));
        Assert.That(NewPowerService.GetActiveKind(Configuration, s => s == 2086, e => e == 423)?.Kind, Is.EqualTo(NewPowerKind.Nuke));
        Assert.That(NewPowerService.GetActiveKind(Configuration, _ => false, _ => false), Is.Null);
    }

    /// <summary>
    /// It triggers on the skills of the skill enhancement tree (the fourth class skills) and the card skills (Webzen guide
    /// 340), not on other skills.
    /// </summary>
    [Test]
    public void TriggersOnTreeAndCardSkills()
    {
        var tree = new EnhanceTreeConfiguration { Nodes = [new EnhanceTreeNode { Skill = 1076 }, new EnhanceTreeNode { Skill = 1077 }] };

        Assert.That(NewPowerService.IsTriggerSkill(tree, 1076, false), Is.True);
        Assert.That(NewPowerService.IsTriggerSkill(tree, 2037, true), Is.True);
        Assert.That(NewPowerService.IsTriggerSkill(tree, 9, false), Is.False);
        Assert.That(NewPowerService.IsTriggerSkill(null, 1076, false), Is.False);
    }

    /// <summary>
    /// One strike per cast: a second hit within the cooldown (the hits of one area cast) doesn't trigger again.
    /// </summary>
    [Test]
    public void TriggersOncePerCooldown()
    {
        var state = new NewPowerService.TriggerState();
        var now = new DateTime(2026, 11, 19, 0, 0, 0, DateTimeKind.Utc);
        var cooldown = TimeSpan.FromMilliseconds(Configuration.CooldownMilliseconds);

        Assert.That(state.TryTrigger(now, cooldown), Is.True);
        Assert.That(state.TryTrigger(now.AddMilliseconds(100), cooldown), Is.False);
        Assert.That(state.TryTrigger(now + cooldown, cooldown), Is.True);
    }

    private static IAttackable Target(byte x, bool alive, byte y = 20)
    {
        var target = new Mock<IAttackable>();
        target.Setup(t => t.Position).Returns(new Point(x, y));
        target.Setup(t => t.IsAlive).Returns(alive);
        return target.Object;
    }

    private static IAttributeSystem Attributes(Dictionary<AttributeDefinition, float> values)
    {
        var attributes = new Mock<IAttributeSystem>();
        attributes.Setup(a => a[It.IsAny<AttributeDefinition>()]).Returns((AttributeDefinition d) => values.GetValueOrDefault(d));
        return attributes.Object;
    }
}
