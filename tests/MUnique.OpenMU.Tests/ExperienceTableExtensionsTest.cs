// <copyright file="ExperienceTableExtensionsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using MUnique.OpenMU.GameLogic;

/// <summary>
/// Tests the <see cref="ExperienceTableExtensions"/>.
/// </summary>
[TestFixture]
public class ExperienceTableExtensionsTest
{
    /// <summary>
    /// The experience of the next level is the entry after the current level.
    /// </summary>
    [Test]
    public void NextLevelBelowTheCap()
    {
        long[] table = [0, 0, 100, 300, 600];

        Assert.That(table.GetExperienceOfNextLevel(2), Is.EqualTo(300));
    }

    /// <summary>
    /// A character above the level cap, e.g. after the cap was lowered, gets the experience of the last level of the
    /// table instead of an exception, so it can still log in and be shown.
    /// </summary>
    [Test]
    public void NextLevelAboveTheCapIsTheLastEntry()
    {
        long[] table = [0, 0, 100, 300, 600];

        Assert.That(table.GetExperienceOfNextLevel(10), Is.EqualTo(600));
    }
}
