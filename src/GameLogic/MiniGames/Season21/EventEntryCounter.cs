// <copyright file="EventEntryCounter.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using Microsoft.Extensions.Logging.Abstractions;

/// <summary>
/// Counts the event entries of the characters a day, for <see cref="Season21EventsConfiguration.DailyEntryLimits"/>. The
/// counts are kept in a state file shared by every game server of the process, so a restart keeps them.
/// </summary>
public sealed class EventEntryCounter
{
    private readonly CharacterStateStore<CharacterEventEntries> _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventEntryCounter"/> class.
    /// </summary>
    /// <param name="stateFile">The state file; none keeps the counts in memory.</param>
    public EventEntryCounter(string? stateFile = null)
    {
        this._store = CharacterStateStore<CharacterEventEntries>.Get(stateFile);
    }

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

        lock (this._store.SyncRoot)
        {
            return this._store.Find(character) is { } entries
                   && entries.Day == day
                   && entries.Counts.GetValueOrDefault(type) >= limit.Limit;
        }
    }

    /// <summary>
    /// Counts an entry of a character and writes the state file.
    /// </summary>
    /// <param name="character">The id of the character.</param>
    /// <param name="type">The event.</param>
    /// <param name="day">The day.</param>
    /// <param name="logger">The logger for a failed write.</param>
    public void Count(Guid character, MiniGameType type, DateOnly day, ILogger? logger = null)
    {
        lock (this._store.SyncRoot)
        {
            var entries = this._store.GetOrCreate(character);
            if (entries.Day != day)
            {
                entries.Day = day;
                entries.Counts.Clear();
            }

            entries.Counts[type] = entries.Counts.GetValueOrDefault(type) + 1;
        }

        this._store.Save(TimeSpan.Zero, logger ?? NullLogger.Instance);
    }
}
