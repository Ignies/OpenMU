// <copyright file="EvolutionRulesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Classes;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// Tests the <see cref="EvolutionRules"/>: the fourth evolution at a Season 21 total of 800, the fifth at 1200, read
/// through the progression mode.
/// </summary>
[TestFixture]
public class EvolutionRulesTest
{
    /// <summary>
    /// Without the progression and in the original mode a character needs the Season 21 total level.
    /// </summary>
    [Test]
    public void OriginalAsksForTheTotalLevel()
    {
        var original = new ProgressionConfiguration { Mode = ProgressionMode.Original };
        foreach (var configuration in new[] { null, original })
        {
            Assert.That(EvolutionRules.GetRequiredLevel(ClassEvolution.Fourth, configuration), Is.EqualTo(800));
            Assert.That(EvolutionRules.GetRequiredLevel(ClassEvolution.Fifth, configuration), Is.EqualTo(1200));
            Assert.That(EvolutionRules.IsMet(ClassEvolution.Fourth, 400, 400, 0, configuration), Is.True);
            Assert.That(EvolutionRules.IsMet(ClassEvolution.Fourth, 400, 399, 9, configuration), Is.False);
            Assert.That(EvolutionRules.IsMet(ClassEvolution.Fifth, 400, 800, 0, configuration), Is.True);
        }
    }

    /// <summary>
    /// The normalized mode asks for T(800) = 335 and T(1200) = 465 levels plus master levels.
    /// </summary>
    [Test]
    public void NormalizedAsksForTheMappedLevel()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Normalized };

        Assert.That(EvolutionRules.GetRequiredLevel(ClassEvolution.Fourth, configuration), Is.EqualTo(335));
        Assert.That(EvolutionRules.GetRequiredLevel(ClassEvolution.Fifth, configuration), Is.EqualTo(465));
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fourth, 335, 0, 0, configuration), Is.True);
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fifth, 400, 64, 0, configuration), Is.False);
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fifth, 400, 65, 0, configuration), Is.True);
    }

    /// <summary>
    /// The stars mode asks for the power level E: a fifth evolution needs a star, as no level reaches 465.
    /// </summary>
    [Test]
    public void StarsAskForThePowerLevel()
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars };

        Assert.That(EvolutionRules.GetRequiredLevel(ClassEvolution.Fourth, configuration), Is.EqualTo(335));
        Assert.That(EvolutionRules.GetRequiredLevel(ClassEvolution.Fifth, configuration), Is.EqualTo(465));
        Assert.That(EvolutionRules.GetLevelKey(configuration), Is.EqualTo(ProgressionLevelKey.PowerLevel));
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fourth, 334, 0, 0, configuration), Is.False);
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fourth, 235, 0, 1, configuration), Is.True);
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fifth, 400, 200, 0, configuration), Is.False);
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fifth, 365, 0, 1, configuration), Is.True);
        Assert.That(EvolutionRules.IsMet(ClassEvolution.Fifth, 265, 0, 2, configuration), Is.True);
    }

    /// <summary>
    /// The first three evolutions keep their quests and levels; the rules don't ask for anything.
    /// </summary>
    /// <param name="evolution">The evolution.</param>
    [TestCase(ClassEvolution.First)]
    [TestCase(ClassEvolution.Second)]
    [TestCase(ClassEvolution.Third)]
    public void TheFirstThreeEvolutionsHaveNoRule(ClassEvolution evolution)
    {
        var configuration = new ProgressionConfiguration { Mode = ProgressionMode.Stars };
        Assert.That(EvolutionRules.GetRequiredLevel(evolution, configuration), Is.Zero);
        Assert.That(EvolutionRules.IsMet(evolution, 1, 0, 0, configuration), Is.True);
    }
}
