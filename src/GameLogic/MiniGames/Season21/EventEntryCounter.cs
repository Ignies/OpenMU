// <copyright file="EventEntryCounter.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using System.Collections.Concurrent;

/// <summary>
/// Counts the event entries of the characters per day, against the <see cref="EventEntryLimit"/>s.
/// </summary>
/// <remarks>
/// The counts live in the game server which runs the plugin, and a restart clears them: the limit isn't shared between
/// the game servers of a world (that needs a stored field of the character).
/// </remarks>
public sealed class EventEntryCounter
{
    private readonly ConcurrentDictionary<(Guid Character, MiniGameType Type), (DateOnly Day, int Count)> _entries = new();

    /// <summary>
    /// Gets a value indicating whether a character entered an event as often as its limit allows on a day.
    /// </summary>
    /// <param name="character">The id of the character.</param>
    /// <param name="type">The event.</param>
    /// <param name="limits">The limits.</param>
    /// <param name="day">The day.</param>
    /// <returns><c>true</c>, if the limit is reached.</returns>
    public bool IsLimitReached(Guid character, MiniGameType type, IEnumerable<EventEntryLimit> limits, DateOnly day)
    {
        if (limits.FirstOrDefault(l => l.Type == type) is not { Limit: > 0 } limit)
        {
            return false;
        }

        return this._entries.TryGetValue((character, type), out var entries) && entries.Day == day && entries.Count >= limit.Limit;
    }

    /// <summary>
    /// Counts an entry of a character.
    /// </summary>
    /// <param name="character">The id of the character.</param>
    /// <param name="type">The event.</param>
    /// <param name="day">The day.</param>
    public void Count(Guid character, MiniGameType type, DateOnly day)
    {
        this._entries.AddOrUpdate((character, type), (day, 1), (_, entries) => entries.Day == day ? (day, entries.Count + 1) : (day, 1));
    }
}
