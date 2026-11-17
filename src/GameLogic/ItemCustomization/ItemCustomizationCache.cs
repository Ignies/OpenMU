// <copyright file="ItemCustomizationCache.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

using System.Collections.Concurrent;

/// <summary>
/// The customisation records read lately, per item (a miss too). Shared by every game server of the process, since
/// they read the same database; a writer outside the fork (the Hidden plugin) calls <see cref="Forget"/> by name.
/// </summary>
public static class ItemCustomizationCache
{
    private const int MaximumCached = 50_000;

    private static readonly ConcurrentDictionary<Guid, (DataModel.Entities.ItemCustomization? Customization, long Expires)> Records = new();

    /// <summary>
    /// Stores a record, or a miss (null), until the given tick.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="customization">The record, or null when the item has none.</param>
    /// <param name="expires">The tick (<see cref="Environment.TickCount64"/>) it expires at.</param>
    public static void Store(Guid itemId, DataModel.Entities.ItemCustomization? customization, long expires)
    {
        if (Records.Count > MaximumCached)
        {
            Records.Clear();
        }

        Records[itemId] = (customization, expires);
    }

    /// <summary>
    /// Gets a stored record that has not expired.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="now">The current tick.</param>
    /// <param name="customization">The record, or null for a stored miss.</param>
    /// <returns>True when something is stored for the item.</returns>
    public static bool TryGet(Guid itemId, long now, out DataModel.Entities.ItemCustomization? customization)
    {
        var found = Records.TryGetValue(itemId, out var cached) && cached.Expires > now;
        customization = found ? cached.Customization : null;
        return found;
    }

    /// <summary>
    /// Drops an item, so the next request reads its record again.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    public static void Forget(Guid itemId) => Records.TryRemove(itemId, out _);
}
