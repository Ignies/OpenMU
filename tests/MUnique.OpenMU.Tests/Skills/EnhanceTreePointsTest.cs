// <copyright file="EnhanceTreePointsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Skills;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests the points of the skill enhancement tree per progression mode (<see cref="EnhanceTreePoints"/>).
/// </summary>
[TestFixture]
public class EnhanceTreePointsTest
{
    private readonly EnhanceTreeConfiguration _configuration = new();

    /// <summary>
    /// Original: one point per level above the Season 21 total of 800, up to 950.
    /// </summary>
    [Test]
    public void OriginalCountsTheLevelsAbove800()
    {
        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 400, 500, 0, null, this._configuration), Is.EqualTo(100));
        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fifth, 400, 1350, 0, null, this._configuration), Is.EqualTo(950));
        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 400, 300, 0, null, this._configuration), Is.Zero);
    }

    /// <summary>
    /// Before the fourth evolution there is no tree.
    /// </summary>
    [Test]
    public void ThirdEvolutionHasNoPoints()
    {
        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Third, 400, 1350, 0, null, this._configuration), Is.Zero);
        Assert.That(EnhanceTreePoints.GetTotal(null, 400, 1350, 0, null, this._configuration), Is.Zero);
    }

    /// <summary>
    /// Normalized: level plus master level is read as its Season 21 total, so the master levels feed the tree.
    /// </summary>
    [Test]
    public void NormalizedReadsTheMasterLevels()
    {
        var progression = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };
        var expected = (int)Math.Round(ProgressionMapping.GetSeason21Requirement(600, progression)) - 800;

        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 400, 200, 0, progression, this._configuration), Is.EqualTo(expected));
        Assert.That(expected, Is.InRange(800, 900));
        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 400, 100, 0, progression, this._configuration), Is.LessThan(expected));
    }

    /// <summary>
    /// Stars: the power level (level plus the stars' worth) is read the same way, so each star adds points.
    /// </summary>
    [Test]
    public void StarsAddPoints()
    {
        var progression = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        var twoStars = EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 300, 0, 2, progression, this._configuration);
        var threeStars = EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 300, 0, 3, progression, this._configuration);

        Assert.That(twoStars, Is.GreaterThan(0));
        Assert.That(threeStars, Is.GreaterThan(twoStars));
        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fifth, 400, 0, 10, progression, this._configuration), Is.EqualTo(950));
    }

    /// <summary>
    /// The configured start and maximum apply.
    /// </summary>
    [Test]
    public void ConfigurationLimitsApply()
    {
        var configuration = new EnhanceTreeConfiguration { PointsStartAt = 700, MaximumPoints = 50 };

        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 400, 320, 0, null, configuration), Is.EqualTo(20));
        Assert.That(EnhanceTreePoints.GetTotal(ClassEvolution.Fourth, 400, 500, 0, null, configuration), Is.EqualTo(50));
    }
}
