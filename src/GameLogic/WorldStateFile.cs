// <copyright file="WorldStateFile.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic;

using System.IO;
using System.Text.Json;

/// <summary>
/// A small JSON file which keeps a runtime state of a feature over a restart of the server, e.g. the respawn
/// timers of the field bosses. It's state, not configuration: losing it only resets the timers.
/// </summary>
public sealed class WorldStateFile
{
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldStateFile"/> class.
    /// </summary>
    /// <param name="path">The path; a relative one is resolved against the directory of the server.</param>
    public WorldStateFile(string path)
    {
        this.Path = System.IO.Path.IsPathRooted(path) ? path : System.IO.Path.Combine(AppContext.BaseDirectory, path);
    }

    /// <summary>
    /// Gets the full path of the file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Reads the state.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <returns>The state, or <c>null</c> when the file is missing or can't be read.</returns>
    public T? Load<T>()
        where T : class
    {
        lock (this._lock)
        {
            try
            {
                return File.Exists(this.Path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(this.Path)) : null;
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Writes the state, replacing the file at once so a crash never leaves half of it.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="state">The state.</param>
    public void Save<T>(T state)
    {
        lock (this._lock)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path)!);
            var temporary = this.Path + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(state));
            File.Move(temporary, this.Path, true);
        }
    }
}
