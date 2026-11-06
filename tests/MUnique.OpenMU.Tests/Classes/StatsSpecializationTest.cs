// <copyright file="StatsSpecializationTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Classes;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.Classes;

/// <summary>
/// Tests the stat specialization of Season 21 (<see cref="StatsSpecializationPlugIn"/>).
/// </summary>
[TestFixture]
public class StatsSpecializationTest
{
    /// <summary>
    /// The default configuration holds the S21Emu table (<c>Character/StatsSpecialize.txt</c>): 39 rows over the 16
    /// classes, three sections each.
    /// </summary>
    [Test]
    public void DefaultConfigurationIsTheSourceTable()
    {
        var configuration = StatsSpecializationConfiguration.CreateDefault();

        Assert.That(configuration.Specializations, Has.Count.EqualTo(39));
        Assert.That(configuration.Specializations.Select(s => s.BaseClass).Distinct().Count(), Is.EqualTo(16));
        Assert.That(configuration.Specializations.Select(s => s.Sections.Count), Has.All.EqualTo(3));
        var fairyElf = configuration.Specializations.First(s => s.BaseClass == BaseClassCode.FairyElf);
        Assert.That(fairyElf.Stat, Is.EqualTo(StatsSpecializationStat.Agility));
        Assert.That(fairyElf.Option, Is.EqualTo(StatsSpecializationOption.AttackPower));
        Assert.That(fairyElf.Sections.Select(s => (s.UpToStat, s.Percent)), Is.EqualTo(new[] { (1800, 10f), (2100, 30f), (65535, 45f) }));
        var crusader = configuration.Specializations.Where(s => s.BaseClass == BaseClassCode.Crusader).Select(s => (s.Stat, s.Option));
        Assert.That(crusader, Is.EqualTo(new[]
        {
            (StatsSpecializationStat.Strength, StatsSpecializationOption.AttackPower),
            (StatsSpecializationStat.Agility, StatsSpecializationOption.AttackRate),
            (StatsSpecializationStat.Agility, StatsSpecializationOption.Defense),
        }));
    }

    /// <summary>
    /// A specialization raises its targets by the percent of the section its stat is in, and follows the stat.
    /// </summary>
    [Test]
    public void PercentFollowsTheSectionOfTheStat()
    {
        var attributes = CreateAttributes(strength: 1000);
        var configuration = StatsSpecializationConfiguration.CreateDefault();

        var elements = StatsSpecializationPlugIn.Apply(attributes, BaseClassCode.DarkKnight, configuration);

        Assert.That(elements, Is.Not.Empty);
        Assert.That(attributes[Stats.MinimumPhysBaseDmg], Is.EqualTo(100 * 1.1f).Within(0.01));
        Assert.That(attributes[Stats.MaximumPhysBaseDmg], Is.EqualTo(200 * 1.1f).Within(0.01));
        attributes[Stats.BaseStrength] = 1800;
        Assert.That(attributes[Stats.MinimumPhysBaseDmg], Is.EqualTo(180 * 1.3f).Within(0.01));
        attributes[Stats.BaseStrength] = 2500;
        Assert.That(attributes[Stats.MinimumPhysBaseDmg], Is.EqualTo(250 * 1.45f).Within(0.01));
        Assert.That(attributes[Stats.DefenseBase], Is.EqualTo(50 * 1.1f).Within(0.01));
    }

    /// <summary>
    /// A class gets only its own rows: the dark wizard raises wizardry damage and attack rates, not physical damage.
    /// </summary>
    [Test]
    public void ClassGetsOnlyItsRows()
    {
        var attributes = CreateAttributes(strength: 1000);

        StatsSpecializationPlugIn.Apply(attributes, BaseClassCode.DarkWizard, StatsSpecializationConfiguration.CreateDefault());

        Assert.That(attributes[Stats.MinimumPhysBaseDmg], Is.EqualTo(100).Within(0.01));
        Assert.That(attributes[Stats.MinimumWizBaseDmg], Is.EqualTo(80 * 1.1f).Within(0.01));
        Assert.That(attributes[Stats.AttackRatePvm], Is.EqualTo(300 * 1.1f).Within(0.01));
        Assert.That(attributes[Stats.AttackRatePvp], Is.EqualTo(400 * 1.1f).Within(0.01));
    }

    /// <summary>
    /// The operator's sections replace the source: a first section of 0 % up to 1500 gives nothing below it.
    /// </summary>
    [Test]
    public void OperatorSectionsApply()
    {
        var attributes = CreateAttributes(strength: 1000);
        var configuration = new StatsSpecializationConfiguration
        {
            Specializations =
            [
                new StatsSpecialization
                {
                    BaseClass = BaseClassCode.DarkKnight,
                    Stat = StatsSpecializationStat.Strength,
                    Option = StatsSpecializationOption.AttackPower,
                    Sections = [new() { UpToStat = 1500, Percent = 0 }, new() { UpToStat = 65535, Percent = 20 }],
                },
            ],
        };

        StatsSpecializationPlugIn.Apply(attributes, BaseClassCode.DarkKnight, configuration);

        Assert.That(attributes[Stats.MinimumPhysBaseDmg], Is.EqualTo(100).Within(0.01));
        attributes[Stats.BaseStrength] = 2000;
        Assert.That(attributes[Stats.MinimumPhysBaseDmg], Is.EqualTo(200 * 1.2f).Within(0.01));
    }

    /// <summary>
    /// The fork marker lists the feature, so the Hidden glue can offer its switch.
    /// </summary>
    [Test]
    public void ForkMarkerListsTheFeature()
    {
        Assert.That(GameLogic.ForkInfo.Has(StatsSpecializationPlugIn.FeatureId), Is.True);
    }

    private static AttributeSystem CreateAttributes(float strength)
    {
        var stats = new IAttribute[]
        {
            new StatAttribute(Stats.BaseStrength, strength),
            new StatAttribute(Stats.BaseAgility, 500),
            new StatAttribute(Stats.BaseEnergy, 800),
        };
        var relationships = new[]
        {
            new AttributeRelationship(Stats.TotalStrength, 1, Stats.BaseStrength),
            new AttributeRelationship(Stats.TotalAgility, 1, Stats.BaseAgility),
            new AttributeRelationship(Stats.TotalEnergy, 1, Stats.BaseEnergy),
            new AttributeRelationship(Stats.MinimumPhysBaseDmg, 0.1f, Stats.TotalStrength),
            new AttributeRelationship(Stats.MaximumPhysBaseDmg, 0.2f, Stats.TotalStrength),
            new AttributeRelationship(Stats.DefenseBase, 0.1f, Stats.TotalAgility),
            new AttributeRelationship(Stats.AttackRatePvm, 0.6f, Stats.TotalAgility),
            new AttributeRelationship(Stats.AttackRatePvp, 0.8f, Stats.TotalAgility),
            new AttributeRelationship(Stats.MinimumWizBaseDmg, 0.1f, Stats.TotalEnergy),
        };
        return new AttributeSystem(stats, [], relationships);
    }
}
