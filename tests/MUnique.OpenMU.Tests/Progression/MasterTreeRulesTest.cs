// <copyright file="MasterTreeRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests for <see cref="MasterTreeRules"/>: Season 21 stops the master tree points at 400, the later levels feed the
/// skill enhancement tree.
/// </summary>
[TestFixture]
public class MasterTreeRulesTest
{
    /// <summary>
    /// The master levels up to 400 give their points, later ones none.
    /// </summary>
    /// <param name="masterLevel">The reached master level.</param>
    /// <param name="points">The expected points.</param>
    [TestCase(1, 1)]
    [TestCase(400, 1)]
    [TestCase(401, 0)]
    [TestCase(1350, 0)]
    public void PointsForMasterLevel(int masterLevel, int points)
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original };
        Assert.That(MasterTreeRules.GetPointsForMasterLevel(masterLevel, 1, configuration), Is.EqualTo(points));
    }

    /// <summary>
    /// Without the progression or with the cap at 0, every master level gives its points (Season 6).
    /// </summary>
    [Test]
    public void NoCapWithoutProgression()
    {
        Assert.That(MasterTreeRules.GetPointsForMasterLevel(1000, 1, null), Is.EqualTo(1));
        Assert.That(MasterTreeRules.GetPointsForMasterLevel(1000, 1, new ProgressionConfiguration { MasterTreePointsCap = 0 }), Is.EqualTo(1));
    }

    /// <summary>
    /// A level which crosses the cap gives only the points up to it.
    /// </summary>
    [Test]
    public void LevelCrossingTheCapGivesTheRest()
    {
        var configuration = new ProgressionConfiguration { MasterTreePointsCap = 401 };
        Assert.That(MasterTreeRules.GetPointsForMasterLevel(201, 2, configuration), Is.EqualTo(1));
    }

    /// <summary>
    /// The stars' master tree points stop at the same cap: 20 stars of 20 points are 400, more give nothing.
    /// </summary>
    [Test]
    public void StarTreePointsAreCapped()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        Assert.That(StarRules.GetTreePoints(10, configuration), Is.EqualTo(200));
        Assert.That(StarRules.GetTreePoints(20, configuration), Is.EqualTo(400));
        Assert.That(StarRules.GetTreePoints(30, configuration), Is.EqualTo(400));
    }
}
