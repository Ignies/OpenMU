// <copyright file="CharacterStateStore.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

using System.Collections.Concurrent;
using System.IO;

/// <summary>
/// A runtime state per character of a feature, kept in a <see cref="WorldStateFile"/> over a restart, e.g. the hunting
/// records. One store exists per file in the process, so the game servers of an all-in-one deployment share it.
/// It's state, not configuration: losing the file resets the progress of the feature, not the characters.
/// </summary>
/// <typeparam name="T">The type of the state of a character.</typeparam>
public sealed class CharacterStateStore<T>
    where T : class, new()
{
    private static readonly ConcurrentDictionary<string, CharacterStateStore<T>> Shared = new(StringComparer.OrdinalIgnoreCase);

    private readonly WorldStateFile? _file;

    private readonly Dictionary<Guid, T> _states;

    private DateTime _lastSave = DateTime.MinValue;

    private bool _isDirty;

    private CharacterStateStore(WorldStateFile? file)
    {
        this._file = file;
        this._states = file?.Load<Dictionary<Guid, T>>() ?? [];
    }

    /// <summary>
    /// Gets the lock object which guards the states; hold it while reading or changing one.
    /// </summary>
    public object SyncRoot { get; } = new();

    /// <summary>
    /// Gets the store of a file; an empty path gives a store which only lives in memory.
    /// </summary>
    /// <param name="path">The path of the state file, relative to the server directory.</param>
    /// <returns>The store.</returns>
    public static CharacterStateStore<T> Get(string? path)
        => string.IsNullOrWhiteSpace(path)
            ? new CharacterStateStore<T>(null)
            : Shared.GetOrAdd(new WorldStateFile(path).Path, full => new CharacterStateStore<T>(new WorldStateFile(full)));

    /// <summary>
    /// Gets the state of a character, creating it. The caller holds <see cref="SyncRoot"/>.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <returns>The state.</returns>
    public T GetOrCreate(Guid characterId)
    {
        if (!this._states.TryGetValue(characterId, out var state))
        {
            state = new T();
            this._states.Add(characterId, state);
        }

        this._isDirty = true;
        return state;
    }

    /// <summary>
    /// Gets the state of a character, if it has one. The caller holds <see cref="SyncRoot"/>.
    /// </summary>
    /// <param name="characterId">The id of the character.</param>
    /// <returns>The state, or <c>null</c>.</returns>
    public T? Find(Guid characterId) => this._states.GetValueOrDefault(characterId);

    /// <summary>
    /// Gets every state. The caller holds <see cref="SyncRoot"/>.
    /// </summary>
    /// <returns>The states.</returns>
    public IEnumerable<KeyValuePair<Guid, T>> GetAll() => this._states;

    /// <summary>
    /// Writes the file when something changed and the last write is at least <paramref name="interval"/> ago.
    /// </summary>
    /// <param name="interval">The minimum time between two writes; <see cref="TimeSpan.Zero"/> writes now.</param>
    /// <param name="logger">The logger for a failed write.</param>
    public void Save(TimeSpan interval, ILogger logger)
    {
        lock (this.SyncRoot)
        {
            if (this._file is null || !this._isDirty || DateTime.UtcNow - this._lastSave < interval)
            {
                return;
            }

            try
            {
                this._file.Save(this._states);
                this._isDirty = false;
                this._lastSave = DateTime.UtcNow;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                logger.LogWarning(ex, "The state can't be written to {File}.", this._file.Path);
            }
        }
    }
}
