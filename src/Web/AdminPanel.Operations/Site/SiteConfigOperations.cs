// <copyright file="SiteConfigOperations.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.AdminAuth;

/// <summary>
/// The website's configuration editor (Readme.md): the whole schema in one call, paged and filtered lists, versioned
/// reads, and every write as a changeset (rows, bulk edits, imports, reverts) that is previewed with a token, refused
/// when anything it was based on changed, audited in one row and announced to the running game.
/// </summary>
/// <param name="persistence">The persistence.</param>
/// <param name="configurations">The active configuration.</param>
/// <param name="executor">The audit and role executor.</param>
/// <param name="confirmations">The preview tokens.</param>
/// <param name="settings">The write settings.</param>
/// <param name="audit">The audit.</param>
/// <param name="changes">The game's configuration change listener, when a game runs in this process.</param>
/// <param name="options">How saves are announced; null for a database (its saves announce the changed objects).</param>
public sealed class SiteConfigOperations(IPersistenceContextProvider persistence, IDataSource<GameConfiguration> configurations, AdminOperationExecutor executor, AdminChangeConfirmation confirmations, AdminWriteSettings settings, IAdminAuditRepository audit, IConfigurationChangeMediatorListener? changes = null, SiteBridgeOptions? options = null)
{
    /// <summary>The longest change one audit row keeps whole; larger edits are split by the site.</summary>
    public const int MaximumChange = 15000;

    private static readonly string[] Changesets = ["config_changeset", "config_bulk", "config_import", "config_revert"];

    /// <summary>Describes every type the site edits.</summary>
    /// <returns>The types with their fields.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Viewer)]
    public JsonObject Schema() => new()
    {
        ["format"] = 1,
        ["maximumRows"] = ConfigurationChangeSet.MaximumRows,
        ["types"] = new JsonArray(SiteTypes.All.Select(t => (JsonNode)SiteSchema.Of(t)).ToArray()),
    };

    /// <summary>Lists objects of a type: a page, searched, filtered and sorted.</summary>
    /// <param name="type">The type.</param>
    /// <param name="text">A search over captions, text fields and the number.</param>
    /// <param name="where">Conditions: <c>[{field, op, value}]</c>.</param>
    /// <param name="owner">The nearest owner: <c>{type, ids, collection}</c>.</param>
    /// <param name="sort">A field to sort by, <c>-</c> before it for descending; the caption by default.</param>
    /// <param name="fields">Comma separated fields to carry; all by default.</param>
    /// <param name="offset">The rows to skip.</param>
    /// <param name="limit">The rows to return, 1-100.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Viewer)]
    public async ValueTask<JsonObject> ListAsync(string type, string? text, string? where, string? owner, string? sort, string? fields, int offset = 0, int limit = 50, CancellationToken cancellationToken = default)
    {
        AdminReadResult.ValidatePage(offset, limit);
        var modelType = SiteTypes.Resolve(type);
        var selected = string.IsNullOrWhiteSpace(fields) ? null : fields.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);
        using var context = persistence.CreateNewContext();
        var config = await this.LoadAsync(context, cancellationToken).ConfigureAwait(false);
        var matches = await MatchingAsync(modelType, SiteFilter.Parse(modelType, text, where, owner), config, context, cancellationToken).ConfigureAwait(false);
        var sorted = Sort(matches, modelType, sort);
        return new JsonObject
        {
            ["type"] = modelType.Name,
            ["total"] = sorted.Count,
            ["offset"] = offset,
            ["limit"] = limit,
            ["items"] = new JsonArray(sorted.Skip(offset).Take(limit).Select(o => (JsonNode)SiteSchema.Row(o, modelType, selected)).ToArray()),
        };
    }

    /// <summary>Reads one object: its values and version, its owners and its children's counts.</summary>
    /// <param name="type">The type.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The object.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Viewer)]
    public async ValueTask<JsonObject> ReadAsync(string type, Guid id, CancellationToken cancellationToken)
    {
        var modelType = SiteTypes.Resolve(type);
        using var context = persistence.CreateNewContext();
        var config = await this.LoadAsync(context, cancellationToken).ConfigureAwait(false);
        var item = await SiteTypes.FindAsync(modelType, id, config, context, cancellationToken).ConfigureAwait(false)
            ?? throw new AdminResourceNotFoundException($"There is no {modelType.Name} {id}.", nameof(id));
        var owners = ConfigurationOwners.Of(await SiteTypes.RootsAsync(config, context, cancellationToken).ConfigureAwait(false));
        var path = new JsonArray();
        var at = item;
        while (owners.OwnerOf(at) is { } owner)
        {
            path.Insert(0, new JsonObject { ["type"] = SiteTypes.ModelType(owner.Owner)?.Name, ["id"] = owner.Owner.GetId().ToString(), ["caption"] = SiteSchema.Caption(owner.Owner), ["collection"] = owner.Property.Name });
            at = owner.Owner;
        }

        return new JsonObject { ["type"] = modelType.Name, ["item"] = SiteSchema.Row(item, modelType), ["owners"] = path };
    }

    /// <summary>Finds objects of every type by their caption (or number), best matches first, each with its owner.</summary>
    /// <param name="text">The text, at least two characters.</param>
    /// <param name="limit">The most hits, 1-100.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The hits: <c>{type, id, caption, owner}</c>.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Viewer)]
    public async ValueTask<JsonObject> FindAsync(string text, int limit = 50, CancellationToken cancellationToken = default)
    {
        var search = text.Trim();
        if (search.Length < 2)
        {
            throw new AdminOperationException("Search for at least two characters.", AdminOperationErrorKind.InvalidInput);
        }

        AdminReadResult.ValidatePage(0, limit);
        using var context = persistence.CreateNewContext();
        var config = await this.LoadAsync(context, cancellationToken).ConfigureAwait(false);
        var owners = ConfigurationOwners.Of(await SiteTypes.RootsAsync(config, context, cancellationToken).ConfigureAwait(false));
        var hits = new List<(int Score, string Caption, object Item, Type Type)>();
        foreach (var item in owners.All)
        {
            if (SiteTypes.ModelType(item) is not { } type)
            {
                continue;
            }

            var caption = SiteSchema.Caption(item);
            var score = caption.Equals(search, StringComparison.OrdinalIgnoreCase) ? 0
                : caption.StartsWith(search, StringComparison.OrdinalIgnoreCase) ? 1
                : caption.Contains(search, StringComparison.OrdinalIgnoreCase) ? 2
                : -1;
            if (score >= 0)
            {
                hits.Add((score, caption, item, type));
            }
        }

        return new JsonObject
        {
            ["total"] = hits.Count,
            ["items"] = new JsonArray(hits.OrderBy(h => h.Score).ThenBy(h => h.Caption.Length).ThenBy(h => h.Caption, StringComparer.OrdinalIgnoreCase).Take(limit).Select(h => (JsonNode)new JsonObject
            {
                ["type"] = h.Type.Name, ["id"] = h.Item.GetId().ToString(), ["caption"] = h.Caption,
                ["owner"] = owners.OwnerOf(h.Item) is { } owner ? new JsonObject { ["type"] = SiteTypes.ModelType(owner.Owner)?.Name, ["id"] = owner.Owner.GetId().ToString(), ["caption"] = SiteSchema.Caption(owner.Owner) } : null,
            }).ToArray()),
        };
    }

    /// <summary>Previews or applies a changeset.</summary>
    /// <param name="rows">The rows: <c>[{op: update|create|delete, type, id, version, expect, fields, parentType, parentId, collection}]</c>.</param>
    /// <param name="apply">Whether to apply the previewed change.</param>
    /// <param name="confirmationToken">The preview's token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview with its token, or the applied change.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Administrator, ReadOnly = false)]
    public ValueTask<JsonObject> ChangeAsync(string rows, bool apply, string? confirmationToken, CancellationToken cancellationToken)
        => this.RunAsync("config_changeset", new { rows = Digest(rows) }, (_, _) => ValueTask.FromResult(ParseRows(rows)), apply, confirmationToken, cancellationToken);

    /// <summary>Sets, multiplies or adds to a number field of every object a filter takes (a page of them at a time).</summary>
    /// <param name="type">The type.</param>
    /// <param name="where">Conditions, as for a list.</param>
    /// <param name="owner">The nearest owner, as for a list.</param>
    /// <param name="field">The number field.</param>
    /// <param name="mode">set, multiply or add.</param>
    /// <param name="value">The value.</param>
    /// <param name="offset">The matches to skip (by identifier order).</param>
    /// <param name="limit">The matches to change, 1-150.</param>
    /// <param name="apply">Whether to apply the previewed change.</param>
    /// <param name="confirmationToken">The preview's token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview with its token and the number of matches, or the applied change.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Administrator, ReadOnly = false)]
    public async ValueTask<JsonObject> BulkAsync(string type, string? where, string? owner, string field, string mode, double value, int offset = 0, int limit = 150, bool apply = false, string? confirmationToken = null, CancellationToken cancellationToken = default)
    {
        var modelType = SiteTypes.Resolve(type);
        var target = SiteSchema.Fields(modelType).FirstOrDefault(f => f.Name == field && f.IsNumber && f.Writable)
            ?? throw new AdminOperationException($"{field} is no writable number field of {modelType.Name}.", AdminOperationErrorKind.InvalidInput);
        if (mode is not ("set" or "multiply" or "add") || !double.IsFinite(value) || limit is < 1 or > 150 || offset < 0)
        {
            throw new AdminOperationException("Mode is set, multiply or add, with a finite value; limit 1-150.", AdminOperationErrorKind.InvalidInput);
        }

        var total = 0;
        var result = await this.RunAsync(
            "config_bulk",
            new { type, where, owner, field, mode, value, offset, limit },
            async (context, config) =>
            {
                var matches = (await MatchingAsync(modelType, SiteFilter.Parse(modelType, null, where, owner), config, context, cancellationToken).ConfigureAwait(false)).OrderBy(o => o.GetId()).ToList();
                total = matches.Count;
                var rows = new JsonArray();
                foreach (var item in matches.Skip(offset).Take(limit))
                {
                    var now = Convert.ToDouble(target.Property.GetValue(item) ?? 0, System.Globalization.CultureInfo.InvariantCulture);
                    var next = Bounded(target, mode switch { "set" => value, "multiply" => now * value, _ => now + value });
                    rows.Add(new JsonObject { ["op"] = "update", ["type"] = modelType.Name, ["id"] = item.GetId().ToString(), ["fields"] = new JsonObject { [field] = JsonSerializer.SerializeToNode(Convert.ChangeType(next, Nullable.GetUnderlyingType(target.Property.PropertyType) ?? target.Property.PropertyType, System.Globalization.CultureInfo.InvariantCulture)) } });
                }

                return rows;
            },
            apply,
            confirmationToken,
            cancellationToken).ConfigureAwait(false);
        result["matches"] = total;
        return result;
    }

    /// <summary>Exports an object and everything it owns.</summary>
    /// <param name="type">The type.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>{format, type, id, caption, objects: [{type, id, parentType, parentId, collection, fields}]}</c>, owners first.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Viewer)]
    public async ValueTask<JsonObject> ExportAsync(string type, Guid id, CancellationToken cancellationToken)
    {
        var modelType = SiteTypes.Resolve(type);
        using var context = persistence.CreateNewContext();
        var config = await this.LoadAsync(context, cancellationToken).ConfigureAwait(false);
        var root = await SiteTypes.FindAsync(modelType, id, config, context, cancellationToken).ConfigureAwait(false)
            ?? throw new AdminResourceNotFoundException($"There is no {modelType.Name} {id}.", nameof(id));
        var objects = new JsonArray();
        var pending = new Queue<(object Item, object? Parent, string? Collection)>([(root, null, null)]);
        while (pending.TryDequeue(out var next))
        {
            if (objects.Count >= 5000)
            {
                throw new AdminOperationException("The export holds more than 5000 objects. Export a smaller part.", AdminOperationErrorKind.InvalidInput);
            }

            var itemType = SiteTypes.ModelType(next.Item)!;
            objects.Add(new JsonObject
            {
                ["type"] = itemType.Name, ["id"] = next.Item.GetId().ToString(),
                ["parentType"] = next.Parent is null ? null : SiteTypes.ModelType(next.Parent)?.Name, ["parentId"] = next.Parent?.GetId().ToString(), ["collection"] = next.Collection,
                ["fields"] = ConfigurationPatch.Snapshot(next.Item, itemType),
            });
            foreach (var (child, property) in ConfigurationOwners.Children(next.Item))
            {
                if (SiteTypes.ModelType(child) is not null && property.PropertyType.IsGenericType)
                {
                    pending.Enqueue((child, next.Item, property.Name));
                }
            }
        }

        return new JsonObject { ["format"] = "openmu-config/1", ["type"] = modelType.Name, ["id"] = id.ToString(), ["caption"] = SiteSchema.Caption(root), ["objects"] = objects };
    }

    /// <summary>Imports an export: the objects that exist get the exported values, the missing ones are created.</summary>
    /// <param name="data">The export.</param>
    /// <param name="apply">Whether to apply the previewed change.</param>
    /// <param name="confirmationToken">The preview's token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview with its token, or the applied change.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Administrator, ReadOnly = false)]
    public ValueTask<JsonObject> ImportAsync(string data, bool apply, string? confirmationToken, CancellationToken cancellationToken)
        => this.RunAsync("config_import", new { data = Digest(data) }, (context, config) => ImportRowsAsync(data, config, context, cancellationToken), apply, confirmationToken, cancellationToken);

    /// <summary>The configuration changes made through changesets, newest first.</summary>
    /// <param name="offset">The entries to skip.</param>
    /// <param name="limit">The entries to return, 1-100.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The page: who, when, what, how it ended, and its first rows.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Administrator)]
    public async ValueTask<JsonObject> HistoryAsync(int offset = 0, int limit = 25, CancellationToken cancellationToken = default)
    {
        await executor.RequireAsync(AdminRoles.Administrator, cancellationToken).ConfigureAwait(false);
        AdminReadResult.ValidatePage(offset, limit);
        var found = new List<AdminAuditEntry>();
        var skipped = 0;
        for (var page = 0; page < 50 && found.Count < limit; page++)
        {
            var (items, total) = await audit.ReadAsync(page * 100, 100, cancellationToken).ConfigureAwait(false);
            foreach (var entry in items.Where(e => Changesets.Contains(e.Operation)))
            {
                if (skipped++ >= offset && found.Count < limit)
                {
                    found.Add(entry);
                }
            }

            if ((page + 1) * 100 >= total)
            {
                break;
            }
        }

        return new JsonObject
        {
            ["offset"] = offset,
            ["items"] = new JsonArray(found.Select(e => (JsonNode)new JsonObject
            {
                ["id"] = e.Id.ToString(), ["at"] = e.StartedAt.ToString("O"), ["by"] = e.PrincipalName, ["frontend"] = e.Frontend,
                ["operation"] = e.Operation, ["outcome"] = e.Outcome, ["rows"] = Rows(e)?.DeepClone() ?? new JsonArray(),
            }).ToArray()),
        };
    }

    /// <summary>Previews or applies the reversal of a changeset: refused when anything it changed was changed again.</summary>
    /// <param name="auditId">The changeset's audit entry.</param>
    /// <param name="apply">Whether to apply the previewed change.</param>
    /// <param name="confirmationToken">The preview's token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview with its token, or the applied change.</returns>
    [NotExposedToAgents("The website's editor; agents read and change the configuration with the config_* tools.")]
    [AdminOperation(AdminRole.Administrator, ReadOnly = false)]
    public async ValueTask<JsonObject> RevertAsync(Guid auditId, bool apply, string? confirmationToken, CancellationToken cancellationToken)
    {
        var entry = await audit.GetAsync(auditId, cancellationToken).ConfigureAwait(false);
        if (entry is not { Outcome: "Succeeded" } || !Changesets.Contains(entry.Operation) || Rows(entry) is not { Count: > 0 } rows)
        {
            throw new AdminOperationException("Only a successful changeset with its rows in the audit can be reverted.", AdminOperationErrorKind.InvalidInput);
        }

        return await this.RunAsync("config_revert", new { auditId }, (_, _) => ValueTask.FromResult(Inverse(rows)), apply, confirmationToken, cancellationToken).ConfigureAwait(false);
    }

    private static JsonArray? Rows(AdminAuditEntry entry)
    {
        try
        {
            return JsonNode.Parse(entry.ResultJson)?["change"]?["rows"] as JsonArray;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>The rows that undo a changeset, last first: updates put the values back (expecting what they set), creates are deleted, deletes created again with their identifier.</summary>
    private static JsonArray Inverse(JsonArray rows)
    {
        var inverse = new JsonArray();
        foreach (var row in rows.OfType<JsonObject>().Reverse())
        {
            var text = (string name) => row[name]?.GetValue<string>();
            inverse.Add(text("o") switch
            {
                "u" => new JsonObject { ["op"] = "update", ["type"] = text("t"), ["id"] = text("i"), ["expect"] = row["a"]?.DeepClone(), ["fields"] = row["b"]?.DeepClone() },
                "c" => new JsonObject { ["op"] = "delete", ["type"] = text("t"), ["id"] = text("i"), ["expect"] = row["a"]?.DeepClone() },
                _ => new JsonObject
                {
                    ["op"] = "create", ["type"] = text("t"), ["id"] = text("i"), ["parentType"] = text("p"), ["parentId"] = text("pi"), ["collection"] = text("c"),
                    ["fields"] = new JsonObject(((JsonObject)row["b"]!).Where(p => p.Value is not null && p.Value is not JsonArray { Count: 0 }).Select(p => KeyValuePair.Create(p.Key, p.Value?.DeepClone()))),
                },
            });
        }

        return inverse;
    }

    private static async ValueTask<JsonArray> ImportRowsAsync(string data, GameConfiguration config, IContext context, CancellationToken cancellationToken)
    {
        if (ParseJson(data) is not JsonObject export || export["format"]?.GetValue<string>() != "openmu-config/1" || export["objects"] is not JsonArray objects)
        {
            throw new AdminOperationException("This is not an export of the configuration editor (format openmu-config/1).", AdminOperationErrorKind.InvalidInput);
        }

        var rows = new JsonArray();
        foreach (var exported in objects.OfType<JsonObject>())
        {
            var type = SiteTypes.Resolve(exported["type"]?.GetValue<string>() ?? string.Empty);
            var fields = exported["fields"] as JsonObject ?? new JsonObject();
            var id = Guid.TryParse(exported["id"]?.GetValue<string>(), out var parsed) ? parsed : throw new AdminOperationException("An exported object has no identifier.", AdminOperationErrorKind.InvalidInput);
            if (await SiteTypes.FindAsync(type, id, config, context, cancellationToken).ConfigureAwait(false) is { } existing)
            {
                var now = ConfigurationPatch.Snapshot(existing, type);
                var changed = new JsonObject(fields.Where(p => now.ContainsKey(p.Key) && !JsonNode.DeepEquals(now[p.Key], p.Value)).Select(p => KeyValuePair.Create(p.Key, p.Value?.DeepClone())));
                if (changed.Count > 0)
                {
                    rows.Add(new JsonObject { ["op"] = "update", ["type"] = type.Name, ["id"] = id.ToString(), ["fields"] = changed });
                }
            }
            else
            {
                rows.Add(new JsonObject
                {
                    ["op"] = "create", ["type"] = type.Name, ["id"] = id.ToString(), ["parentType"] = exported["parentType"]?.DeepClone(), ["parentId"] = exported["parentId"]?.DeepClone(), ["collection"] = exported["collection"]?.DeepClone(),
                    ["fields"] = new JsonObject(fields.Where(p => p.Value is not null && p.Value is not JsonArray { Count: 0 }).Select(p => KeyValuePair.Create(p.Key, p.Value?.DeepClone()))),
                });
            }
        }

        return rows.Count > 0 ? rows : throw new AdminOperationException("The configuration already holds every exported value.", AdminOperationErrorKind.InvalidInput);
    }

    private static JsonArray ParseRows(string rows)
        => ParseJson(rows) as JsonArray ?? throw new AdminOperationException("Rows are a JSON array.", AdminOperationErrorKind.InvalidInput);

    private static JsonNode? ParseJson(string text)
    {
        try
        {
            return JsonNode.Parse(text, documentOptions: new JsonDocumentOptions { MaxDepth = 16 });
        }
        catch (JsonException)
        {
            throw new AdminOperationException("Not valid JSON.", AdminOperationErrorKind.InvalidInput);
        }
    }

    private static string Digest(string text) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)))[..32];

    private static double Bounded(SiteSchema.Field field, double value)
    {
        var (min, max) = field.Bounds;
        var target = Nullable.GetUnderlyingType(field.Property.PropertyType) ?? field.Property.PropertyType;
        var rounded = target == typeof(float) || target == typeof(double) || target == typeof(decimal) ? value : Math.Round(value, MidpointRounding.AwayFromZero);
        return Math.Clamp(rounded, min ?? double.MinValue, max ?? double.MaxValue);
    }

    private static List<object> Sort(List<object> items, Type type, string? sort)
    {
        var descending = sort?.StartsWith('-') == true;
        var name = descending ? sort![1..] : sort;
        var field = SiteSchema.Fields(type).FirstOrDefault(f => f.Name == name && f.Kind is "scalar" or "enum")
            ?? SiteSchema.Fields(type).FirstOrDefault(f => f.Name == "Number" && f.IsNumber);
        IComparable? Key(object item) => field is null ? SiteSchema.Caption(item) : field.Property.GetValue(item) as IComparable ?? SiteSchema.Caption(item);
        var ordered = descending ? items.OrderByDescending(Key) : items.OrderBy(Key);
        return ordered.ThenBy(SiteSchema.Caption, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static async ValueTask<List<object>> MatchingAsync(Type type, SiteFilter filter, GameConfiguration config, IContext context, CancellationToken cancellationToken)
    {
        var owners = filter.Owner is null ? null : ConfigurationOwners.Of(await SiteTypes.RootsAsync(config, context, cancellationToken).ConfigureAwait(false));
        return (await SiteTypes.ObjectsAsync(type, config, context, cancellationToken).ConfigureAwait(false)).Where(o => filter.Passes(o, owners)).ToList();
    }

    private async ValueTask<GameConfiguration> LoadAsync(IContext context, CancellationToken cancellationToken)
    {
        var id = (await configurations.GetOwnerAsync(cancellationToken: cancellationToken).ConfigureAwait(false)).GetId();
        return await context.GetByIdAsync<GameConfiguration>(id, cancellationToken).ConfigureAwait(false)
            ?? throw new AdminOperationException("The active game configuration could not be loaded.", AdminOperationErrorKind.Unavailable);
    }

    /// <summary>
    /// Plans the rows against the current state, then previews (a token bound to the whole change) or applies (the
    /// token is checked against the change planned again now, so anything that moved in between refuses it).
    /// </summary>
    private async ValueTask<JsonObject> RunAsync(string operation, object arguments, Func<IContext, GameConfiguration, ValueTask<JsonArray>> rows, bool apply, string? token, CancellationToken cancellationToken)
    {
        var actor = await AdminChangeExecution.RequireAsync(executor, settings, AdminRoles.Administrator, cancellationToken).ConfigureAwait(false);
        using var gate = apply ? await confirmations.Gate.LockAsync(cancellationToken).ConfigureAwait(false) : null;
        using var context = persistence.CreateNewContext();
        if (apply && context is IConfigurationEditContext edit)
        {
            await edit.BeginConfigurationEditAsync(cancellationToken).ConfigureAwait(false);
        }

        var config = await this.LoadAsync(context, cancellationToken).ConfigureAwait(false);
        var set = await ConfigurationChangeSet.PlanAsync(context, config, await rows(context, config).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
        if (set.Count == 0)
        {
            throw new AdminOperationException("Nothing to change: every value is already as asked.", AdminOperationErrorKind.InvalidInput);
        }

        var change = set.Change;
        if (change.ToJsonString().Length > MaximumChange)
        {
            throw new AdminOperationException($"The change is too large for one audit row ({change.ToJsonString().Length} characters, at most {MaximumChange}). Split it.", AdminOperationErrorKind.InvalidInput);
        }

        var result = await AdminChangeExecution.RunAsync(
            executor,
            confirmations,
            actor,
            AdminRoles.Administrator,
            operation,
            arguments,
            change,
            apply,
            token,
            async () =>
            {
                await set.ApplyAsync().ConfigureAwait(false);
                await AdminChangeExecution.SaveAsync(context, cancellationToken).ConfigureAwait(false);
                await configurations.InvalidateAsync().ConfigureAwait(false);
                if (changes is not null)
                {
                    await set.NotifyAsync(changes, options?.SavesPublishChanges ?? true).ConfigureAwait(false);
                }
            },
            cancellationToken).ConfigureAwait(false);
        result["applies"] = set.Applies;
        result["affectedObjects"] = set.Count;
        result.Remove("restartGuidance");
        result["restartRequired"] = set.Applies["all"]!.GetValue<string>() == ConfigurationApplies.Restart;
        return result;
    }
}
