// <copyright file="SiteFilter.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence;

/// <summary>
/// Which objects of a type a list or a bulk edit takes: a text search over captions and text fields, conditions
/// (<c>[{field, op, value}]</c>, all of them), and an owner (<c>{type, ids, collection}</c>: the object's nearest owner).
/// </summary>
internal sealed class SiteFilter
{
    private static readonly string[] Operators = ["eq", "ne", "gt", "ge", "lt", "le", "contains", "in"];

    private readonly List<Func<object, bool>> _tests = [];

    private SiteFilter(OwnerFilter? owner)
    {
        this.Owner = owner;
    }

    /// <summary>Gets the owner filter, or null.</summary>
    public OwnerFilter? Owner { get; }

    /// <summary>Reads a filter.</summary>
    /// <param name="type">The data model type.</param>
    /// <param name="text">The search text, or null.</param>
    /// <param name="where">The conditions as JSON, or null.</param>
    /// <param name="owner">The owner as JSON, or null.</param>
    /// <returns>The filter.</returns>
    public static SiteFilter Parse(Type type, string? text, string? where, string? owner)
    {
        var filter = new SiteFilter(owner is { Length: > 0 } ? OwnerFilter.Parse(owner) : null);
        if (!string.IsNullOrWhiteSpace(text))
        {
            var search = text.Trim();
            var texts = SiteSchema.Fields(type).Where(f => f.Kind == "scalar" && (f.Target == typeof(string) || f.Target == typeof(LocalizedString))).ToArray();
            var number = SiteSchema.Fields(type).FirstOrDefault(f => f.Name == "Number" && f.IsNumber);
            filter._tests.Add(item => SiteSchema.Caption(item).Contains(search, StringComparison.OrdinalIgnoreCase)
                || texts.Any(f => f.Value(item)?.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) == true)
                || (number is not null && number.Value(item)?.ToString() == search));
        }

        if (string.IsNullOrWhiteSpace(where))
        {
            return filter;
        }

        if (Json(where) is not JsonArray conditions || conditions.Count > 10)
        {
            throw Invalid("Conditions are a JSON array of at most 10 {field, op, value}.");
        }

        foreach (var condition in conditions.OfType<JsonObject>())
        {
            filter._tests.Add(Condition(type, condition));
        }

        return filter;
    }

    /// <summary>Whether an object passes.</summary>
    /// <param name="item">The object.</param>
    /// <param name="owners">The owners, when the filter names one.</param>
    /// <returns>Whether it passes.</returns>
    public bool Passes(object item, ConfigurationOwners? owners)
        => (this.Owner is null || (owners?.OwnerOf(item) is { } owner && this.Owner.Matches(owner.Owner, owner.Property.Name))) && this._tests.All(t => t(item));

    private static Func<object, bool> Condition(Type type, JsonObject condition)
    {
        var name = condition["field"]?.GetValue<string>();
        var op = condition["op"]?.GetValue<string>() ?? "eq";
        var field = SiteSchema.Fields(type).FirstOrDefault(f => f.Name == name && f.Kind is "scalar" or "enum" or "reference")
            ?? throw Invalid($"Filter on a scalar, enum or reference field of {type.Name}; {name} is none.");
        if (!Operators.Contains(op))
        {
            throw Invalid("A condition's op is one of eq, ne, gt, ge, lt, le, contains, in.");
        }

        var expected = op == "in" ? (condition["value"] as JsonArray ?? throw Invalid("The in operator takes an array.")).Select(v => Text(v)).ToHashSet(StringComparer.OrdinalIgnoreCase) : null;
        var value = Text(condition["value"]);
        var number = field.IsNumber && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : (double?)null;
        return item =>
        {
            var actual = field.Value(item);
            var text = field.Kind == "reference" ? actual?["$ref"]?.GetValue<string>() : Text(actual);
            if (expected is not null)
            {
                return text is not null && expected.Contains(text);
            }

            if (op == "contains")
            {
                return text?.Contains(value ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true;
            }

            var comparison = number is { } want && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var have)
                ? have.CompareTo(want)
                : string.Compare(text, value, StringComparison.OrdinalIgnoreCase);
            return op switch { "eq" => comparison == 0, "ne" => comparison != 0, "gt" => comparison > 0, "ge" => comparison >= 0, "lt" => comparison < 0, _ => comparison <= 0 };
        };
    }

    private static string? Text(JsonNode? node) => node switch
    {
        null => null,
        JsonValue value when value.TryGetValue<string>(out var text) => text,
        _ => node.ToJsonString(),
    };

    private static JsonNode? Json(string text)
    {
        try
        {
            return JsonNode.Parse(text, documentOptions: new JsonDocumentOptions { MaxDepth = 8 });
        }
        catch (JsonException)
        {
            throw Invalid("The filter is not valid JSON.");
        }
    }

    private static AdminOperationException Invalid(string message) => new(message, AdminOperationErrorKind.InvalidInput);

    /// <summary>The objects owned by one of some objects of a type (in one of its collections, when named).</summary>
    /// <param name="Type">The owner's type name.</param>
    /// <param name="Ids">The owners' identifiers.</param>
    /// <param name="Collection">The collection, or null for any.</param>
    internal sealed record OwnerFilter(string Type, HashSet<Guid> Ids, string? Collection)
    {
        /// <summary>Reads an owner filter.</summary>
        /// <param name="json">The JSON.</param>
        /// <returns>The filter.</returns>
        public static OwnerFilter Parse(string json)
        {
            if (Json(json) is not JsonObject owner || owner["type"]?.GetValue<string>() is not { } type || owner["ids"] is not JsonArray ids || ids.Count is 0 or > 500)
            {
                throw Invalid("The owner is {type, ids: [1-500 identifiers], collection}.");
            }

            var parsed = ids.Select(i => Guid.TryParse(Text(i), out var id) ? id : throw Invalid("An owner identifier is not a GUID.")).ToHashSet();
            return new OwnerFilter(type, parsed, owner["collection"]?.GetValue<string>());
        }

        /// <summary>Whether an owner and its field match.</summary>
        /// <param name="owner">The owner.</param>
        /// <param name="collection">The field the object is in.</param>
        /// <returns>Whether they match.</returns>
        public bool Matches(object owner, string collection)
            => SiteTypes.ModelType(owner)?.Name == this.Type && this.Ids.Contains(owner.GetId()) && (this.Collection is null || this.Collection == collection);
    }
}
