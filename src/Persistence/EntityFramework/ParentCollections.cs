// <copyright file="ParentCollections.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.EntityFramework;

using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// The collection of a parent which holds a changed entry, read from the model.
/// </summary>
internal static class ParentCollections
{
    /// <summary>
    /// Gets the collection navigation of <paramref name="parentType"/> which <paramref name="foreignKey"/> fills.
    /// Listing a tracked entry's collections instead detects the changes of every collection it has, which made
    /// a save of thousands of changed items take a pass over the whole game configuration for each of them.
    /// </summary>
    /// <param name="parentType">The parent's entity type.</param>
    /// <param name="foreignKey">The foreign key of the changed entry to its parent.</param>
    /// <returns>The navigation, or <c>null</c>.</returns>
    public static INavigation? Of(IEntityType parentType, IForeignKey? foreignKey)
    {
        return foreignKey is null
            ? null
            : parentType.GetNavigations().FirstOrDefault(n => n.IsCollection && n.ForeignKey == foreignKey);
    }
}
