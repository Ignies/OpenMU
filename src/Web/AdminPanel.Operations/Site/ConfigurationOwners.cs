// <copyright file="ConfigurationOwners.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using MUnique.OpenMU.DataModel.Composition;

/// <summary>
/// Who owns what: one walk over the aggregate fields (<see cref="MemberOfAggregateAttribute"/>) from the roots. A change
/// to a monster's attribute is a change to the monster for the game, which registers the monster, not its attributes.
/// </summary>
internal sealed class ConfigurationOwners
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Aggregates = new();

    private readonly Dictionary<object, (object Owner, PropertyInfo Property)> _owners = new(ReferenceEqualityComparer.Instance);

    private readonly List<object> _all = [];

    private ConfigurationOwners()
    {
    }

    /// <summary>Gets every object reached, roots first.</summary>
    public IReadOnlyList<object> All => this._all;

    /// <summary>Walks the roots.</summary>
    /// <param name="roots">The roots.</param>
    /// <returns>The owners.</returns>
    public static ConfigurationOwners Of(IEnumerable<object> roots)
    {
        var owners = new ConfigurationOwners();
        var pending = new Stack<object>();
        foreach (var root in roots)
        {
            owners._all.Add(root);
            pending.Push(root);
        }

        while (pending.TryPop(out var node))
        {
            foreach (var (child, property) in Children(node))
            {
                if (owners._owners.TryAdd(child, (node, property)))
                {
                    owners._all.Add(child);
                    pending.Push(child);
                }
            }
        }

        return owners;
    }

    /// <summary>The aggregate fields of a type.</summary>
    /// <param name="type">The data model type.</param>
    /// <returns>The fields.</returns>
    public static PropertyInfo[] AggregateFields(Type type) => Aggregates.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetIndexParameters().Length == 0 && p.IsDefined(typeof(MemberOfAggregateAttribute))).OrderBy(p => p.Name, StringComparer.Ordinal).ToArray());

    /// <summary>The objects an object owns, with the field that holds each.</summary>
    /// <param name="node">The object.</param>
    /// <returns>The children.</returns>
    public static IEnumerable<(object Child, PropertyInfo Property)> Children(object node)
    {
        if (SiteTypes.ModelType(node) is not { } type)
        {
            yield break;
        }

        foreach (var property in AggregateFields(type))
        {
            var value = property.GetValue(node);
            if (value is IEnumerable collection and not string)
            {
                foreach (var child in collection.Cast<object>().ToList())
                {
                    yield return (child, property);
                }
            }
            else if (value is not null)
            {
                yield return (value, property);
            }
        }
    }

    /// <summary>The owner of an object and the field it is in, or null for a root.</summary>
    /// <param name="item">The object.</param>
    /// <returns>The owner.</returns>
    public (object Owner, PropertyInfo Property)? OwnerOf(object item) => this._owners.TryGetValue(item, out var owner) ? owner : null;

    /// <summary>The owners of an object, nearest first.</summary>
    /// <param name="item">The object.</param>
    /// <returns>The chain up to its root.</returns>
    public IEnumerable<object> Chain(object item)
    {
        var at = item;
        while (this.OwnerOf(at) is { } owner)
        {
            yield return owner.Owner;
            at = owner.Owner;
        }
    }
}
