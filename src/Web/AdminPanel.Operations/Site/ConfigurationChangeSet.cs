// <copyright file="ConfigurationChangeSet.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.Persistence;

/// <summary>
/// One reviewed change of the configuration (Readme.md, "Changesets"): rows that update, create or delete objects, each
/// checked against the current state (its version or expected values), described before and after for the preview and
/// the audit, applied in one context, and announced to the game: each touched object and the objects that own it.
/// </summary>
internal sealed class ConfigurationChangeSet
{
    /// <summary>The most rows one changeset carries.</summary>
    public const int MaximumRows = 200;

    private readonly IContext _context;

    private readonly GameConfiguration _config;

    private readonly List<object> _roots;

    private readonly List<Row> _rows = [];

    private ConfigurationOwners? _owners;

    private ConfigurationChangeSet(IContext context, GameConfiguration config, List<object> roots)
    {
        this._context = context;
        this._config = config;
        this._roots = roots;
    }

    /// <summary>Gets the number of rows that change something.</summary>
    public int Count => this._rows.Count;

    /// <summary>Gets the change for the preview and the audit: <c>{kind, rows: [{o, t, i, p, pi, c, b, a}]}</c>.</summary>
    public JsonObject Change => new() { ["kind"] = "changeset", ["rows"] = new JsonArray(this._rows.Select(r => (JsonNode)r.Compact).ToArray()) };

    /// <summary>Gets when each row applies (live, next, restart), and the latest of them.</summary>
    public JsonObject Applies => new()
    {
        ["all"] = this._rows.Select(r => r.Applies).Aggregate(ConfigurationApplies.Live, ConfigurationApplies.Latest),
        ["rows"] = new JsonArray(this._rows.Select(r => (JsonNode)r.Applies).ToArray()),
    };

    private ConfigurationOwners Owners => this._owners ??= ConfigurationOwners.Of(this._roots);

    /// <summary>Checks every row against the current state.</summary>
    /// <param name="context">The context the change is saved in.</param>
    /// <param name="config">The configuration loaded in it.</param>
    /// <param name="rows">The rows.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The changeset; rows that would change nothing are left out.</returns>
    public static async ValueTask<ConfigurationChangeSet> PlanAsync(IContext context, GameConfiguration config, JsonArray rows, CancellationToken cancellationToken)
    {
        if (rows.Count is 0 or > MaximumRows)
        {
            throw Invalid($"A changeset has 1-{MaximumRows} rows.");
        }

        var set = new ConfigurationChangeSet(context, config, await SiteTypes.RootsAsync(config, context, cancellationToken).ConfigureAwait(false));
        for (var at = 0; at < rows.Count; at++)
        {
            if (rows[at] is not JsonObject row)
            {
                throw Invalid($"Row {at} is not an object.");
            }

            var planned = Text(row, "op") switch
            {
                "update" => await set.PlanUpdateAsync(at, row, cancellationToken).ConfigureAwait(false),
                "create" => await set.PlanCreateAsync(at, row, cancellationToken).ConfigureAwait(false),
                "delete" => await set.PlanDeleteAsync(at, row, cancellationToken).ConfigureAwait(false),
                _ => throw Invalid($"Row {at}: op is update, create or delete."),
            };
            if (planned is not null)
            {
                set._rows.Add(planned);
            }
        }

        return set;
    }

    /// <summary>Applies every row to the loaded objects (saved by the caller).</summary>
    /// <returns>The task.</returns>
    public async ValueTask ApplyAsync()
    {
        foreach (var row in this._rows)
        {
            switch (row.Op)
            {
                case "update":
                    ConfigurationPatch.Apply(row.Item!, row.Values);
                    break;
                case "create":
                    var parent = row.ParentRow?.Item ?? row.Parent!;
                    var created = row.Id == Guid.Empty ? this._context.CreateNew(row.Type) : this.CreateWithId(row.Type, row.Id);
                    ConfigurationPatch.Apply(created, row.Values);
                    row.Collection!.PropertyType.GetMethod("Add")!.Invoke(row.Collection.GetValue(parent), [created]);
                    row.Item = created;
                    row.Compact["i"] = created.GetId().ToString();
                    break;
                default:
                    row.Collection!.PropertyType.GetMethod("Remove")!.Invoke(row.Collection.GetValue(row.Parent), [row.Item]);
                    var deletion = (ValueTask<bool>)typeof(IContext).GetMethod(nameof(IContext.DeleteAsync))!.MakeGenericMethod(row.Type).Invoke(this._context, [row.Item])!;
                    if (!await deletion.ConfigureAwait(false))
                    {
                        throw new AdminOperationException($"Row {row.At}: the object could not be deleted.");
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Tells the game about the saved change, as its own admin panel's saves do through the persistence: each touched
    /// object (when the save did not, see <see cref="SiteBridgeOptions"/>) and every object that owns one, the game
    /// configuration itself aside unless it was changed.
    /// </summary>
    /// <param name="changes">The game's change listener.</param>
    /// <param name="savesPublish">Whether the save announced the touched objects already.</param>
    /// <returns>The task.</returns>
    public async ValueTask NotifyAsync(IConfigurationChangeMediatorListener changes, bool savesPublish)
    {
        var told = new HashSet<Guid>();
        foreach (var row in this._rows)
        {
            var item = row.Item!;
            var id = item.GetId();
            if (!savesPublish)
            {
                switch (row.Op)
                {
                    case "update":
                        told.Add(id);
                        await changes.HandleConfigurationChangedAsync(row.Type, id, item).ConfigureAwait(false);
                        break;
                    case "create":
                        await changes.HandleConfigurationAddedAsync(row.Type, id, item).ConfigureAwait(false);
                        break;
                    default:
                        await changes.HandleConfigurationRemovedAsync(row.Type, id).ConfigureAwait(false);
                        break;
                }
            }

            var start = row.Op == "update" ? item : row.ParentRow?.Item ?? row.Parent!;
            var chain = row.Op == "update" ? this.Owners.Chain(start) : new[] { start }.Concat(this.Owners.Chain(start));
            foreach (var owner in chain.Where(o => o is not GameConfiguration))
            {
                if (told.Add(owner.GetId()) && SiteTypes.ModelType(owner) is { } ownerType)
                {
                    await changes.HandleConfigurationChangedAsync(ownerType, owner.GetId(), owner).ConfigureAwait(false);
                }
            }
        }
    }

    private static AdminOperationException Invalid(string message) => new(message, AdminOperationErrorKind.InvalidInput);

    private static string? Text(JsonObject row, string name) => row[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static Guid Id(JsonObject row, string name, int at)
        => Guid.TryParse(Text(row, name), out var id) ? id : throw Invalid($"Row {at}: {name} is not a GUID.");

    private static void Check(int at, object item, Type type, JsonObject row)
    {
        var snapshot = ConfigurationPatch.Snapshot(item, type);
        var version = SiteSchema.Version(item, type);
        if ((Text(row, "version") is { } expected && expected != version)
            || (row["expect"] is JsonObject expect && expect.Any(pair => !JsonNode.DeepEquals(snapshot[pair.Key], pair.Value))))
        {
            throw new ConfigurationStaleException(at, version, snapshot);
        }
    }

    private static Dictionary<string, JsonNode?> Changed(JsonObject before, JsonObject after)
        => after.Where(pair => !JsonNode.DeepEquals(before[pair.Key], pair.Value)).ToDictionary(pair => pair.Key, pair => pair.Value);

    private object CreateWithId(Type type, Guid id)
    {
        try
        {
            return this._context.CreateNewWithId(type, id);
        }
        catch (NotSupportedException)
        {
            return this._context.CreateNew(type);
        }
    }

    private async ValueTask<Dictionary<PropertyInfo, object?>> ValuesAsync(int at, Type type, JsonObject? fields, CancellationToken cancellationToken)
    {
        if (fields is null)
        {
            throw Invalid($"Row {at}: fields is an object.");
        }

        Dictionary<PropertyInfo, object?> values;
        try
        {
            values = await ConfigurationPatch.ConvertAsync(type, fields, this._context, cancellationToken).ConfigureAwait(false);
            ConfigurationRules.ValidateReferences(this._config, values);
        }
        catch (AdminOperationException ex)
        {
            throw new AdminOperationException($"Row {at}: {ex.Message}", ex.Kind, ex);
        }

        foreach (var (property, value) in values)
        {
            if (SiteSchema.Fields(type).FirstOrDefault(f => f.Name == property.Name) is { IsNumber: true } field && value is not null)
            {
                var number = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                var (min, max) = field.Bounds;
                if (number < min || number > max)
                {
                    throw Invalid($"Row {at}: {property.Name} is {number}; it must be between {min?.ToString() ?? "-"} and {max?.ToString() ?? "-"}.");
                }
            }
        }

        return values;
    }

    private JsonObject After(object? item, Type type, Dictionary<PropertyInfo, object?> values, JsonObject? before)
    {
        JsonObject after;
        try
        {
            after = ConfigurationPatch.Snapshot(ConfigurationRules.Candidate(item, type, values), type);
        }
        catch (AdminOperationException ex)
        {
            throw new AdminOperationException(ex.Message, ex.Kind, ex);
        }

        foreach (var property in ConfigurationPatch.Fields(type).Where(ConfigurationPatch.IsReferenceCollection))
        {
            after[property.Name] = values.TryGetValue(property, out var proposed)
                ? new JsonArray(((List<object>)proposed!).OrderBy(v => v.GetId()).Select(v => (JsonNode)new JsonObject { ["$ref"] = v.GetId().ToString() }).ToArray())
                : before?[property.Name]?.DeepClone() ?? new JsonArray();
        }

        return after;
    }

    private async ValueTask<Row?> PlanUpdateAsync(int at, JsonObject row, CancellationToken cancellationToken)
    {
        var type = SiteTypes.Resolve(Text(row, "type") ?? string.Empty);
        var id = Id(row, "id", at);
        var item = await SiteTypes.FindAsync(type, id, this._config, this._context, cancellationToken).ConfigureAwait(false)
            ?? throw Invalid($"Row {at}: there is no {type.Name} {id}.");
        Check(at, item, type, row);
        var values = await this.ValuesAsync(at, type, row["fields"] as JsonObject, cancellationToken).ConfigureAwait(false);
        var snapshot = ConfigurationPatch.Snapshot(item, type);
        var before = ConfigurationRules.SelectFields(snapshot, values.Keys);
        var after = ConfigurationRules.SelectFields(this.After(item, type, values, snapshot), values.Keys);
        var changed = Changed(before, after);
        if (changed.Count == 0)
        {
            return null;
        }

        foreach (var unchanged in values.Keys.Where(p => !changed.ContainsKey(p.Name)).ToList())
        {
            values.Remove(unchanged);
        }

        var compact = new JsonObject
        {
            ["o"] = "u", ["t"] = type.Name, ["i"] = id.ToString(),
            ["b"] = ConfigurationRules.SelectFields(snapshot, values.Keys),
            ["a"] = new JsonObject(changed.Select(p => KeyValuePair.Create(p.Key, p.Value?.DeepClone()))),
        };
        var applies = values.Keys.Select(p => ConfigurationApplies.Of(type, p.Name)).Aggregate(ConfigurationApplies.Live, ConfigurationApplies.Latest);
        return new Row(at, "update", type, id, item, null, null, values, compact, applies);
    }

    private async ValueTask<Row?> PlanCreateAsync(int at, JsonObject row, CancellationToken cancellationToken)
    {
        var parentType = SiteTypes.Resolve(Text(row, "parentType") ?? string.Empty);
        var parentId = Id(row, "parentId", at);
        var parentRow = this._rows.FirstOrDefault(r => r.Op == "create" && r.Id == parentId && r.Type == parentType);
        var parent = parentRow is null ? await SiteTypes.FindAsync(parentType, parentId, this._config, this._context, cancellationToken).ConfigureAwait(false) : null;
        if (parent is null && parentRow is null)
        {
            throw Invalid($"Row {at}: there is no {parentType.Name} {parentId} to add to.");
        }

        var (collection, type) = ConfigurationRules.Collection(parentType, Text(row, "collection") ?? string.Empty);
        if (Text(row, "type") is { } named && named != type.Name)
        {
            throw Invalid($"Row {at}: {parentType.Name}.{collection.Name} holds {type.Name}, not {named}.");
        }

        var id = Text(row, "id") is null ? Guid.Empty : Id(row, "id", at);
        if (id != Guid.Empty && (await SiteTypes.FindAsync(type, id, this._config, this._context, cancellationToken).ConfigureAwait(false) is not null || this._rows.Any(r => r.Id == id)))
        {
            throw new AdminOperationException($"Row {at}: a {type.Name} {id} exists already.", AdminOperationErrorKind.Conflict);
        }

        var values = await this.ValuesAsync(at, type, row["fields"] as JsonObject, cancellationToken).ConfigureAwait(false);
        var compact = new JsonObject
        {
            ["o"] = "c", ["t"] = type.Name, ["i"] = id == Guid.Empty ? string.Empty : id.ToString(),
            ["p"] = parentType.Name, ["pi"] = parentId.ToString(), ["c"] = collection.Name,
            ["a"] = this.After(null, type, values, null),
        };
        return new Row(at, "create", type, id, null, parent, collection, values, compact, ConfigurationApplies.Latest(ConfigurationApplies.Of(type), ConfigurationApplies.Of(parentType))) { ParentRow = parentRow };
    }

    private async ValueTask<Row?> PlanDeleteAsync(int at, JsonObject row, CancellationToken cancellationToken)
    {
        var type = SiteTypes.Resolve(Text(row, "type") ?? string.Empty);
        var id = Id(row, "id", at);
        var item = await SiteTypes.FindAsync(type, id, this._config, this._context, cancellationToken).ConfigureAwait(false)
            ?? throw Invalid($"Row {at}: there is no {type.Name} {id}.");
        Check(at, item, type, row);
        if (this.Owners.OwnerOf(item) is not { } owner || SiteTypes.ModelType(owner.Owner) is not { } ownerType)
        {
            throw Invalid($"Row {at}: a {type.Name} that nothing owns cannot be deleted here.");
        }

        var (collection, _) = ConfigurationRules.Collection(ownerType, owner.Property.Name);
        try
        {
            ConfigurationRules.CheckDeletion(this._config, item, owner.Owner, collection, type);
        }
        catch (AdminOperationException ex)
        {
            throw new AdminOperationException($"Row {at}: {ex.Message}", ex.Kind, ex);
        }

        var compact = new JsonObject
        {
            ["o"] = "d", ["t"] = type.Name, ["i"] = id.ToString(),
            ["p"] = ownerType.Name, ["pi"] = owner.Owner.GetId().ToString(), ["c"] = collection.Name,
            ["b"] = ConfigurationPatch.Snapshot(item, type),
        };
        return new Row(at, "delete", type, id, item, owner.Owner, collection, [], compact, ConfigurationApplies.Latest(ConfigurationApplies.Of(type), ConfigurationApplies.Of(ownerType)));
    }

    /// <summary>One planned row.</summary>
    /// <param name="At">Its index in the request.</param>
    /// <param name="Op">update, create or delete.</param>
    /// <param name="Type">The object's type.</param>
    /// <param name="Id">The object's identifier (empty for a create that gets a new one).</param>
    /// <param name="Item">The object (the created one, once applied).</param>
    /// <param name="Parent">The owner (create, delete).</param>
    /// <param name="Collection">The owner's collection (create, delete).</param>
    /// <param name="Values">The values to set.</param>
    /// <param name="Compact">The row as the preview and audit show it.</param>
    /// <param name="Applies">When it applies.</param>
    private sealed record Row(int At, string Op, Type Type, Guid Id, object? Item, object? Parent, PropertyInfo? Collection, Dictionary<PropertyInfo, object?> Values, JsonObject Compact, string Applies)
    {
        /// <summary>Gets or sets the object (the created one, once applied).</summary>
        public object? Item { get; set; } = Item;

        /// <summary>Gets the row that creates the parent, when the parent is new in this changeset.</summary>
        public Row? ParentRow { get; init; }
    }
}
