// <copyright file="ChangedParents.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.EntityFramework;

using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// The parents a save announces as changed besides its changed entries.
/// </summary>
internal static class ChangedParents
{
    /// <summary>
    /// Gets the parents to announce: each once, in the order they came, without the game configuration and parents
    /// known only by id.
    /// </summary>
    /// <param name="parents">The parent of each changed entry, <c>null</c> for none.</param>
    /// <returns>The parents to announce.</returns>
    public static IReadOnlyList<object> ToAnnounce(IEnumerable<object?> parents)
    {
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return parents
            .Where(parent => parent is not null and not GameConfiguration and not Guid && seen.Add(parent))
            .Select(parent => parent!)
            .ToList();
    }
}
