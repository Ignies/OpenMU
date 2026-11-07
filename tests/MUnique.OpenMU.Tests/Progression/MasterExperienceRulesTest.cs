// <copyright file="MasterExperienceRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Progression;

using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests for <see cref="MasterExperienceRules"/>: from a total level of 800, Season 21 gives master experience only for
/// monsters of level 160 (S21Emu <c>MinMasterExperienceExtMonsterLevel</c>).
/// </summary>
[TestFixture]
public class MasterExperienceRulesTest
{
    /// <summary>
    /// Below the total level 800 the configured minimum stays; from 800 it's at least 160.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="masterLevel">The master level.</param>
    /// <param name="minimum">The expected minimum monster level.</param>
    [TestCase(400, 399, 95)]
    [TestCase(400, 400, 160)]
    [TestCase(400, 1000, 160)]
    public void MinimumMonsterLevel(int level, int masterLevel, int minimum)
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Original };
        Assert.That(MasterExperienceRules.GetMinimumMonsterLevel(level, masterLevel, 95, configuration), Is.EqualTo(minimum));
    }

    /// <summary>
    /// Without the progression, or with the gate at 0, the configured minimum stays.
    /// </summary>
    [Test]
    public void NoGateWithoutProgression()
    {
        Assert.That(MasterExperienceRules.GetMinimumMonsterLevel(400, 1000, 95, null), Is.EqualTo(95));
        Assert.That(MasterExperienceRules.GetMinimumMonsterLevel(400, 1000, 95, new ProgressionConfiguration { EnhancedMinimumMonsterLevel = 0 }), Is.EqualTo(95));
    }
}
