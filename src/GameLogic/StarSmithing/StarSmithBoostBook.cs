// <copyright file="StarSmithBoostBook.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Collections.Concurrent;

/// <summary>
/// The smithed boosts of the items in play, by the item's persistent id, so the power-up factory (synchronous) can read
/// them. Filled when a smith enters the world and when a seal or a downgrade writes a record.
/// </summary>
public static class StarSmithBoostBook
{
    private static readonly ConcurrentDictionary<Guid, IReadOnlyList<StarSmithBoost>> Boosts = new();

    /// <summary>
    /// Sets an item's boosts; none forgets it.
    /// </summary>
    /// <param name="itemId">The item's persistent id.</param>
    /// <param name="boosts">The boosts.</param>
    public static void Set(Guid itemId, IReadOnlyList<StarSmithBoost> boosts)
    {
        if (boosts.Count == 0)
        {
            Forget(itemId);
            return;
        }

        Boosts[itemId] = boosts;
    }

    /// <summary>
    /// Gets an item's boosts.
    /// </summary>
    /// <param name="itemId">The item's persistent id.</param>
    /// <returns>The boosts, empty when it has none.</returns>
    public static IReadOnlyList<StarSmithBoost> Get(Guid itemId) => Boosts.TryGetValue(itemId, out var boosts) ? boosts : [];

    /// <summary>
    /// Forgets an item's boosts.
    /// </summary>
    /// <param name="itemId">The item's persistent id.</param>
    public static void Forget(Guid itemId) => Boosts.TryRemove(itemId, out _);
}
