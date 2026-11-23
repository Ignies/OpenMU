// <copyright file="StarSmithConfigValidator.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Text;

/// <summary>
/// The one validator of the recipes and the balance (Readme.md "PR 6 contract", "Configuration JSON"). The plug-in runs
/// it at load (<see cref="Sanitize"/>) and the website's bridge on every write; each refusal names its field as the
/// camelCase JSON path the site edits.
/// </summary>
public static class StarSmithConfigValidator
{
    /// <summary>The most recipes.</summary>
    public const int MaximumRecipes = 64;

    /// <summary>The most ingredients of a recipe.</summary>
    public const int MaximumIngredients = 40;

    /// <summary>The most catalysts of a recipe.</summary>
    public const int MaximumCatalysts = 4;

    /// <summary>The most outcomes of a recipe.</summary>
    public const int MaximumOutcomes = 16;

    /// <summary>The most bytes of a name or hint (the Guidebook's text field).</summary>
    public const int MaximumTextBytes = 64;

    /// <summary>
    /// Checks a configuration's recipes and balance.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="itemExists">Whether the item data has an item (group, number).</param>
    /// <returns>The errors, each with its field as the camelCase JSON path; empty when valid.</returns>
    public static IReadOnlyList<StarSmithConfigError> Validate(StarSmithingConfiguration config, Func<byte, short, bool> itemExists)
    {
        var errors = new List<StarSmithConfigError>();
        if (config.Recipes.Count > MaximumRecipes)
        {
            errors.Add(new("recipes", $"At most {MaximumRecipes} recipes."));
        }

        var numbers = new HashSet<short>();
        for (var i = 0; i < config.Recipes.Count; i++)
        {
            ValidateRecipe(config.Recipes[i], $"recipes[{i}]", numbers, config.Forge.StarCount, itemExists, errors);
        }

        ValidateBalance(config.Balance, errors);
        return errors;
    }

    /// <summary>
    /// Drops what would break the game at load: every recipe with an error in its number, chances, catalysts, fate,
    /// shape or outcomes (a missing name, hint or table only shows less; items are not checked here), and the balance
    /// (back to the default) when it has one.
    /// </summary>
    /// <param name="config">The configuration, changed in place.</param>
    /// <returns>The errors of what was dropped.</returns>
    public static IReadOnlyList<StarSmithConfigError> Sanitize(StarSmithingConfiguration config)
    {
        string[] critical = ["number", "baseChance", "maximumChance", "catalysts[", "onFail", "shape", "outcomes["];
        var dropped = new List<StarSmithConfigError>();
        var numbers = new HashSet<short>();
        var kept = new List<StarSmithRecipe>();
        for (var i = 0; i < config.Recipes.Count && kept.Count < MaximumRecipes; i++)
        {
            var at = $"recipes[{i}].";
            var errors = new List<StarSmithConfigError>();
            ValidateRecipe(config.Recipes[i], at[..^1], numbers, config.Forge.StarCount, (_, _) => true, errors);
            errors.RemoveAll(e => !critical.Any(c => e.Field[at.Length..].StartsWith(c, StringComparison.Ordinal)));
            if (errors.Count == 0)
            {
                kept.Add(config.Recipes[i]);
            }

            dropped.AddRange(errors);
        }

        config.Recipes = kept;
        var balance = new List<StarSmithConfigError>();
        ValidateBalance(config.Balance, balance);
        if (balance.Count > 0)
        {
            config.Balance = new StarSmithBalance();
            dropped.AddRange(balance);
        }

        return dropped;
    }

    /// <summary>
    /// The JSON name of a stat.
    /// </summary>
    /// <param name="stat">The stat.</param>
    /// <returns>Its camelCase name.</returns>
    public static string JsonName(StarSmithBoostStat stat) => char.ToLowerInvariant(stat.ToString()[0]) + stat.ToString()[1..];

    private static void ValidateRecipe(StarSmithRecipe recipe, string at, HashSet<short> numbers, int starCount, Func<byte, short, bool> itemExists, List<StarSmithConfigError> errors)
    {
        void Error(string field, string message) => errors.Add(new($"{at}.{field}", message));

        if (recipe.Number < 1)
        {
            Error("number", "A number from 1 to 32767.");
        }
        else if (!numbers.Add(recipe.Number))
        {
            Error("number", $"Recipe {recipe.Number} is there twice.");
        }

        if (recipe.Name.Trim().Length == 0 || Encoding.UTF8.GetByteCount(recipe.Name) > MaximumTextBytes)
        {
            Error("name", $"A name of 1 to {MaximumTextBytes} bytes.");
        }

        if (recipe.Targets == StarSmithTargetKind.None || ((int)recipe.Targets & ~31) != 0)
        {
            Error("targets", "At least one of weapon, armor, wings, pendant, ring.");
        }

        if (recipe.MinimumItemLevel > 15)
        {
            Error("minimumItemLevel", "From 0 to 15.");
        }

        if (recipe.Ingredients.Count is 0 or > MaximumIngredients)
        {
            Error("ingredients", $"1 to {MaximumIngredients} ingredients.");
        }

        var seen = new HashSet<(byte, short)>();
        for (var i = 0; i < recipe.Ingredients.Count; i++)
        {
            var ingredient = recipe.Ingredients[i];
            if (!itemExists(ingredient.Group, ingredient.Number))
            {
                Error($"ingredients[{i}]", $"There is no item {ingredient.Group}/{ingredient.Number}.");
            }
            else if (!seen.Add((ingredient.Group, ingredient.Number)))
            {
                Error($"ingredients[{i}]", "This item is there twice.");
            }

            if (ingredient.Minimum > ingredient.Maximum || ingredient.Maximum > 10)
            {
                Error($"ingredients[{i}].maximum", "The minimum at most the maximum, at most 10.");
            }
        }

        if (recipe.BaseChance is < 0 or > 10_000)
        {
            Error("baseChance", "From 0 to 10,000 (hundredths of a percent).");
        }

        if (recipe.MaximumChance < Math.Max(0, recipe.BaseChance) || recipe.MaximumChance > 10_000)
        {
            Error("maximumChance", "From the base chance to 10,000.");
        }

        if (recipe.Catalysts.Count > MaximumCatalysts)
        {
            Error("catalysts", $"At most {MaximumCatalysts} catalysts.");
        }

        for (var i = 0; i < recipe.Catalysts.Count; i++)
        {
            var catalyst = recipe.Catalysts[i];
            if (!itemExists(catalyst.Group, catalyst.Number))
            {
                Error($"catalysts[{i}]", $"There is no item {catalyst.Group}/{catalyst.Number}.");
            }

            if (catalyst.Chance is < 0 or > 10_000)
            {
                Error($"catalysts[{i}].chance", "From 0 to 10,000.");
            }
        }

        if (!Enum.IsDefined(recipe.OnFail))
        {
            Error("onFail", "One of ingredientsLost, itemDowngraded, itemDestroyed.");
        }
        else if (recipe.OnFail == StarSmithOnFail.ItemDestroyed && !recipe.Risky)
        {
            Error("onFail", "Only a risky recipe destroys an item.");
        }

        ValidateShape(recipe.Shape, at, starCount, errors);
        ValidateHint(recipe, at, errors);

        if (recipe.Outcomes.Count is 0 or > MaximumOutcomes)
        {
            Error("outcomes", $"1 to {MaximumOutcomes} outcomes.");
        }

        for (var i = 0; i < recipe.Outcomes.Count; i++)
        {
            ValidateOutcome(recipe.Outcomes[i], $"{at}.outcomes[{i}]", errors);
        }

        if (recipe.Experience is < 0 or > 100_000)
        {
            Error("experience", "From 0 to 100,000.");
        }
    }

    private static void ValidateShape(StarSmithShape? shape, string at, int starCount, List<StarSmithConfigError> errors)
    {
        if (shape is null || shape.Stars.Count is < 2 or > 12)
        {
            errors.Add(new($"{at}.shape.stars", "2 to 12 stars."));
            return;
        }

        var seen = new HashSet<int>();
        for (var i = 0; i < shape.Stars.Count; i++)
        {
            if (shape.Stars[i] < 0 || shape.Stars[i] >= starCount || !seen.Add(shape.Stars[i]))
            {
                errors.Add(new($"{at}.shape.stars[{i}]", $"A star from 0 to {starCount - 1}, each once."));
            }
        }

        if (!Enum.IsDefined(shape.Match))
        {
            errors.Add(new($"{at}.shape.match", "exact or extendable."));
        }

        if (shape.ExtensionPercent is < 0 or > 100)
        {
            errors.Add(new($"{at}.shape.extensionPercent", "From 0 to 100."));
        }

        if (shape.MaximumExtension is < 0 or > 6)
        {
            errors.Add(new($"{at}.shape.maximumExtension", "From 0 to 6."));
        }
    }

    private static void ValidateHint(StarSmithRecipe recipe, string at, List<StarSmithConfigError> errors)
    {
        if (Encoding.UTF8.GetByteCount(recipe.Hint) > MaximumTextBytes)
        {
            errors.Add(new($"{at}.hint.text", $"At most {MaximumTextBytes} bytes."));
        }

        switch (recipe.HintKind)
        {
            case StarSmithHintKind.None:
                break;
            case StarSmithHintKind.Riddle when recipe.Hint.Trim().Length == 0:
                errors.Add(new($"{at}.hint.text", "A riddle needs its text."));
                break;
            case StarSmithHintKind.Riddle:
                break;
            case StarSmithHintKind.Ingredient when !recipe.Ingredients.Any(i => i.Group == recipe.HintGroup && i.Number == recipe.HintNumber):
                errors.Add(new($"{at}.hint.number", "An ingredient hint names one of the recipe's ingredients."));
                break;
            case StarSmithHintKind.Ingredient:
                break;
            default:
                errors.Add(new($"{at}.hint.kind", "none, riddle or ingredient."));
                break;
        }
    }

    private static void ValidateOutcome(StarSmithOutcome outcome, string at, List<StarSmithConfigError> errors)
    {
        if (outcome.Weight is < 1 or > 10_000)
        {
            errors.Add(new($"{at}.weight", "From 1 to 10,000."));
        }

        if (!Enum.IsDefined(outcome.Tier))
        {
            errors.Add(new($"{at}.tier", "common, uncommon, rare, epic, legendary or mythic."));
        }

        if (outcome.Boosts.Count > StarSmithBoosts.Maximum)
        {
            errors.Add(new($"{at}.boosts", $"At most {StarSmithBoosts.Maximum} boosts."));
        }

        var stats = new HashSet<StarSmithBoostStat>();
        for (var i = 0; i < outcome.Boosts.Count; i++)
        {
            var boost = outcome.Boosts[i];
            if (!Enum.IsDefined(boost.Stat) || !stats.Add(boost.Stat))
            {
                errors.Add(new($"{at}.boosts[{i}].stat", "A known stat, each once."));
                continue;
            }

            if (boost.Minimum <= 0)
            {
                errors.Add(new($"{at}.boosts[{i}].min", "More than 0."));
            }

            var ceiling = StarSmithBalance.Ceiling(boost.Stat);
            if (boost.Maximum < boost.Minimum || boost.Maximum > ceiling)
            {
                errors.Add(new($"{at}.boosts[{i}].max", $"From the minimum to {ceiling} (the code ceiling)."));
            }
        }
    }

    private static void ValidateBalance(StarSmithBalance balance, List<StarSmithConfigError> errors)
    {
        if (balance.Tiers.Count != 6)
        {
            errors.Add(new("balance.tiers", "Exactly 6 tiers, Common to Mythic."));
        }
        else
        {
            for (var i = 0; i < balance.Tiers.Count; i++)
            {
                var tier = balance.Tiers[i];
                var at = $"balance.tiers[{i}]";
                if (tier.Name.Trim().Length is 0 or > 16)
                {
                    errors.Add(new($"{at}.name", "1 to 16 characters."));
                }

                if (tier.Color is < 0 or > 0xFFFFFF)
                {
                    errors.Add(new($"{at}.color", "A colour as #rrggbb."));
                }

                if (tier.EffectSlots is < 0 or > 2)
                {
                    errors.Add(new($"{at}.effectSlots", "From 0 to 2."));
                }

                if (tier.BoostSlots is < 0 or > StarSmithBoosts.Maximum)
                {
                    errors.Add(new($"{at}.boostSlots", $"From 0 to {StarSmithBoosts.Maximum}."));
                }
            }
        }

        if (balance.LevelCurve.Base is < 1 or > 100_000)
        {
            errors.Add(new("balance.levelCurve.base", "From 1 to 100,000."));
        }

        if (balance.LevelCurve.Exponent is < 1 or > 3)
        {
            errors.Add(new("balance.levelCurve.exponent", "From 1 to 3."));
        }

        if (balance.Scaling.Base is < 0 or > 1)
        {
            errors.Add(new("balance.scaling.factor", "From 0 to 1."));
        }

        if (balance.Scaling.Exponent is < 0.1 or > 2)
        {
            errors.Add(new("balance.scaling.exponent", "From 0.1 to 2."));
        }

        if (balance.Chance.PerLevel is < 0 or > 100)
        {
            errors.Add(new("balance.chance.perLevel", "From 0 to 100."));
        }

        if (balance.Chance.Maximum is < 0 or > 3000)
        {
            errors.Add(new("balance.chance.maximum", "From 0 to 3,000."));
        }

        ValidateCaps(balance.Pve, "pve", errors);
        ValidateCaps(balance.Pvp, "pvp", errors);
    }

    private static void ValidateCaps(Dictionary<StarSmithBoostStat, StarSmithCap>? caps, string table, List<StarSmithConfigError> errors)
    {
        foreach (var stat in Enum.GetValues<StarSmithBoostStat>())
        {
            var at = $"balance.caps.{table}.{JsonName(stat)}";
            if (caps is null || !caps.TryGetValue(stat, out var cap) || cap is null)
            {
                errors.Add(new(at, "Every stat needs its caps."));
                continue;
            }

            var ceiling = StarSmithBalance.Ceiling(stat);
            if (cap.PerItem < 0 || cap.PerItem > cap.Total)
            {
                errors.Add(new($"{at}.perItem", "From 0 to the total."));
            }

            if (cap.Total > ceiling)
            {
                errors.Add(new($"{at}.total", $"At most {ceiling} (the code ceiling)."));
            }
        }
    }
}
