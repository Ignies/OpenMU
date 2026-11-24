// <copyright file="StarSmithRecipesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The recipe engine's pure parts (Hidden-Babylon docs/star_smithing.md sections 3 and 9): which recipe a target and its
/// ingredients make, the chance of an attempt, and how the smith's level scales what it rolls.
/// </summary>
[TestFixture]
public class StarSmithRecipesTest
{
    private static readonly StarSmithRecipe Ember = new()
    {
        Number = 1,
        Targets = StarSmithTargetKind.Weapon,
        Ingredients = [new() { Group = 12, Number = 7, Minimum = 2, Maximum = 4 }],
        BaseChance = 4000,
        MaximumChance = 8000,
    };

    private static readonly StarSmithRecipe EmberPlus = new()
    {
        Number = 2,
        Targets = StarSmithTargetKind.Weapon,
        MinimumItemLevel = 9,
        Ingredients = [new() { Group = 12, Number = 7, Minimum = 2, Maximum = 4 }, new() { Group = 14, Number = 13, Minimum = 1, Maximum = 1 }],
        BaseChance = 2500,
        MaximumChance = 6000,
    };

    private static readonly StarSmithRecipe Ward = new()
    {
        Number = 3,
        Targets = StarSmithTargetKind.Armor | StarSmithTargetKind.Wings,
        Ingredients = [new() { Group = 15, Number = 3, Minimum = 1, Maximum = 10 }],
        BaseChance = 5000,
        MaximumChance = 9000,
    };

    private static readonly StarSmithRecipe[] Book = [Ember, EmberPlus, Ward];

    /// <summary>
    /// The ingredients make the recipe whose rule fits the target and whose counts they meet; order does not matter.
    /// </summary>
    [Test]
    public void MatchesByTargetAndCounts()
    {
        var sword = new StarSmithTarget(StarSmithTargetKind.Weapon, Level: 7, Excellent: false);
        Assert.That(StarSmithRecipes.Match(Book, sword, [(12, 7, 3)]), Is.SameAs(Ember));
        Assert.That(StarSmithRecipes.Match(Book, sword, [(12, 7, 5)]), Is.Null, "more than the recipe takes");
        Assert.That(StarSmithRecipes.Match(Book, sword, [(12, 7, 1)]), Is.Null, "fewer than the recipe takes");
        Assert.That(StarSmithRecipes.Match(Book, sword, [(12, 7, 2), (13, 1, 1)]), Is.Null, "an ingredient no recipe takes");
        Assert.That(StarSmithRecipes.Match(Book, new StarSmithTarget(StarSmithTargetKind.Wings, 0, false), [(15, 3, 6)]), Is.SameAs(Ward));
        Assert.That(StarSmithRecipes.Match(Book, new StarSmithTarget(StarSmithTargetKind.Ring, 0, false), [(15, 3, 6)]), Is.Null, "not for rings");
    }

    /// <summary>
    /// Of the recipes that fit, the most specific wins: more kinds of ingredient, then a higher item level.
    /// </summary>
    [Test]
    public void MostSpecificWins()
    {
        var high = new StarSmithTarget(StarSmithTargetKind.Weapon, Level: 11, Excellent: true);
        Assert.That(StarSmithRecipes.Match(Book, high, [(14, 13, 1), (12, 7, 2)]), Is.SameAs(EmberPlus));
        Assert.That(StarSmithRecipes.Match(Book, high with { Level = 8 }, [(14, 13, 1), (12, 7, 2)]), Is.Null, "below its item level");
        Assert.That(StarSmithRecipes.Match(Book, high, [(12, 7, 2)]), Is.SameAs(Ember));
    }

    /// <summary>
    /// The chance: the base, plus 0.15% a smith level (15% at most), plus the catalysts, capped by the recipe's maximum.
    /// </summary>
    [Test]
    public void ChanceOfAnAttempt()
    {
        Assert.That(StarSmithRecipes.Chance(Ember, smithLevel: 1, catalysts: 0), Is.EqualTo(4015));
        Assert.That(StarSmithRecipes.Chance(Ember, smithLevel: 40, catalysts: 500), Is.EqualTo(5100));
        Assert.That(StarSmithRecipes.Chance(Ember, smithLevel: 100, catalysts: 0), Is.EqualTo(5500));
        Assert.That(StarSmithRecipes.Chance(Ember, smithLevel: 100, catalysts: 5000), Is.EqualTo(8000));
    }

    /// <summary>
    /// A rolled magnitude scales with the smith's level up to x1.5 at level 100.
    /// </summary>
    [Test]
    public void MagnitudeScalesWithTheSmith()
    {
        Assert.That(StarSmithRecipes.Magnitude(10, smithLevel: 0), Is.EqualTo(10).Within(1e-9));
        Assert.That(StarSmithRecipes.Magnitude(10, smithLevel: 100), Is.EqualTo(15).Within(1e-9));
        Assert.That(StarSmithRecipes.Magnitude(10, smithLevel: 50), Is.EqualTo(10 * (1 + (0.5 * Math.Pow(0.5, 0.8)))).Within(1e-9));
        Assert.That(StarSmithRecipes.Magnitude(10, smithLevel: 250), Is.EqualTo(15).Within(1e-9), "the level is capped at 100");
    }

    /// <summary>
    /// A roll under the chance succeeds (0 to 9,999).
    /// </summary>
    [Test]
    public void RollAgainstTheChance()
    {
        Assert.That(StarSmithRecipes.Succeeds(roll: 3999, chance: 4000), Is.True);
        Assert.That(StarSmithRecipes.Succeeds(roll: 4000, chance: 4000), Is.False);
    }

    /// <summary>
    /// The balance decides the level's chance bonus, the level curve and the scaling (Readme.md "PR 6 contract").
    /// </summary>
    [Test]
    public void TheBalanceTablesApply()
    {
        Assert.That(StarSmithRecipes.Chance(Ember, smithLevel: 100, catalysts: 0, new StarSmithChanceBonus { PerLevel = 10, Maximum = 500 }), Is.EqualTo(4500));
        Assert.That(StarSmithLevels.ToNext(2, new StarSmithCurve { Base = 50, Exponent = 1 }), Is.EqualTo(100));
        Assert.That(StarSmithLevels.Add(1, 0, 150, new StarSmithCurve { Base = 50, Exponent = 1 }), Is.EqualTo((3, 0L)));
        Assert.That(new StarSmithBalance { Scaling = new StarSmithCurve { Base = 1, Exponent = 1 } }.LevelFactor(50), Is.EqualTo(1.5).Within(1e-9));
    }
}
