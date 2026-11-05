// <copyright file="StarRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Resets;

/// <summary>
/// Tests for <see cref="StarRules"/> and <see cref="StarResetDefaults"/>.
/// </summary>
[TestFixture]
public class StarRulesTest
{
    private readonly ProgressionConfiguration _configuration = new() { Mode = ProgressionMode.Stars };

    /// <summary>
    /// Bands of 300 Season 21 levels above 400: 700-999 one star, 1000-1299 two, 1300-1599 three, 1600-1899 four.
    /// </summary>
    /// <param name="requirement">The Season 21 requirement.</param>
    /// <param name="stars">The expected stars.</param>
    [TestCase(400, 0)]
    [TestCase(699, 0)]
    [TestCase(700, 1)]
    [TestCase(999, 1)]
    [TestCase(1000, 2)]
    [TestCase(1300, 3)]
    [TestCase(1600, 4)]
    [TestCase(1899, 4)]
    public void StarsRequired(int requirement, int stars)
    {
        Assert.That(StarRules.GetStarsRequired(requirement, this._configuration), Is.EqualTo(stars));
    }

    /// <summary>
    /// A star is worth 100 power levels up to five stars, 50 after; at a band's entry the power level is the map's T.
    /// </summary>
    [Test]
    public void PowerLevel()
    {
        Assert.That(StarRules.GetStarWorth(0, this._configuration), Is.Zero);
        Assert.That(StarRules.GetStarWorth(5, this._configuration), Is.EqualTo(500));
        Assert.That(StarRules.GetStarWorth(10, this._configuration), Is.EqualTo(750));
        Assert.That(StarRules.GetPowerLevel(200, 1, this._configuration), Is.EqualTo(300));
        Assert.That(StarRules.GetPowerLevel(200, 2, this._configuration), Is.EqualTo(400));
        Assert.That(StarRules.GetPowerLevel(195, 3, this._configuration), Is.EqualTo(495));
        Assert.That(StarRules.GetPowerLevel(185, 4, this._configuration), Is.EqualTo(585));
    }

    /// <summary>
    /// The level a requirement is checked against, per key: the power level and level plus master level are capped.
    /// </summary>
    [Test]
    public void RequirementLevelPerKey()
    {
        Assert.That(StarRules.GetRequirementLevel(ProgressionLevelKey.Level, 300, 50, 1, this._configuration), Is.EqualTo(300));
        Assert.That(StarRules.GetRequirementLevel(ProgressionLevelKey.LevelAndMasterLevel, 380, 50, 0, this._configuration), Is.EqualTo(400));
        Assert.That(StarRules.GetRequirementLevel(ProgressionLevelKey.PowerLevel, 300, 0, 1, this._configuration), Is.EqualTo(400));
        Assert.That(StarRules.GetRequirementLevel(ProgressionLevelKey.PowerLevel, 10, 0, 1, this._configuration), Is.EqualTo(110));
        Assert.That(StarRules.GetRequirementLevel(ProgressionLevelKey.PowerLevel, 400, 0, 3, this._configuration), Is.EqualTo(400));
        Assert.That(StarRules.GetRequirementLevel(ProgressionLevelKey.Season21Total, 400, 600, 0, this._configuration), Is.EqualTo(1000));
    }

    /// <summary>
    /// Stat points per star: 100 levels' worth for stars 1-5 and 50 levels' worth after, scaled by the class's points per level.
    /// </summary>
    /// <param name="star">The star.</param>
    /// <param name="pointsPerLevel">The class's points per level.</param>
    /// <param name="points">The expected points.</param>
    [TestCase(1, 5, 500)]
    [TestCase(5, 5, 500)]
    [TestCase(6, 5, 250)]
    [TestCase(1, 7, 700)]
    [TestCase(10, 7, 350)]
    [TestCase(11, 5, 250)]
    public void PointsPerStar(int star, int pointsPerLevel, int points)
    {
        Assert.That(StarRules.GetPointsForStar(star, pointsPerLevel, this._configuration), Is.EqualTo(points));
    }

    /// <summary>
    /// The free points after a star: all star points plus the quest points (the balance table: 10 stars, 3,750 + 80).
    /// </summary>
    [Test]
    public void FreePointsAfterAStarKeepTheQuestPoints()
    {
        Assert.That(StarRules.GetTotalStarPoints(4, 5, this._configuration), Is.EqualTo(2000));
        Assert.That(StarRules.GetTotalStarPoints(10, 5, this._configuration), Is.EqualTo(3750));
        Assert.That(StarRules.GetFreePointsAfterStar(10, 5, 80, this._configuration), Is.EqualTo(3830));
    }

    /// <summary>
    /// Master tree points: 20 a star, 200 at ten stars, the Season 6 master budget.
    /// </summary>
    [Test]
    public void TreePoints()
    {
        Assert.That(StarRules.GetTreePoints(1, this._configuration), Is.EqualTo(20));
        Assert.That(StarRules.GetTreePoints(10, this._configuration), Is.EqualTo(200));
    }

    /// <summary>
    /// The power bonus: 1 percent a star, capped at 10, against monsters only by default.
    /// </summary>
    [Test]
    public void PowerBonusIsCappedAndPvmOnly()
    {
        Assert.That(StarRules.GetPowerBonusPercent(3, false, this._configuration), Is.EqualTo(3));
        Assert.That(StarRules.GetPowerBonusPercent(15, false, this._configuration), Is.EqualTo(10));
        Assert.That(StarRules.GetPowerBonusPercent(3, true, this._configuration), Is.Zero);
        Assert.That(StarRules.GetPowerBonusPercent(3, true, new ProgressionConfiguration { StarPowerInPvp = true }), Is.EqualTo(3));
    }

    /// <summary>
    /// The experience penalty: 6 percent a star, at least 40 percent left.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="factor">The expected experience factor.</param>
    [TestCase(0, 1.0)]
    [TestCase(1, 0.94)]
    [TestCase(5, 0.70)]
    [TestCase(10, 0.40)]
    [TestCase(12, 0.40)]
    public void ExperienceFactor(int stars, double factor)
    {
        Assert.That(StarRules.GetExperienceFactor(stars, this._configuration), Is.EqualTo(factor).Within(1e-9));
    }

    /// <summary>
    /// The cosmetic tiers: 1-3 Bronze, 4-6 Silver, 7-9 Gold, 10 and more Crown.
    /// </summary>
    /// <param name="stars">The stars.</param>
    /// <param name="tier">The expected tier.</param>
    [TestCase(0, StarTier.None)]
    [TestCase(1, StarTier.Bronze)]
    [TestCase(3, StarTier.Bronze)]
    [TestCase(4, StarTier.Silver)]
    [TestCase(7, StarTier.Gold)]
    [TestCase(9, StarTier.Gold)]
    [TestCase(10, StarTier.Crown)]
    [TestCase(50, StarTier.Crown)]
    public void Tiers(int stars, StarTier tier)
    {
        Assert.That(StarRules.GetTier(stars), Is.EqualTo(tier));
    }

    /// <summary>
    /// The requirements per star with the seeded reset configuration: level 400, 5,000,000 zen a star, a Jewel of Life
    /// more per star from star 6, the third class, ten stars at most.
    /// </summary>
    [Test]
    public void RequirementsPerStar()
    {
        var reset = new ResetConfiguration();
        StarResetDefaults.ApplyTo(reset, JewelOfLife());

        var expectedItems = new[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5 };
        for (var star = 1; star <= 10; star++)
        {
            var requirement = StarRules.GetRequirement(star - 1, reset, this._configuration);
            Assert.That(requirement, Is.EqualTo(new StarRequirement(star, 400, 5_000_000 * star, expectedItems[star - 1], true, false)), $"star {star}");
        }

        Assert.That(StarRules.GetRequirement(10, reset, this._configuration).LimitReached, Is.True);
        Assert.That(reset.LevelAfterReset, Is.EqualTo(10));
        Assert.That(reset.ResetStats, Is.True);
    }

    /// <summary>
    /// The star defaults only fill fields which are still at OpenMU's defaults, so an operator's values stay.
    /// </summary>
    [Test]
    public void StarDefaultsKeepOperatorValues()
    {
        var reset = new ResetConfiguration { RequiredMoney = 777, ResetLimit = 3, LevelAfterReset = 1 };
        reset.ItemCostTiers.Add(new ResetConfiguration.ResetItemCostTier { MinimumResetCount = 1, RequiredItemAmount = 9 });

        StarResetDefaults.ApplyTo(reset, JewelOfLife());

        Assert.That(reset.RequiredMoney, Is.EqualTo(777));
        Assert.That(reset.ResetLimit, Is.EqualTo(3));
        Assert.That(reset.LevelAfterReset, Is.EqualTo(1));
        Assert.That(reset.ItemCostTiers, Has.Count.EqualTo(1));
        Assert.That(reset.RequiredResetItem?.Number, Is.EqualTo(16));
    }

    private static ItemDefinition JewelOfLife() => new() { Name = "Jewel of Life", Group = 14, Number = 16, Width = 1, Height = 1 };
}
