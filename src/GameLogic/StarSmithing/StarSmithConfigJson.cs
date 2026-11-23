// <copyright file="StarSmithConfigJson.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// The recipes and the balance in the website bridge's camelCase shape (Readme.md "PR 6 contract", "Configuration
/// JSON"): targets as a list, enums as camelCase names, the hint as one object, colours as <c>#rrggbb</c>, the caps
/// under <c>caps.pve</c> and <c>caps.pvp</c>. Reading names each field it cannot read, by the same paths the validator uses.
/// </summary>
public static class StarSmithConfigJson
{
    private static readonly (StarSmithTargetKind Kind, string Name)[] Targets =
    [
        (StarSmithTargetKind.Weapon, "weapon"), (StarSmithTargetKind.Armor, "armor"), (StarSmithTargetKind.Wings, "wings"),
        (StarSmithTargetKind.Pendant, "pendant"), (StarSmithTargetKind.Ring, "ring"),
    ];

    /// <summary>
    /// Writes the recipes and the balance.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <returns><c>{recipes, balance}</c>.</returns>
    public static JsonObject Write(StarSmithingConfiguration config) => new()
    {
        ["recipes"] = new JsonArray(config.Recipes.Select(r => (JsonNode)WriteRecipe(r)).ToArray()),
        ["balance"] = WriteBalance(config.Balance),
    };

    /// <summary>
    /// Reads <c>{recipes?, balance?}</c>; a part left out is null.
    /// </summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The recipes and the balance read, and the fields that could not be read.</returns>
    public static (List<StarSmithRecipe>? Recipes, StarSmithBalance? Balance, IReadOnlyList<StarSmithConfigError> Errors) Read(string json)
    {
        var errors = new List<StarSmithConfigError>();
        JsonObject? root;
        try
        {
            root = JsonNode.Parse(json) as JsonObject;
        }
        catch (JsonException)
        {
            root = null;
        }

        if (root is null)
        {
            errors.Add(new("config", "The configuration must be a JSON object."));
            return (null, null, errors);
        }

        var reader = new Reader(errors);
        List<StarSmithRecipe>? recipes = null;
        if (root["recipes"] is { } recipesNode)
        {
            recipes = recipesNode is JsonArray array
                ? array.Select((node, i) => reader.Recipe(node, $"recipes[{i}]")).ToList()
                : reader.Fail<List<StarSmithRecipe>>("recipes", "A list of recipes.");
        }

        var balance = root["balance"] is { } balanceNode ? reader.Balance(balanceNode, "balance") : null;
        return (recipes, balance, errors);
    }

    /// <summary>
    /// The camelCase name of an enum value.
    /// </summary>
    /// <typeparam name="T">The enum.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The name.</returns>
    public static string Name<T>(T value)
        where T : struct, Enum
    {
        var text = value.ToString();
        return char.ToLowerInvariant(text[0]) + text[1..];
    }

    private static JsonObject WriteRecipe(StarSmithRecipe r) => new()
    {
        ["number"] = (int)r.Number,
        ["name"] = r.Name,
        ["experience"] = r.Experience,
        ["targets"] = new JsonArray(Targets.Where(t => r.Targets.HasFlag(t.Kind)).Select(t => (JsonNode)t.Name).ToArray()),
        ["minimumItemLevel"] = (int)r.MinimumItemLevel,
        ["excellent"] = r.Excellent,
        ["ingredients"] = new JsonArray(r.Ingredients.Select(i => (JsonNode)new JsonObject { ["group"] = (int)i.Group, ["number"] = (int)i.Number, ["minimum"] = (int)i.Minimum, ["maximum"] = (int)i.Maximum }).ToArray()),
        ["baseChance"] = r.BaseChance,
        ["maximumChance"] = r.MaximumChance,
        ["catalysts"] = new JsonArray(r.Catalysts.Select(c => (JsonNode)new JsonObject { ["group"] = (int)c.Group, ["number"] = (int)c.Number, ["chance"] = c.Chance }).ToArray()),
        ["onFail"] = Name(r.OnFail),
        ["risky"] = r.Risky,
        ["shape"] = r.Shape is not { } shape ? null : new JsonObject
        {
            ["stars"] = new JsonArray(shape.Stars.Select(s => (JsonNode)s).ToArray()),
            ["ordered"] = shape.Ordered,
            ["match"] = Name(shape.Match),
            ["extensionPercent"] = shape.ExtensionPercent,
            ["maximumExtension"] = shape.MaximumExtension,
        },
        ["hint"] = new JsonObject { ["kind"] = Name(r.HintKind), ["text"] = r.Hint, ["group"] = (int)r.HintGroup, ["number"] = (int)r.HintNumber },
        ["outcomes"] = new JsonArray(r.Outcomes.Select(o => (JsonNode)new JsonObject
        {
            ["weight"] = o.Weight,
            ["tier"] = Name(o.Tier),
            ["boosts"] = new JsonArray(o.Boosts.Select(b => (JsonNode)new JsonObject { ["stat"] = Name(b.Stat), ["min"] = b.Minimum, ["max"] = b.Maximum }).ToArray()),
        }).ToArray()),
    };

    private static JsonObject WriteBalance(StarSmithBalance b)
    {
        JsonObject Caps(Dictionary<StarSmithBoostStat, StarSmithCap> table)
        {
            var caps = new JsonObject();
            foreach (var (stat, cap) in table.OrderBy(c => c.Key))
            {
                caps[Name(stat)] = new JsonObject { ["perItem"] = cap.PerItem, ["total"] = cap.Total };
            }

            return caps;
        }

        return new JsonObject
        {
            ["tiers"] = new JsonArray(b.Tiers.Select(t => (JsonNode)new JsonObject
            {
                ["name"] = t.Name,
                ["color"] = string.Create(CultureInfo.InvariantCulture, $"#{t.Color & 0xFFFFFF:x6}"),
                ["effectSlots"] = t.EffectSlots,
                ["boostSlots"] = t.BoostSlots,
            }).ToArray()),
            ["levelCurve"] = new JsonObject { ["base"] = b.LevelCurve.Base, ["exponent"] = b.LevelCurve.Exponent },
            ["scaling"] = new JsonObject { ["factor"] = b.Scaling.Base, ["exponent"] = b.Scaling.Exponent },
            ["chance"] = new JsonObject { ["perLevel"] = b.Chance.PerLevel, ["maximum"] = b.Chance.Maximum },
            ["caps"] = new JsonObject { ["pve"] = Caps(b.Pve), ["pvp"] = Caps(b.Pvp) },
        };
    }

    /// <summary>
    /// Reads fields, collecting an error for each one it cannot read and going on with a default.
    /// </summary>
    private sealed class Reader(List<StarSmithConfigError> errors)
    {
        public T Fail<T>(string at, string message)
            where T : new()
        {
            errors.Add(new(at, message));
            return new T();
        }

        public StarSmithRecipe Recipe(JsonNode? node, string at)
        {
            if (node is not JsonObject o)
            {
                return this.Fail<StarSmithRecipe>(at, "A recipe object.");
            }

            var hint = o["hint"] as JsonObject;
            return new StarSmithRecipe
            {
                Number = (short)this.Int(o, "number", at, 0, short.MinValue, short.MaxValue),
                Name = this.Text(o, "name", at),
                Experience = this.Int(o, "experience", at, 0),
                Targets = this.TargetsOf(o["targets"], $"{at}.targets"),
                MinimumItemLevel = (byte)this.Int(o, "minimumItemLevel", at, 0, 0, byte.MaxValue),
                Excellent = this.NullableBool(o, "excellent", at),
                Ingredients = this.List(o["ingredients"], $"{at}.ingredients", (n, p) => n is JsonObject i
                    ? new StarSmithIngredient { Group = (byte)this.Int(i, "group", p, 0, 0, byte.MaxValue), Number = (short)this.Int(i, "number", p, 0, short.MinValue, short.MaxValue), Minimum = (byte)this.Int(i, "minimum", p, 1, 0, byte.MaxValue), Maximum = (byte)this.Int(i, "maximum", p, 1, 0, byte.MaxValue) }
                    : this.Fail<StarSmithIngredient>(p, "An ingredient object.")),
                BaseChance = this.Int(o, "baseChance", at, 0),
                MaximumChance = this.Int(o, "maximumChance", at, 10_000),
                Catalysts = this.List(o["catalysts"], $"{at}.catalysts", (n, p) => n is JsonObject c
                    ? new StarSmithCatalyst { Group = (byte)this.Int(c, "group", p, 0, 0, byte.MaxValue), Number = (short)this.Int(c, "number", p, 0, short.MinValue, short.MaxValue), Chance = this.Int(c, "chance", p, 0) }
                    : this.Fail<StarSmithCatalyst>(p, "A catalyst object.")),
                OnFail = this.Choice(o, "onFail", at, StarSmithOnFail.IngredientsLost),
                Risky = this.Bool(o, "risky", at),
                Shape = o["shape"] is JsonObject s ? new StarSmithShape
                {
                    Stars = this.List(s["stars"], $"{at}.shape.stars", (n, p) => this.IntOf(n, p, 0)),
                    Ordered = s["ordered"] is null || this.Bool(s, "ordered", $"{at}.shape"),
                    Match = this.Choice(s, "match", $"{at}.shape", StarSmithShapeMatch.Exact),
                    ExtensionPercent = this.Int(s, "extensionPercent", $"{at}.shape", 10),
                    MaximumExtension = this.Int(s, "maximumExtension", $"{at}.shape", 3),
                } : null,
                HintKind = hint is null ? StarSmithHintKind.None : this.Choice(hint, "kind", $"{at}.hint", StarSmithHintKind.None),
                Hint = hint is null ? string.Empty : this.Text(hint, "text", $"{at}.hint"),
                HintGroup = hint is null ? (byte)0 : (byte)this.Int(hint, "group", $"{at}.hint", 0, 0, byte.MaxValue),
                HintNumber = hint is null ? (short)0 : (short)this.Int(hint, "number", $"{at}.hint", 0, short.MinValue, short.MaxValue),
                Outcomes = this.List(o["outcomes"], $"{at}.outcomes", (n, p) => n is JsonObject outcome
                    ? new StarSmithOutcome
                    {
                        Weight = this.Int(outcome, "weight", p, 1),
                        Tier = this.Choice(outcome, "tier", p, StarSmithRarity.Common),
                        Boosts = this.List(outcome["boosts"], $"{p}.boosts", (b, bp) => b is JsonObject boost
                            ? new StarSmithBoostRange { Stat = this.Choice(boost, "stat", bp, StarSmithBoostStat.Damage), Minimum = this.Number(boost, "min", bp, 0), Maximum = this.Number(boost, "max", bp, 0) }
                            : this.Fail<StarSmithBoostRange>(bp, "A boost object.")),
                    }
                    : this.Fail<StarSmithOutcome>(p, "An outcome object.")),
            };
        }

        public StarSmithBalance Balance(JsonNode node, string at)
        {
            var balance = new StarSmithBalance();
            if (node is not JsonObject o)
            {
                return this.Fail<StarSmithBalance>(at, "A balance object.");
            }

            balance.Tiers = this.List(o["tiers"], $"{at}.tiers", (n, p) => n is JsonObject t
                ? new StarSmithTier { Name = this.Text(t, "name", p), Color = this.Color(t, "color", p), EffectSlots = this.Int(t, "effectSlots", p, 0), BoostSlots = this.Int(t, "boostSlots", p, 0) }
                : this.Fail<StarSmithTier>(p, "A tier object."));
            if (o["levelCurve"] is JsonObject curve)
            {
                balance.LevelCurve = new StarSmithCurve { Base = this.Number(curve, "base", $"{at}.levelCurve", 100), Exponent = this.Number(curve, "exponent", $"{at}.levelCurve", 1.6) };
            }

            if (o["scaling"] is JsonObject scaling)
            {
                balance.Scaling = new StarSmithCurve { Base = this.Number(scaling, "factor", $"{at}.scaling", 0.5), Exponent = this.Number(scaling, "exponent", $"{at}.scaling", 0.8) };
            }

            if (o["chance"] is JsonObject chance)
            {
                balance.Chance = new StarSmithChanceBonus { PerLevel = this.Int(chance, "perLevel", $"{at}.chance", StarSmithRecipes.LevelBonus), Maximum = this.Int(chance, "maximum", $"{at}.chance", StarSmithRecipes.MaximumLevelBonus) };
            }

            if (o["caps"] is JsonObject caps)
            {
                balance.Pve = this.Caps(caps["pve"], $"{at}.caps.pve");
                balance.Pvp = this.Caps(caps["pvp"], $"{at}.caps.pvp");
            }

            return balance;
        }

        public int IntOf(JsonNode? node, string at, int fallback, int minimum = int.MinValue, int maximum = int.MaxValue)
        {
            if (node is JsonValue value && value.TryGetValue<double>(out var number) && number == Math.Floor(number) && number >= minimum && number <= maximum)
            {
                return (int)number;
            }

            errors.Add(new(at, minimum == int.MinValue ? "A whole number." : $"A whole number from {minimum} to {maximum}."));
            return fallback;
        }

        private Dictionary<StarSmithBoostStat, StarSmithCap> Caps(JsonNode? node, string at)
        {
            var caps = new Dictionary<StarSmithBoostStat, StarSmithCap>();
            if (node is not JsonObject o)
            {
                errors.Add(new(at, "The caps by stat."));
                return caps;
            }

            foreach (var (name, value) in o)
            {
                if (!System.Enum.GetValues<StarSmithBoostStat>().Any(s => Name(s) == name))
                {
                    errors.Add(new($"{at}.{name}", "Not a stat."));
                }
                else if (value is JsonObject cap)
                {
                    caps[System.Enum.GetValues<StarSmithBoostStat>().First(s => Name(s) == name)] = new StarSmithCap(this.Number(cap, "perItem", $"{at}.{name}", 0), this.Number(cap, "total", $"{at}.{name}", 0));
                }
                else
                {
                    errors.Add(new($"{at}.{name}", "{perItem, total}."));
                }
            }

            return caps;
        }

        private List<T> List<T>(JsonNode? node, string at, Func<JsonNode?, string, T> read)
        {
            if (node is null)
            {
                return [];
            }

            if (node is not JsonArray array)
            {
                errors.Add(new(at, "A list."));
                return [];
            }

            return array.Select((item, i) => read(item, $"{at}[{i}]")).ToList();
        }

        private int Int(JsonObject o, string name, string at, int fallback, int minimum = int.MinValue, int maximum = int.MaxValue)
            => o[name] is null ? fallback : this.IntOf(o[name], $"{at}.{name}", fallback, minimum, maximum);

        private double Number(JsonObject o, string name, string at, double fallback)
        {
            if (o[name] is null)
            {
                return fallback;
            }

            if (o[name] is JsonValue value && value.TryGetValue<double>(out var number) && double.IsFinite(number))
            {
                return Math.Round(number, 2);
            }

            errors.Add(new($"{at}.{name}", "A number."));
            return fallback;
        }

        private bool Bool(JsonObject o, string name, string at)
        {
            if (o[name] is null)
            {
                return false;
            }

            if (o[name] is JsonValue value && value.TryGetValue<bool>(out var flag))
            {
                return flag;
            }

            errors.Add(new($"{at}.{name}", "true or false."));
            return false;
        }

        private bool? NullableBool(JsonObject o, string name, string at) => o[name] is null ? null : this.Bool(o, name, at);

        private string Text(JsonObject o, string name, string at)
        {
            if (o[name] is null)
            {
                return string.Empty;
            }

            if (o[name] is JsonValue value && value.TryGetValue<string>(out var text))
            {
                return text.Trim();
            }

            errors.Add(new($"{at}.{name}", "A text."));
            return string.Empty;
        }

        private int Color(JsonObject o, string name, string at)
        {
            var text = this.Text(o, name, at).TrimStart('#');
            if (text.Length == 6 && int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var color))
            {
                return color;
            }

            errors.Add(new($"{at}.{name}", "A colour as #rrggbb."));
            return 0;
        }

        private T Choice<T>(JsonObject o, string name, string at, T fallback)
            where T : struct, Enum
        {
            var text = this.Text(o, name, at);
            foreach (var value in System.Enum.GetValues<T>())
            {
                if (string.Equals(Name(value), text, StringComparison.Ordinal))
                {
                    return value;
                }
            }

            errors.Add(new($"{at}.{name}", $"One of {string.Join(", ", System.Enum.GetValues<T>().Select(v => Name(v)))}."));
            return fallback;
        }

        private StarSmithTargetKind TargetsOf(JsonNode? node, string at)
        {
            var kinds = StarSmithTargetKind.None;
            foreach (var (item, i) in (node as JsonArray ?? new JsonArray()).Select((item, i) => (item, i)))
            {
                var name = item is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
                var target = Targets.FirstOrDefault(t => t.Name == name);
                if (target.Name is null)
                {
                    errors.Add(new($"{at}[{i}]", "weapon, armor, wings, pendant or ring."));
                }

                kinds |= target.Kind;
            }

            return kinds;
        }
    }
}
