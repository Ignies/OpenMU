// <copyright file="MonsterNormalizerTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests for <see cref="MonsterNormalizer"/>: the 139 monster rows of the progression design.
/// </summary>
[TestFixture]
public class MonsterNormalizerTest
{
    private static IEnumerable<TestCaseData> MonsterCases => ProgressionGoldenData.Monsters.Select(m => new TestCaseData(m).SetName($"Monster {m.Number} {m.Name}"));

    /// <summary>
    /// The design pins 23 sets and 139 monsters.
    /// </summary>
    [Test]
    public void TheTablesAreComplete()
    {
        Assert.That(ProgressionGoldenData.Sets, Has.Count.EqualTo(23));
        Assert.That(ProgressionGoldenData.Monsters, Has.Count.EqualTo(139));
        Assert.That(ProgressionGoldenData.Monsters.Select(m => m.Number).Distinct().Count(), Is.EqualTo(139));
    }

    /// <summary>
    /// Each monster row of the design, in the stars mode (which carries no over-cap scaling).
    /// </summary>
    /// <param name="monster">The monster row.</param>
    [TestCaseSource(nameof(MonsterCases))]
    public void MonsterRowMatchesTheDesign(GoldenMonster monster)
    {
        var actual = Normalize(monster.Set, new ProgressionConfiguration { Mode = ProgressionMode.Stars })[monster.Number];

        Assert.That(actual, Is.EqualTo(monster.Expected));
    }

    /// <summary>
    /// Every normalized monster stays below level 256, where the drop lists stop.
    /// </summary>
    [Test]
    public void EveryLevelStaysBelow256()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var levels = ProgressionGoldenData.Sets.SelectMany(s => Normalize(s.Id, configuration).Values).Select(r => r.Level).ToList();

        Assert.That(levels, Has.Count.EqualTo(139));
        Assert.That(levels, Has.All.LessThan(256));
    }

    /// <summary>
    /// The original mode returns the source rows unchanged.
    /// </summary>
    [Test]
    public void OriginalReturnsTheSourceRows()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original };
        foreach (var set in ProgressionGoldenData.Sets)
        {
            var actual = Normalize(set.Id, configuration);
            foreach (var monster in ProgressionGoldenData.Monsters.Where(m => m.Set == set.Id))
            {
                Assert.That(actual[monster.Number], Is.EqualTo(monster.Source));
            }
        }
    }

    /// <summary>
    /// Below power level 400 the normalized mode equals the stars mode; above it the over-cap strength scales health and
    /// damage, linearly to the setting at power level 600.
    /// </summary>
    [Test]
    public void OverCapStrengthScalesNormalizedSetsAbove400()
    {
        var stars = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var normalized = new ProgressionConfiguration { Mode = ProgressionMode.Normalized, OverCapStrengthPercent = 50 };

        Assert.That(Normalize("kubera", normalized), Is.EquivalentTo(Normalize("kubera", stars)));

        // Aquilas: T 586.0, so the factor is 1 - 0.5 * 186 / 200 = 0.535.
        var original = Normalize("aquilastemple", stars)[975];
        var scaled = Normalize("aquilastemple", normalized)[975];
        Assert.That(scaled.Health, Is.EqualTo(original.Health * 0.535).Within(original.Health * 0.01));
        Assert.That(scaled.DamageMax, Is.EqualTo(original.DamageMax * 0.535).Within(original.DamageMax * 0.01));
        Assert.That(scaled.Defense, Is.EqualTo(original.Defense));
        Assert.That(scaled.Level, Is.EqualTo(original.Level));
    }

    /// <summary>
    /// The percent knobs scale health, damage and the defensive numbers; the level stays.
    /// </summary>
    [Test]
    public void PercentKnobsScaleTheNumbers()
    {
        var plain = Normalize("kubera", new ProgressionConfiguration())[810];
        var scaled = Normalize("kubera", new ProgressionConfiguration { MonsterHealthPercent = 200, MonsterDamagePercent = 50, MonsterDefensePercent = 150 })[810];

        Assert.That(scaled.Level, Is.EqualTo(plain.Level));
        Assert.That(scaled.Health, Is.EqualTo(plain.Health * 2));
        Assert.That(scaled.DamageMin, Is.EqualTo(plain.DamageMin / 2.0).Within(1));
        Assert.That(scaled.Defense, Is.EqualTo(plain.Defense * 1.5).Within(1));
        Assert.That(scaled.AttackRate, Is.EqualTo(plain.AttackRate * 1.5).Within(1));
        Assert.That(scaled.DefenseRate, Is.EqualTo(plain.DefenseRate * 1.5).Within(1));
    }

    /// <summary>
    /// The boss multipliers are settings: Nix keeps 20 times its map's normal health by default.
    /// </summary>
    [Test]
    public void BossMultiplierIsASetting()
    {
        var plain = Normalize("nixie_lake", new ProgressionConfiguration())[746];
        var tenfold = Normalize("nixie_lake", new ProgressionConfiguration { BossHealthMultiplier = 10 })[746];

        Assert.That(tenfold.Health, Is.EqualTo(plain.Health / 2.0).Within(100));
    }

    /// <summary>
    /// The kind of a monster: bosses by number, elites by designation.
    /// </summary>
    [Test]
    public void Kinds()
    {
        Assert.That(MonsterNormalizer.GetKind(746, "Nix"), Is.EqualTo(MonsterKind.Boss));
        Assert.That(MonsterNormalizer.GetKind(848, "(Elite) Scorched Warrior"), Is.EqualTo(MonsterKind.Elite));
        Assert.That(MonsterNormalizer.GetKind(898, "Crimson Lycan (Elite)"), Is.EqualTo(MonsterKind.Elite));
        Assert.That(MonsterNormalizer.GetKind(769, "Deep Dungeon Elite Skeleton Warrior"), Is.EqualTo(MonsterKind.Elite));
        Assert.That(MonsterNormalizer.GetKind(810, "Mine Digger"), Is.EqualTo(MonsterKind.Normal));
    }

    private static Dictionary<short, MonsterStatRow> Normalize(string setId, ProgressionConfiguration configuration)
    {
        var set = ProgressionGoldenData.Sets.Single(s => s.Id == setId);
        var monsters = ProgressionGoldenData.Monsters
            .Where(m => m.Set == setId)
            .Select(m => new SetMonster(m.Number, m.Name, m.Source, m.Spawned > 0))
            .ToList();
        var rows = MonsterNormalizer.Normalize(set.Requirement, monsters, configuration);
        Assert.That(rows, Has.Count.EqualTo(monsters.Count));
        return monsters.Zip(rows).ToDictionary(p => p.First.Number, p => p.Second);
    }
}
