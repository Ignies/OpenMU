// <copyright file="ProgressionMappingTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests for <see cref="ProgressionMapping"/>: the owner anchors and the 23 set rows of the progression design.
/// </summary>
[TestFixture]
public class ProgressionMappingTest
{
    private static IEnumerable<TestCaseData> SetCases => ProgressionGoldenData.Sets.Select(s => new TestCaseData(s).SetName($"Set {s.Id} ({s.Requirement})"));

    /// <summary>
    /// The owner anchors: 400 maps to 190, 1000 maps to level 400 in normalized and to level 200 with 2 stars in stars.
    /// </summary>
    [Test]
    public void OwnerAnchorsHold()
    {
        var normalized = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        var stars = new ProgressionConfiguration { Mode = ProgressionMode.Stars };

        Assert.That(ProgressionMapping.GetPowerLevel(400, normalized), Is.EqualTo(190).Within(1e-9));
        Assert.That(ProgressionMapping.GetPowerLevel(1000, normalized), Is.EqualTo(400).Within(1e-9));
        Assert.That(ProgressionMapping.GetRequirement(1000, normalized), Is.EqualTo(new MapRequirement(400, 0, 0)));
        Assert.That(ProgressionMapping.GetRequirement(400, normalized), Is.EqualTo(new MapRequirement(190, 0, 0)));
        Assert.That(ProgressionMapping.GetRequirement(1000, stars), Is.EqualTo(new MapRequirement(200, 0, 2)));
        Assert.That(ProgressionMapping.GetRequirement(400, stars), Is.EqualTo(new MapRequirement(190, 0, 0)));
    }

    /// <summary>
    /// Each set row of the design: T(R), the normalized and the stars requirement, the warp zen and the anchor level.
    /// </summary>
    /// <param name="set">The set row.</param>
    [TestCaseSource(nameof(SetCases))]
    public void SetRowMatchesTheDesign(GoldenSet set)
    {
        var normalized = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        var stars = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var powerLevel = ProgressionMapping.GetPowerLevel(set.Requirement, normalized);

        Assert.That(Math.Round(powerLevel, 1), Is.EqualTo(set.PowerLevel));
        Assert.That(ProgressionMapping.GetRequirement(set.Requirement, normalized), Is.EqualTo(new MapRequirement(set.Level, set.MasterLevel, 0)));
        Assert.That(ProgressionMapping.GetRequirement(set.Requirement, stars), Is.EqualTo(new MapRequirement(set.StarLevel, 0, set.Stars)));
        Assert.That(ProgressionMapping.GetEntryZen(set.Requirement, 50_000, normalized), Is.EqualTo(set.Zen));
        Assert.That(ProgressionMapping.GetEntryZen(set.Requirement, 50_000, stars), Is.EqualTo(set.Zen));
        Assert.That(Math.Round(ProgressionMapping.GetMonsterAnchorLevel(powerLevel)), Is.EqualTo(set.AnchorLevel));
    }

    /// <summary>
    /// The power level keeps Season 21's order.
    /// </summary>
    [Test]
    public void PowerLevelIsMonotone()
    {
        var configuration = new ProgressionConfiguration();
        var previous = 0.0;
        for (var requirement = 1; requirement <= 1750; requirement++)
        {
            var powerLevel = ProgressionMapping.GetPowerLevel(requirement, configuration);
            Assert.That(powerLevel, Is.GreaterThan(previous), $"R {requirement}");
            previous = powerLevel;
        }
    }

    /// <summary>
    /// The original mode keeps the Season 21 numbers: a total split at level 400, the source zen.
    /// </summary>
    [Test]
    public void OriginalKeepsTheSourceNumbers()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original };

        Assert.That(ProgressionMapping.GetRequirement(850, configuration), Is.EqualTo(new MapRequirement(400, 450, 0)));
        Assert.That(ProgressionMapping.GetRequirement(300, configuration), Is.EqualTo(new MapRequirement(300, 0, 0)));
        Assert.That(ProgressionMapping.GetEntryZen(850, 50_000, configuration), Is.EqualTo(50_000));
        Assert.That(ProgressionMapping.GetClassicEntryLevel(280, 380, configuration), Is.EqualTo(380));
        Assert.That(ProgressionMapping.GetDropLevel(846, configuration), Is.EqualTo(846));
    }

    /// <summary>
    /// The anchors, the rounding and the flat zen are settings.
    /// </summary>
    [Test]
    public void AnchorsRoundingAndZenAreSettings()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Normalized, Anchor400 = 200, RequirementRounding = 10, EntryZenFlat = 30_000 };

        Assert.That(ProgressionMapping.GetPowerLevel(400, configuration), Is.EqualTo(200).Within(1e-9));
        Assert.That(ProgressionMapping.GetRequirement(630, configuration).Level % 10, Is.Zero);
        Assert.That(ProgressionMapping.GetEntryZen(630, 50_000, configuration), Is.EqualTo(30_000));
    }

    /// <summary>
    /// Settings outside their ranges are clamped.
    /// </summary>
    [Test]
    public void SettingsOutsideTheRangesAreClamped()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Normalized, Anchor400 = 1, Anchor1000 = 9999, LevelCap = 5000, MasterLevelCap = 900 };

        Assert.That(ProgressionMapping.GetPowerLevel(400, configuration), Is.EqualTo(150).Within(1e-9));
        Assert.That(ProgressionMapping.GetPowerLevel(1000, configuration), Is.EqualTo(400).Within(1e-9));
        Assert.That(ProgressionMapping.GetLevelCap(configuration), Is.EqualTo(400));
        Assert.That(ProgressionMapping.GetMasterLevelCap(configuration), Is.EqualTo(200));
    }

    /// <summary>
    /// Normalized master levels stop at the master level cap.
    /// </summary>
    [Test]
    public void NormalizedMasterLevelStopsAtTheCap()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Normalized, MasterLevelCap = 100 };

        Assert.That(ProgressionMapping.GetRequirement(1600, configuration), Is.EqualTo(new MapRequirement(400, 100, 0)));
    }

    /// <summary>
    /// The settings without a value take the mode's default.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="masterLevelCap">The expected master level cap.</param>
    /// <param name="bracketKey">The expected event bracket key.</param>
    /// <param name="itemKey">The expected item level requirement key.</param>
    [TestCase(ProgressionMode.Original, 1350, ProgressionLevelKey.Season21Total, ProgressionLevelKey.Level)]
    [TestCase(ProgressionMode.Normalized, 200, ProgressionLevelKey.LevelAndMasterLevel, ProgressionLevelKey.Level)]
    [TestCase(ProgressionMode.Stars, 0, ProgressionLevelKey.PowerLevel, ProgressionLevelKey.PowerLevel)]
    public void ModeDefaults(ProgressionMode mode, int masterLevelCap, ProgressionLevelKey bracketKey, ProgressionLevelKey itemKey)
    {
        var configuration = new ProgressionConfiguration { Mode = mode };

        Assert.That(ProgressionMapping.GetMasterLevelCap(configuration), Is.EqualTo(masterLevelCap));
        Assert.That(ProgressionMapping.GetEventBracketKey(configuration), Is.EqualTo(bracketKey));
        Assert.That(ProgressionMapping.GetItemLevelRequirementKey(configuration), Is.EqualTo(itemKey));
    }

    /// <summary>
    /// The defaults of the settings table.
    /// </summary>
    [Test]
    public void ConfigurationDefaults()
    {
        var configuration = new ProgressionFeaturePlugIn().CreateDefaultConfig() as ProgressionConfiguration;

        Assert.That(configuration, Is.Not.Null);
        Assert.That(configuration!.Mode, Is.EqualTo(ProgressionMode.Stars));
        Assert.That(configuration.LevelCap, Is.EqualTo(400));
        Assert.That(configuration.Anchor400, Is.EqualTo(190));
        Assert.That(configuration.Anchor1000, Is.EqualTo(400));
        Assert.That(configuration.RequirementRounding, Is.EqualTo(5));
        Assert.That(configuration.ClassicMapEntryLevels, Is.EqualTo(ClassicMapEntryLevels.Season6));
        Assert.That(configuration.MonsterDropLevelCap, Is.EqualTo(160));
        Assert.That(configuration.OverCapStrengthPercent, Is.EqualTo(85));
        Assert.That(configuration.StarBandStart, Is.EqualTo(400));
        Assert.That(configuration.StarBandWidth, Is.EqualTo(300));
        Assert.That(configuration.StarTreePoints, Is.EqualTo(20));
    }

    /// <summary>
    /// The classic maps keep their Season 6 levels by default; the option maps the Season 21 levels.
    /// </summary>
    [Test]
    public void ClassicEntryLevels()
    {
        var season6 = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var season21 = new ProgressionConfiguration { Mode = ProgressionMode.Stars, ClassicMapEntryLevels = ClassicMapEntryLevels.Season21 };
        var mapped = new ProgressionConfiguration { Mode = ProgressionMode.Stars, ClassicMapEntryLevels = ClassicMapEntryLevels.Season21Normalized };

        Assert.That(ProgressionMapping.GetClassicEntryLevel(280, 380, season6), Is.EqualTo(280));
        Assert.That(ProgressionMapping.GetClassicEntryLevel(280, 380, season21), Is.EqualTo(380));
        Assert.That(ProgressionMapping.GetClassicEntryLevel(280, 380, mapped), Is.EqualTo(182));
        Assert.That(ProgressionMapping.GetClassicEntryLevel(10, 1, mapped), Is.EqualTo(1));
    }

    /// <summary>
    /// Item drops roll with the capped monster level in the normalized modes; 0 turns the cap off.
    /// </summary>
    [Test]
    public void DropLevelCap()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var off = new ProgressionConfiguration { Mode = ProgressionMode.Stars, MonsterDropLevelCap = 0 };

        Assert.That(ProgressionMapping.GetDropLevel(187, configuration), Is.EqualTo(160));
        Assert.That(ProgressionMapping.GetDropLevel(140, configuration), Is.EqualTo(140));
        Assert.That(ProgressionMapping.GetDropLevel(187, off), Is.EqualTo(187));
    }

    /// <summary>
    /// The inverse of the curve gives back the Season 21 requirement.
    /// </summary>
    [Test]
    public void Season21RequirementIsTheInverse()
    {
        var configuration = new ProgressionConfiguration();

        Assert.That(ProgressionMapping.GetSeason21Requirement(190, configuration), Is.EqualTo(400).Within(1e-6));
        Assert.That(ProgressionMapping.GetSeason21Requirement(400, configuration), Is.EqualTo(1000).Within(1e-6));
        Assert.That(ProgressionMapping.GetSeason21Requirement(ProgressionMapping.GetPowerLevel(1350, configuration), configuration), Is.EqualTo(1350).Within(1e-6));
    }

    /// <summary>
    /// Event brackets: a Season 6 grade checks the capped key, a Season 21 grade the Season 21 total which the key stands
    /// for; the Season 21 total as key keeps the original check.
    /// </summary>
    [Test]
    public void EventBracketLevel()
    {
        var stars = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var normalized = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        var original = new ProgressionConfiguration { Mode = ProgressionMode.Normalized, EventBracketKey = ProgressionLevelKey.Season21Total };

        Assert.That(ProgressionMapping.GetEventBracketLevel(1199, 400, 200, 0, 2, stars), Is.EqualTo(1000));
        Assert.That(ProgressionMapping.GetEventBracketLevel(400, 400, 200, 0, 2, stars), Is.EqualTo(400));
        Assert.That(ProgressionMapping.GetEventBracketLevel(400, 400, 50, 0, 3, stars), Is.EqualTo(350));
        Assert.That(ProgressionMapping.GetEventBracketLevel(int.MaxValue, 400, 400, 185, 0, normalized), Is.EqualTo(1597).Within(3));
        Assert.That(ProgressionMapping.GetEventBracketLevel(400, 400, 400, 185, 0, normalized), Is.EqualTo(400));
        Assert.That(ProgressionMapping.GetEventBracketLevel(1199, 400, 400, 600, 0, original), Is.EqualTo(1000));
    }

    /// <summary>
    /// The master class grades need a star in the stars mode.
    /// </summary>
    [Test]
    public void MasterGradesNeedAStar()
    {
        var stars = new ProgressionConfiguration { Mode = ProgressionMode.Stars };

        Assert.That(ProgressionMapping.MeetsMasterGrade(true, 0, stars), Is.False);
        Assert.That(ProgressionMapping.MeetsMasterGrade(true, 1, stars), Is.True);
        Assert.That(ProgressionMapping.MeetsMasterGrade(false, 0, stars), Is.True);
        Assert.That(ProgressionMapping.MeetsMasterGrade(true, 0, new ProgressionConfiguration { Mode = ProgressionMode.Normalized }), Is.True);
    }
}
