// <copyright file="SiteTypes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Composition;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The types the site edits: the game configuration's graph (plugin configurations aside, they have their own operations
/// and may hold secrets) and the server definitions beside it.
/// </summary>
internal static class SiteTypes
{
    /// <summary>The root objects outside the game configuration.</summary>
    public static readonly Type[] ServerRoots = [typeof(GameServerDefinition), typeof(GameServerConfiguration), typeof(ConnectServerDefinition), typeof(ChatServerDefinition), typeof(SystemConfiguration)];

    private static readonly ConcurrentDictionary<Type, Type?> ModelTypes = new();

    /// <summary>
    /// Gets every editable type, by name: OpenMU's own list and the server roots, and every type they own through their
    /// aggregate fields (a mix's settings, a quest's rewards); characters' and accounts' entities stay out.
    /// </summary>
    public static IReadOnlyList<Type> All { get; } = Closure();

    /// <summary>The type of a name.</summary>
    /// <param name="name">The type name.</param>
    /// <returns>The type.</returns>
    public static Type Resolve(string name) => All.FirstOrDefault(t => t.Name == name)
        ?? throw new AdminOperationException("Unknown or excluded configuration type. Use config.schema for the list.", AdminOperationErrorKind.InvalidInput);

    /// <summary>The data model type of an object (persistence layers derive their own classes from it), or null.</summary>
    /// <param name="item">The object.</param>
    /// <returns>The most derived editable type it is an instance of.</returns>
    public static Type? ModelType(object item) => ModelTypes.GetOrAdd(item.GetType(), runtime =>
    {
        var candidates = All.Where(t => t.IsAssignableFrom(runtime)).ToList();
        return candidates.FirstOrDefault(t => !candidates.Any(o => o != t && t.IsAssignableFrom(o)));
    });

    /// <summary>Every object of a type.</summary>
    /// <param name="type">The type.</param>
    /// <param name="config">The game configuration.</param>
    /// <param name="context">The context the server roots are read from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The objects.</returns>
    public static async ValueTask<List<object>> ObjectsAsync(Type type, GameConfiguration config, IContext context, CancellationToken cancellationToken)
    {
        if (GameConfigurationHelper.Enumerables.TryGetValue(type, out var enumerate))
        {
            return Read(enumerate(config));
        }

        if (ServerRoots.Contains(type))
        {
            return Read(await context.GetAsync(type, cancellationToken).ConfigureAwait(false));
        }

        return ConfigurationOwners.Of(await RootsAsync(config, context, cancellationToken).ConfigureAwait(false)).All.Where(type.IsInstanceOfType).ToList();
    }

    /// <summary>The roots every owner walk starts from: the configuration and the server roots.</summary>
    /// <param name="config">The game configuration.</param>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The roots.</returns>
    public static async ValueTask<List<object>> RootsAsync(GameConfiguration config, IContext context, CancellationToken cancellationToken)
    {
        var roots = new List<object> { config };
        foreach (var root in ServerRoots)
        {
            roots.AddRange(Read(await context.GetAsync(root, cancellationToken).ConfigureAwait(false)));
        }

        return roots;
    }

    /// <summary>An object by type and identifier.</summary>
    /// <param name="type">The type.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="config">The game configuration.</param>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The object, or null.</returns>
    public static async ValueTask<object?> FindAsync(Type type, Guid id, GameConfiguration config, IContext context, CancellationToken cancellationToken)
        => (await ObjectsAsync(type, config, context, cancellationToken).ConfigureAwait(false)).FirstOrDefault(o => o.GetId() == id);

    private static Type[] Closure()
    {
        var known = GameConfigurationHelper.Enumerables.Keys.Concat(ServerRoots).Where(t => t != typeof(PlugInConfiguration)).ToHashSet();
        var pending = new Queue<Type>(known);
        while (pending.TryDequeue(out var type))
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsDefined(typeof(MemberOfAggregateAttribute))))
            {
                var owned = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ? property.PropertyType.GetGenericArguments()[0] : property.PropertyType;
                if (owned.IsClass && owned != typeof(string) && owned != typeof(PlugInConfiguration) && owned.Namespace != typeof(Item).Namespace && known.Add(owned))
                {
                    pending.Enqueue(owned);
                }
            }
        }

        return known.OrderBy(t => t.Name, StringComparer.Ordinal).ToArray();
    }

    private static List<object> Read(IEnumerable collection)
    {
        try
        {
            return collection.Cast<object>().ToList();
        }
        catch (InvalidOperationException ex)
        {
            throw new AdminOperationException("The configuration changed while it was read. Try again.", AdminOperationErrorKind.Conflict, ex);
        }
    }
}
