// <copyright file="ConfigurationRules.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations;

using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Composition;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence;

/// <summary>The checks and snapshots every configuration change shares: the operations' single-object writes and the site's changesets.</summary>
internal static class ConfigurationRules
{
    /// <summary>Refuses references to objects outside the active configuration.</summary>
    /// <param name="config">The active configuration.</param>
    /// <param name="values">The values.</param>
    internal static void ValidateReferences(GameConfiguration config, IReadOnlyDictionary<PropertyInfo, object?> values)
    {
        foreach (var (property, value) in values.Where(p => p.Value is not null && !ConfigurationPatch.IsScalar(p.Key.PropertyType)))
        {
            var type = ConfigurationPatch.IsReferenceCollection(property) ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType;
            var allowed = GameConfigurationHelper.Enumerables[type](config).Cast<object>().Select(o => o.GetId()).ToHashSet();
            var references = value is List<object> list ? list : [value!];
            if (references.Any(o => !allowed.Contains(o.GetId())))
            {
                throw new AdminOperationException("References must belong to the active game configuration.", AdminOperationErrorKind.InvalidInput);
            }
        }
    }

    /// <summary>The fingerprint of a snapshot, with binary fields and aggregate children by identity.</summary>
    /// <param name="value">The snapshot.</param>
    /// <param name="item">The object, or null.</param>
    /// <param name="type">The type.</param>
    /// <returns>The fingerprint.</returns>
    internal static string Fingerprint(JsonObject? value, object? item, Type type)
    {
        if (value is null)
        {
            return Convert.ToHexString(SHA256.HashData("null"u8));
        }

        var state = value.DeepClone().AsObject();
        foreach (var property in type.GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0).OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            var field = item is null ? null : property.GetValue(item);
            if (field is byte[] bytes)
            {
                state["$binary:" + property.Name] = Convert.ToHexString(SHA256.HashData(bytes));
            }
            else if (property.IsDefined(typeof(MemberOfAggregateAttribute)) && field is IEnumerable children)
            {
                state["$children:" + property.Name] = new JsonArray(children.Cast<object>().Select(c => c.GetId().ToString()).Order(StringComparer.Ordinal).Select(id => (JsonNode)JsonValue.Create(id)!).ToArray());
            }
            else
            {
                // Scalar/reference fields are projected already; other shapes are outside the editable surface.
            }
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(state.ToJsonString())));
    }

    /// <summary>The fields of a snapshot that a change names, in name order.</summary>
    /// <param name="snapshot">The snapshot.</param>
    /// <param name="fields">The fields.</param>
    /// <returns>The selected fields.</returns>
    internal static JsonObject SelectFields(JsonObject snapshot, IEnumerable<PropertyInfo> fields)
    {
        var result = new JsonObject();
        foreach (var field in fields.OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            result[field.Name] = snapshot[field.Name]?.DeepClone();
        }

        return result;
    }

    /// <summary>A validated copy of an object (or a new one) with the given values, for the after-state of a preview.</summary>
    /// <param name="item">The object, or null for a new one.</param>
    /// <param name="type">The type.</param>
    /// <param name="values">The values.</param>
    /// <returns>The candidate.</returns>
    internal static object Candidate(object? item, Type type, Dictionary<PropertyInfo, object?> values)
    {
        var candidate = Activator.CreateInstance(type) ?? throw new AdminOperationException("This configuration type cannot be constructed.");
        if (item is not null)
        {
            foreach (var property in type.GetProperties().Where(p => p.CanRead && p.SetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0))
            {
                property.SetValue(candidate, property.GetValue(item));
            }
        }

        foreach (var (property, value) in values.Where(v => !ConfigurationPatch.IsReferenceCollection(v.Key)))
        {
            property.SetValue(candidate, value);
        }

        ConfigurationPatch.Validate(candidate);
        return candidate;
    }

    /// <summary>An object of the active configuration by type and identifier.</summary>
    /// <param name="config">The active configuration.</param>
    /// <param name="type">The type.</param>
    /// <param name="id">The identifier.</param>
    /// <returns>The object.</returns>
    internal static object Find(GameConfiguration config, Type type, Guid id)
        => GameConfigurationHelper.Enumerables[type](config).Cast<object>().FirstOrDefault(o => o.GetId() == id)
           ?? throw new AdminOperationException("The configuration object is not part of the active game configuration.", AdminOperationErrorKind.InvalidInput);

    /// <summary>An aggregate collection of a type, by name, with its child type.</summary>
    /// <param name="type">The owning type.</param>
    /// <param name="name">The collection field.</param>
    /// <returns>The collection and its child type.</returns>
    internal static (PropertyInfo Property, Type ChildType) Collection(Type type, string name)
    {
        var property = type.GetProperty(name);
        if (property is null || !property.IsDefined(typeof(MemberOfAggregateAttribute)) || !property.PropertyType.IsGenericType || property.PropertyType.GetGenericTypeDefinition() != typeof(ICollection<>))
        {
            throw new AdminOperationException("Choose an aggregate ICollection field from config_describe.", AdminOperationErrorKind.InvalidInput);
        }

        var childType = property.PropertyType.GetGenericArguments()[0];
        ConfigurationPatch.Resolve(childType.Name);
        return (property, childType);
    }

    /// <summary>Refuses deleting an object that owns children, carries fields outside the snapshot or is referenced.</summary>
    /// <param name="config">The active configuration.</param>
    /// <param name="item">The object.</param>
    /// <param name="parent">Its owner.</param>
    /// <param name="owningCollection">The owner's collection that holds it.</param>
    /// <param name="type">The object's type.</param>
    internal static void CheckDeletion(GameConfiguration config, object item, object parent, PropertyInfo owningCollection, Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new AdminOperationException("This immutable configuration type has no safe reconstruction path. Use the panel to manage it.");
        }

        var restorable = ConfigurationPatch.Fields(type).Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
        if (type.GetProperties().Any(p => p.SetMethod?.IsPublic == true && p.Name != "Id" && !restorable.Contains(p.Name) && p.GetValue(item) is not null))
        {
            throw new AdminOperationException("The object contains fields outside the safe snapshot. Use the panel to manage it; deleting it here would lose data needed for reversal.");
        }

        if (type.GetProperties().Where(p => p.IsDefined(typeof(MemberOfAggregateAttribute))).Any(p => p.GetValue(item) is IEnumerable children ? children.Cast<object>().Any() : p.GetValue(item) is not null))
        {
            throw new AdminOperationException("The object owns child objects. Remove them in separate reviewed steps before deleting it.");
        }

        foreach (var (sourceType, enumerate) in GameConfigurationHelper.Enumerables)
        {
            foreach (var source in enumerate(config).Cast<object>())
            {
                foreach (var property in sourceType.GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && !ConfigurationPatch.IsScalar(p.PropertyType)))
                {
                    if ((ReferenceEquals(source, parent) && property.Name == owningCollection.Name) || ReferenceEquals(source, item))
                    {
                        continue;
                    }

                    var value = property.GetValue(source);
                    if (ReferenceEquals(value, item) || (value is IEnumerable references && references.Cast<object>().Any(v => ReferenceEquals(v, item))))
                    {
                        throw new AdminOperationException("The object is still referenced by another configuration object. Remove the reference before deleting it.");
                    }
                }
            }
        }
    }
}
