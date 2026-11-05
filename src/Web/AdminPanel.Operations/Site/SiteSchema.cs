// <copyright file="SiteSchema.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Composition;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Interfaces;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// What the site's editor needs to know of a type (Readme.md, "Schema"): every field with its kind (scalar, enum,
/// reference, references, children, child), whether it is writable, its bounds and when it applies; and the rows it
/// lists: the fields' values with the captions of what they reference.
/// </summary>
internal static class SiteSchema
{
    /// <summary>The most entries of a reference list a row carries.</summary>
    public const int MaximumReferences = 100;

    private static readonly string[] SecretNames = ["password", "secret", "token", "credential", "keyhash", "connectionstring"];

    private static readonly ConcurrentDictionary<Type, Field[]> FieldsOfType = new();

    /// <summary>Bounds the model checks (ConfigurationPatch.Validate) without declaring them on the field.</summary>
    private static readonly Dictionary<(Type Type, string Field), (double? Min, double? Max)> KnownBounds = new()
    {
        [(typeof(GameConfiguration), nameof(GameConfiguration.ExperienceRate))] = (0, null),
        [(typeof(GameConfiguration), nameof(GameConfiguration.MasterExperienceRate))] = (0, null),
        [(typeof(GameServerDefinition), nameof(GameServerDefinition.ExperienceRate))] = (0, null),
        [(typeof(MonsterSpawnArea), nameof(MonsterSpawnArea.Quantity))] = (0, null),
        [(typeof(DropItemGroup), nameof(DropItemGroup.Chance))] = (0, 1),
    };

    /// <summary>The fields of a type the site shows.</summary>
    /// <param name="type">The data model type.</param>
    /// <returns>The fields, by name.</returns>
    public static Field[] Fields(Type type) => FieldsOfType.GetOrAdd(type, Describe);

    /// <summary>The schema of a type.</summary>
    /// <param name="type">The data model type.</param>
    /// <returns>Its name, captions, when it applies and its fields.</returns>
    public static JsonObject Of(Type type) => new()
    {
        ["name"] = type.Name,
        ["caption"] = type.GetTypeCaption(),
        ["description"] = type.GetTypeDescription(),
        ["group"] = type.Namespace?.Split('.').Last(),
        ["root"] = type == typeof(GameConfiguration) || SiteTypes.ServerRoots.Contains(type),
        ["applies"] = ConfigurationApplies.Of(type),
        ["fields"] = new JsonArray(Fields(type).Select(f => (JsonNode)f.Describe()).ToArray()),
    };

    /// <summary>A list row: identifier, caption, version and the fields' values.</summary>
    /// <param name="item">The object.</param>
    /// <param name="type">Its data model type.</param>
    /// <param name="selected">The fields to carry, or null for all.</param>
    /// <returns>The row.</returns>
    public static JsonObject Row(object item, Type type, IReadOnlyCollection<string>? selected = null)
    {
        var fields = new JsonObject();
        foreach (var field in Fields(type).Where(f => selected is null || selected.Contains(f.Name)))
        {
            fields[field.Name] = field.Value(item);
        }

        return new JsonObject { ["id"] = item.GetId().ToString(), ["caption"] = Caption(item), ["version"] = Version(item, type), ["fields"] = fields };
    }

    /// <summary>The version of an object: a short fingerprint of its patchable fields and its children's identities.</summary>
    /// <param name="item">The object.</param>
    /// <param name="type">Its data model type.</param>
    /// <returns>The version.</returns>
    public static string Version(object item, Type type)
        => ConfigurationRules.Fingerprint(ConfigurationPatch.Snapshot(item, type), item, type)[..16].ToLowerInvariant();

    /// <summary>A short name for an object: its name or designation, what it is about, or its number.</summary>
    /// <param name="item">The object.</param>
    /// <returns>The caption.</returns>
    public static string Caption(object? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        var type = SiteTypes.ModelType(item) ?? item.GetType();
        var text = Text(item, type, "Name") ?? Text(item, type, "Designation") ?? Text(item, type, "Description");
        if (text is null && type.GetProperties().FirstOrDefault(p => p.PropertyType == typeof(AttributeSystem.AttributeDefinition) && p.GetIndexParameters().Length == 0)?.GetValue(item) is AttributeSystem.AttributeDefinition attribute)
        {
            text = attribute.Designation;
        }

        if (text is null && type.GetProperty("Number")?.GetValue(item) is { } number)
        {
            text = $"{type.GetTypeCaption()} {number}";
        }

        text ??= type.GetTypeCaption();
        return text.Length > 120 ? text[..120] : text;
    }

    private static string? Text(object item, Type type, string name)
        => type.GetProperty(name)?.GetValue(item) switch
        {
            string { Length: > 0 } text => text,
            LocalizedString { Value: { Length: > 0 } localized } => localized,
            _ => null,
        };

    private static Field[] Describe(Type type)
    {
        var writable = ConfigurationPatch.Fields(type).Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && p.Name != "Id" && !p.IsDefined(typeof(SecretAttribute))
                        && (writable.Contains(p.Name) || !IsSecretName(p.Name)))
            .Select(p => Field.Create(type, p, writable.Contains(p.Name)))
            .OfType<Field>()
            .OrderBy(f => f.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsSecretName(string name) => SecretNames.Any(n => name.Contains(n, StringComparison.OrdinalIgnoreCase));

    /// <summary>One field of a type.</summary>
    /// <param name="Type">The declaring data model type.</param>
    /// <param name="Property">The property.</param>
    /// <param name="Kind">scalar, enum, reference, references, children or child.</param>
    /// <param name="Target">The scalar's type name, or the referenced or owned type.</param>
    /// <param name="Writable">Whether a change may set it.</param>
    internal sealed record Field(Type Type, PropertyInfo Property, string Kind, Type Target, bool Writable)
    {
        /// <summary>Gets the name.</summary>
        public string Name => this.Property.Name;

        /// <summary>Gets a value indicating whether the field is a number.</summary>
        public bool IsNumber => this.Kind == "scalar" && Number(this.Target);

        /// <summary>Gets the bounds: declared, of the number type, or known from the model's checks.</summary>
        public (double? Min, double? Max) Bounds
        {
            get
            {
                if (KnownBounds.TryGetValue((this.Type, this.Name), out var known))
                {
                    return known;
                }

                if (this.Property.GetCustomAttribute<RangeAttribute>() is { } range)
                {
                    return (Convert.ToDouble(range.Minimum, System.Globalization.CultureInfo.InvariantCulture), Convert.ToDouble(range.Maximum, System.Globalization.CultureInfo.InvariantCulture));
                }

                return this.Target switch
                {
                    _ when this.Target == typeof(byte) => (byte.MinValue, byte.MaxValue),
                    _ when this.Target == typeof(sbyte) => (sbyte.MinValue, sbyte.MaxValue),
                    _ when this.Target == typeof(short) => (short.MinValue, short.MaxValue),
                    _ when this.Target == typeof(ushort) => (ushort.MinValue, ushort.MaxValue),
                    _ when this.Target == typeof(int) => (int.MinValue, int.MaxValue),
                    _ when this.Target == typeof(uint) => (uint.MinValue, uint.MaxValue),
                    _ when this.Target == typeof(long) => (long.MinValue, long.MaxValue),
                    _ => (null, null),
                };
            }
        }

        /// <summary>Describes a field of a property, or null when the site does not show that kind.</summary>
        /// <param name="type">The declaring type.</param>
        /// <param name="property">The property.</param>
        /// <param name="writable">Whether it is writable.</param>
        /// <returns>The field.</returns>
        public static Field? Create(Type type, PropertyInfo property, bool writable)
        {
            var propertyType = property.PropertyType;
            var scalar = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (ConfigurationPatch.IsScalar(propertyType))
            {
                return new Field(type, property, scalar.IsEnum ? "enum" : "scalar", scalar, writable);
            }

            var aggregate = property.IsDefined(typeof(MemberOfAggregateAttribute));
            var element = propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ? propertyType.GetGenericArguments()[0] : null;
            if (element is not null && aggregate)
            {
                return SiteTypes.All.Contains(element) ? new Field(type, property, "children", element, false) : null;
            }

            if (element is not null)
            {
                return ConfigurationPatch.IsReferenceCollection(property) ? new Field(type, property, "references", element, writable) : null;
            }

            if (aggregate)
            {
                return SiteTypes.All.Contains(propertyType) ? new Field(type, property, "child", propertyType, false) : null;
            }

            return ConfigurationPatch.IsReference(propertyType) || SiteTypes.All.Contains(propertyType) ? new Field(type, property, "reference", propertyType, writable) : null;
        }

        /// <summary>The field's description for the site.</summary>
        /// <returns>The description.</returns>
        public JsonObject Describe()
        {
            var (min, max) = this.Bounds;
            var result = new JsonObject
            {
                ["name"] = this.Name,
                ["kind"] = this.Kind,
                ["type"] = this.Target == typeof(LocalizedString) ? "String" : this.Target.Name,
                ["writable"] = this.Writable,
                ["nullable"] = !this.Property.PropertyType.IsValueType || Nullable.GetUnderlyingType(this.Property.PropertyType) is not null,
                ["applies"] = ConfigurationApplies.Of(this.Type, this.Name),
                ["caption"] = this.Type.GetPropertyCaption(this.Name),
                ["description"] = this.Type.GetPropertyDescription(this.Name),
            };
            if (this.Kind == "enum")
            {
                result["values"] = new JsonArray(Enum.GetNames(this.Target).Select(n => (JsonNode)n).ToArray());
                result["flags"] = this.Target.IsDefined(typeof(FlagsAttribute));
            }

            if (min is { } low)
            {
                result["min"] = low;
            }

            if (max is { } high)
            {
                result["max"] = high;
            }

            return result;
        }

        /// <summary>The field's value in a row.</summary>
        /// <param name="item">The object.</param>
        /// <returns>The value.</returns>
        public JsonNode? Value(object item)
        {
            var value = this.Property.GetValue(item);
            return this.Kind switch
            {
                _ when value is null => null,
                "scalar" or "enum" => value is LocalizedString localized ? JsonValue.Create(localized.Value)
                    : value.GetType().IsEnum ? JsonValue.Create(value.ToString()) : JsonSerializer.SerializeToNode(value, ConfigurationPatch.Options),
                "reference" or "child" => Reference(value),
                "references" => new JsonArray(((IEnumerable)value).Cast<object>().Take(MaximumReferences).Select(v => (JsonNode)Reference(v)).ToArray()),
                _ => new JsonObject { ["count"] = ((IEnumerable)value).Cast<object>().Count() },
            };
        }

        private static JsonObject Reference(object value) => new() { ["$ref"] = value.GetId().ToString(), ["caption"] = Caption(value) };

        private static bool Number(Type type) => (type.IsPrimitive && type != typeof(bool) && type != typeof(char)) || type == typeof(decimal);
    }
}
