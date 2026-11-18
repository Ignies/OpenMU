// <copyright file="StarSmithLevelsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The smith's levels (docs/star_smithing.md "Smith level and scaling"): 1 to 100, round(100 * L^1.6) experience to
/// the next level.
/// </summary>
[TestFixture]
public class StarSmithLevelsTest
{
    /// <summary>
    /// The experience a level needs to reach the next one.
    /// </summary>
    [TestCase(1, 100)]
    [TestCase(2, 303)]
    [TestCase(10, 3981)]
    [TestCase(50, 52282)]
    [TestCase(99, 155961)]
    [TestCase(100, 0)]
    [TestCase(0, 100)]
    public void ToTheNextLevel(int level, long experience)
    {
        Assert.That(StarSmithLevels.ToNext(level), Is.EqualTo(experience));
    }

    /// <summary>
    /// Experience earned raises the level as far as it reaches and keeps the rest; level 100 keeps none.
    /// </summary>
    [Test]
    public void EarningExperience()
    {
        Assert.That(StarSmithLevels.Add(1, 0, 450), Is.EqualTo((3, 47L)));
        Assert.That(StarSmithLevels.Add(99, 157000, 1000), Is.EqualTo((100, 0L)));
        Assert.That(StarSmithLevels.Add(5, 10, 0), Is.EqualTo((5, 10L)));
    }
}
