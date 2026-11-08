// <copyright file="CachedCollections.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.EntityFramework;

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// The collections of the cached configuration, which hold copies: an object is found by its id, not by reference.
/// </summary>
internal static class CachedCollections
{
    /// <summary>
    /// Gets the object with <paramref name="id"/> in the collection of <paramref name="parent"/>.
    /// </summary>
    /// <param name="collectionAccessor">The collection's accessor.</param>
    /// <param name="parent">The cached parent.</param>
    /// <param name="id">The id.</param>
    /// <returns>The cached object, or <c>null</c>.</returns>
    public static object? Find(IClrCollectionAccessor collectionAccessor, object parent, Guid id)
    {
        return collectionAccessor.GetOrCreate(parent, false) is IEnumerable collection
            ? collection.Cast<object>().FirstOrDefault(cached => cached.GetId() == id)
            : null;
    }
}
