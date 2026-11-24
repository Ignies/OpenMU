// <copyright file="StarSmithConfigValidatorTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The one validator of the recipes and the balance (Readme.md "PR 6 contract", "Configuration JSON"): it runs at load
/// and on every bridge write, and each refusal names its field as the camelCase JSON path.
/// </summary>
[TestFixture]
public class StarSmithConfigValidatorTest
{
    private static readonly Func<byte, short, bool> AnyItem = (_, _) => true;

    /// <summary>
    /// The defaults are valid.
    /// </summary>
    [Test]
    public void TheDefaultsAreValid()
    {
        Assert.That(StarSmithConfigValidator.Validate(new StarSmithingConfiguration(), AnyItem), Is.Empty);
    }

    /// <summary>
    /// A broken recipe names each field at fault.
    /// </summary>
    [Test]
    public void RecipeRefusalsNameTheirFields()
    {
        var good = StarSmithingConfiguration.DefaultRecipes()[0];
        var bad = StarSmithingConfiguration.DefaultRecipes()[1];
        bad.Number = good.Number;
        bad.Name = new string('x', 65);
        bad.OnFail = StarSmithOnFail.ItemDestroyed;
        bad.Risky = false;
        bad.Geometry!.Nodes = 13;
        bad.Outcomes[0].Boosts[0].Maximum = 99;
        bad.HintKind = StarSmithHintKind.Ingredient;
        bad.HintGroup = 1;
        bad.HintNumber = 1;
        bad.Targets = StarSmithTargetKind.None;
        bad.Catalysts = [new StarSmithCatalyst { Group = 14, Number = 13, Chance = 20_000 }];
        var config = new StarSmithingConfiguration { Recipes = [good, bad] };

        var fields = StarSmithConfigValidator.Validate(config, AnyItem).Select(e => e.Field).ToList();

        Assert.That(fields, Is.SupersetOf(new[]
        {
            "recipes[1].number",
            "recipes[1].name",
            "recipes[1].onFail",
            "recipes[1].geometry.nodes",
            "recipes[1].outcomes[0].boosts[0].max",
            "recipes[1].hint.number",
            "recipes[1].targets",
            "recipes[1].catalysts[0].chance",
        }));
        Assert.That(fields.Where(f => f.StartsWith("recipes[0]", StringComparison.Ordinal)), Is.Empty);
    }

    /// <summary>
    /// An item the data does not have is refused where it is named; a riddle needs its text.
    /// </summary>
    [Test]
    public void UnknownItemsAndEmptyRiddles()
    {
        var recipe = StarSmithingConfiguration.DefaultRecipes()[0];
        recipe.Hint = string.Empty;
        recipe.HintKind = StarSmithHintKind.Riddle;
        var errors = StarSmithConfigValidator.Validate(new StarSmithingConfiguration { Recipes = [recipe] }, (group, number) => !(group == 12 && number == 7));
        Assert.That(errors.Select(e => e.Field), Is.EquivalentTo(new[] { "recipes[0].ingredients[0]", "recipes[0].hint.text" }));
        Assert.That(errors.All(e => e.Message.Length > 0), Is.True);
    }

    /// <summary>
    /// The balance: six tiers, colours, caps in order and never past the ceiling.
    /// </summary>
    [Test]
    public void BalanceRefusalsNameTheirFields()
    {
        var balance = new StarSmithBalance();
        balance.Tiers[2].Color = 0x1000000;
        balance.Tiers[3].BoostSlots = 4;
        balance.Pve[StarSmithBoostStat.Damage] = new StarSmithCap(20, 10);
        balance.Pvp[StarSmithBoostStat.Damage] = new StarSmithCap(5, 31);
        balance.Pvp.Remove(StarSmithBoostStat.MaxMana);
        balance.Scaling.Base = 2;
        balance.LevelCurve.Exponent = 0.5;
        var fields = StarSmithConfigValidator.Validate(new StarSmithingConfiguration { Balance = balance }, AnyItem).Select(e => e.Field);

        Assert.That(fields, Is.EquivalentTo(new[]
        {
            "balance.tiers[2].color",
            "balance.tiers[3].boostSlots",
            "balance.caps.pve.damage.perItem",
            "balance.caps.pvp.damage.total",
            "balance.caps.pvp.maxMana",
            "balance.scaling.factor",
            "balance.levelCurve.exponent",
        }));

        balance = new StarSmithBalance();
        balance.Tiers.RemoveAt(5);
        Assert.That(StarSmithConfigValidator.Validate(new StarSmithingConfiguration { Balance = balance }, AnyItem).Select(e => e.Field), Is.EqualTo(new[] { "balance.tiers" }));
    }

    /// <summary>
    /// At load a recipe that would break the game is dropped and a bad balance falls back to the default; the good rest
    /// stays, and a recipe that only lacks a name or a table shows less but stays.
    /// </summary>
    [Test]
    public void LoadDropsWhatIsBroken()
    {
        var bad = StarSmithingConfiguration.DefaultRecipes()[1];
        bad.Outcomes[0].Boosts[0].Maximum = 99;
        var plain = new StarSmithRecipe { Number = 3, Targets = StarSmithTargetKind.Weapon, Geometry = StarSmithGeometry.Of(3, false, [45], [1, 3]) };
        var balance = new StarSmithBalance();
        balance.Tiers.Clear();
        var config = new StarSmithingConfiguration { Recipes = [StarSmithingConfiguration.DefaultRecipes()[0], bad, plain], Balance = balance };

        var dropped = StarSmithConfigValidator.Sanitize(config);

        Assert.That(config.Recipes.Select(r => r.Number), Is.EqualTo(new short[] { 1, 3 }));
        Assert.That(config.Balance.Tiers, Has.Count.EqualTo(6));
        Assert.That(dropped, Is.Not.Empty);
    }

    /// <summary>
    /// PR 7a: a shape's counts, turns and lengths are checked, and a shape too close to another recipe's (with or
    /// without the extension, on a shared item kind) is refused; at load the later one is dropped.
    /// </summary>
    [Test]
    public void ShapesAreCheckedAndKeptApart()
    {
        var fire = StarSmithingConfiguration.DefaultRecipes().Single(r => r.Number == 101);
        var broken = StarSmithingConfiguration.DefaultRecipes().Single(r => r.Number == 102);
        broken.Geometry = new StarSmithGeometry { Nodes = 4, Turns = [5, 175, 60], Lengths = [2, 0.1, 1], Extension = new() { Turn = 0, Length = 9 } };
        var near = StarSmithingConfiguration.DefaultRecipes().Single(r => r.Number == 103);
        near.Geometry = StarSmithGeometry.Of(3, true, [112, 125], [1, 1.1, 0.9]);
        var config = new StarSmithingConfiguration { Recipes = [fire, broken, near] };

        var errors = StarSmithConfigValidator.Validate(config, AnyItem);
        Assert.That(errors.Select(e => e.Field), Is.EquivalentTo(new[]
        {
            "recipes[1].geometry.turns",
            "recipes[1].geometry.turns[0]",
            "recipes[1].geometry.turns[1]",
            "recipes[1].geometry.lengths[0]",
            "recipes[1].geometry.lengths[1]",
            "recipes[1].geometry.extension.turn",
            "recipes[1].geometry.extension.length",
            "recipes[2].geometry",
        }));
        Assert.That(errors.Single(e => e.Field == "recipes[2].geometry").Message, Does.Contain("101"));
        Assert.That(StarSmithConfigValidator.Validate(new StarSmithingConfiguration(), AnyItem), Is.Empty, "the defaults are apart and valid");

        StarSmithConfigValidator.Sanitize(config);
        Assert.That(config.Recipes.Select(r => r.Number), Is.EqualTo(new short[] { 101 }));
    }
}
