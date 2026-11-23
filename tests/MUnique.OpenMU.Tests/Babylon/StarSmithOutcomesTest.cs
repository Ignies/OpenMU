// <copyright file="StarSmithOutcomesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A seal's outcome (Readme.md "PR 6 contract", "Outcomes"): one outcome by weight among those the forge's rarity
/// reaches, each boost rolled in its range, scaled, capped per item, and only as many as the tier has slots.
/// </summary>
[TestFixture]
public class StarSmithOutcomesTest
{
    private static readonly StarSmithBalance Balance = new();

    /// <summary>
    /// An outcome above the forge's rarity is never rolled, however heavy; with none in reach the lowest tier comes.
    /// </summary>
    [Test]
    public void RollsOnlyOutcomesTheRarityReaches()
    {
        var recipe = Recipe(
            new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Common, Boosts = [Range(StarSmithBoostStat.Defense, 1, 1)] },
            new StarSmithOutcome { Weight = 10_000, Tier = StarSmithRarity.Epic, Boosts = [Range(StarSmithBoostStat.Damage, 1, 1)] });
        for (var seed = 0L; seed < 50; seed++)
        {
            Assert.That(StarSmithOutcomes.Roll(recipe, StarSmithRarity.Uncommon, 1, seed, Balance)!.OutcomeIndex, Is.EqualTo(0));
        }

        var high = Recipe(new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Rare, Boosts = [Range(StarSmithBoostStat.Damage, 1, 1)] });
        Assert.That(StarSmithOutcomes.Roll(high, StarSmithRarity.Common, 1, 7, Balance)!.OutcomeIndex, Is.EqualTo(0), "none in reach: the lowest tier");
    }

    /// <summary>
    /// The weights decide among the outcomes in reach.
    /// </summary>
    [Test]
    public void WeightsDecide()
    {
        var recipe = Recipe(
            new StarSmithOutcome { Weight = 9_000, Tier = StarSmithRarity.Common, Boosts = [Range(StarSmithBoostStat.Defense, 1, 1)] },
            new StarSmithOutcome { Weight = 1_000, Tier = StarSmithRarity.Common, Boosts = [Range(StarSmithBoostStat.Damage, 1, 1)] });
        var second = Enumerable.Range(0, 2000).Count(s => StarSmithOutcomes.Roll(recipe, StarSmithRarity.Common, 1, s, Balance)!.OutcomeIndex == 1);
        Assert.That(second, Is.InRange(120, 280));
    }

    /// <summary>
    /// A boost is rolled in its range and multiplied by the seal's magnitude factor, stored times 100.
    /// </summary>
    [Test]
    public void ScalesTheRolledValue()
    {
        var recipe = Recipe(new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Common, Boosts = [Range(StarSmithBoostStat.Defense, 2, 2)] });
        Assert.That(StarSmithOutcomes.Roll(recipe, StarSmithRarity.Common, 1.25, 3, Balance)!.Boosts, Is.EqualTo(new[] { new StarSmithBoost(StarSmithBoostStat.Defense, 250) }));

        var ranged = Recipe(new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Common, Boosts = [Range(StarSmithBoostStat.MaxHealth, 100, 200)] });
        var values = Enumerable.Range(0, 200).Select(s => StarSmithOutcomes.Roll(ranged, StarSmithRarity.Common, 1, s, Balance)!.Boosts[0].Value).ToList();
        Assert.That(values, Is.All.InRange(10_000, 20_000));
        Assert.That(values.Distinct().Count(), Is.GreaterThan(20));
    }

    /// <summary>
    /// The PvE per-item cap holds after scaling.
    /// </summary>
    [Test]
    public void CapsPerItem()
    {
        var recipe = Recipe(new StarSmithOutcome { Weight = 1, Tier = StarSmithRarity.Common, Boosts = [Range(StarSmithBoostStat.Damage, 10, 10)] });
        Assert.That(StarSmithOutcomes.Roll(recipe, StarSmithRarity.Common, 1.5, 1, Balance)!.Boosts[0].Value, Is.EqualTo(600));
    }

    /// <summary>
    /// The forge's rarity tier caps how many boost slots fill: Common one, Epic three.
    /// </summary>
    [Test]
    public void FillsTheTiersSlots()
    {
        var outcome = new StarSmithOutcome
        {
            Weight = 1,
            Tier = StarSmithRarity.Common,
            Boosts = [Range(StarSmithBoostStat.Damage, 1, 1), Range(StarSmithBoostStat.Defense, 1, 1), Range(StarSmithBoostStat.MaxMana, 1, 1)],
        };
        var recipe = Recipe(outcome);
        Assert.That(StarSmithOutcomes.Roll(recipe, StarSmithRarity.Common, 1, 1, Balance)!.Boosts.Select(b => b.Stat), Is.EqualTo(new[] { StarSmithBoostStat.Damage }));
        Assert.That(StarSmithOutcomes.Roll(recipe, StarSmithRarity.Epic, 1, 1, Balance)!.Boosts, Has.Count.EqualTo(3));
    }

    /// <summary>
    /// The same seed rolls the same outcome and values, so a replay verifies it; a recipe without outcomes gives none.
    /// </summary>
    [Test]
    public void IsDeterministic()
    {
        var recipe = StarSmithingConfiguration.DefaultRecipes()[0];
        var a = StarSmithOutcomes.Roll(recipe, StarSmithRarity.Mythic, 1.2, 42, Balance)!;
        var b = StarSmithOutcomes.Roll(recipe, StarSmithRarity.Mythic, 1.2, 42, Balance)!;
        Assert.That(b.OutcomeIndex, Is.EqualTo(a.OutcomeIndex));
        Assert.That(b.Boosts, Is.EqualTo(a.Boosts));
        Assert.That(StarSmithOutcomes.Roll(new StarSmithRecipe(), StarSmithRarity.Mythic, 1, 42, Balance), Is.Null);
    }

    /// <summary>
    /// Boosts are stored on the record as "stat:value" pairs, read back the same, unreadable parts skipped.
    /// </summary>
    [Test]
    public void BoostsRoundTripAsText()
    {
        StarSmithBoost[] boosts = [new(StarSmithBoostStat.Damage, 625), new(StarSmithBoostStat.MaxHealth, 30_000)];
        var text = StarSmithBoosts.Format(boosts);
        Assert.That(text, Is.EqualTo("0:625;2:30000"));
        Assert.That(StarSmithBoosts.Parse(text), Is.EqualTo(boosts));
        Assert.That(StarSmithBoosts.Parse("x;9:5;1:12;"), Is.EqualTo(new[] { new StarSmithBoost(StarSmithBoostStat.Defense, 12) }));
        Assert.That(StarSmithBoosts.Parse(null), Is.Empty);
    }

    private static StarSmithBoostRange Range(StarSmithBoostStat stat, double min, double max) => new() { Stat = stat, Minimum = min, Maximum = max };

    private static StarSmithRecipe Recipe(params StarSmithOutcome[] outcomes) => new() { Number = 1, Outcomes = [.. outcomes] };
}
