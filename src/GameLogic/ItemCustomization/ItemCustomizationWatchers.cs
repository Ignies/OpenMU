// <copyright file="ItemCustomizationWatchers.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

/// <summary>
/// Who was last told about a customised item, and for which place (Readme.md "Changes"): when the record changes, each
/// of them is told again at once (EE 39) instead of on their next ask. Weak, so a player who left is dropped.
/// </summary>
/// <typeparam name="T">The watcher (a player).</typeparam>
public sealed class ItemCustomizationWatchers<T>
    where T : class
{
    private const int MaximumItems = 50_000;

    private const int MaximumPerItem = 64;

    private readonly Dictionary<Guid, List<(WeakReference<T> Watcher, ItemCustomizationStorage Storage, ushort ObjectId)>> _places = [];

    /// <summary>
    /// Notes that a watcher was told about an item at a place.
    /// </summary>
    /// <param name="itemId">The item.</param>
    /// <param name="watcher">The watcher.</param>
    /// <param name="storage">The storage it asked about.</param>
    /// <param name="objectId">The object id it asked about.</param>
    public void Add(Guid itemId, T watcher, ItemCustomizationStorage storage, ushort objectId)
    {
        lock (this._places)
        {
            if (!this._places.TryGetValue(itemId, out var places))
            {
                if (this._places.Count >= MaximumItems)
                {
                    this._places.Clear();
                }

                this._places[itemId] = places = [];
            }

            places.RemoveAll(p => !p.Watcher.TryGetTarget(out var known) || (ReferenceEquals(known, watcher) && p.Storage == storage && p.ObjectId == objectId));
            if (places.Count < MaximumPerItem)
            {
                places.Add((new WeakReference<T>(watcher), storage, objectId));
            }
        }
    }

    /// <summary>
    /// Takes the places an item was told about, each once, of the watchers still there.
    /// </summary>
    /// <param name="itemId">The item.</param>
    /// <returns>The places.</returns>
    public IReadOnlyList<(T Watcher, ItemCustomizationStorage Storage, ushort ObjectId)> Take(Guid itemId)
    {
        lock (this._places)
        {
            if (!this._places.Remove(itemId, out var places))
            {
                return [];
            }

            var live = new List<(T, ItemCustomizationStorage, ushort)>(places.Count);
            foreach (var (watcher, storage, objectId) in places)
            {
                if (watcher.TryGetTarget(out var target))
                {
                    live.Add((target, storage, objectId));
                }
            }

            return live;
        }
    }
}
