// <copyright file="ArcaGuild.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// A guild registered for a war.
/// </summary>
public sealed class ArcaGuild
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArcaGuild"/> class.
    /// </summary>
    /// <param name="id">The id of the guild.</param>
    /// <param name="name">The name.</param>
    /// <param name="spawnGroup">The index of the spawn group of its members.</param>
    public ArcaGuild(uint id, string name, int spawnGroup)
    {
        this.Id = id;
        this.Name = name;
        this.SpawnGroup = spawnGroup;
    }

    /// <summary>
    /// Gets the id of the guild.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the index of the spawn group of its members.
    /// </summary>
    public int SpawnGroup { get; }

    /// <summary>
    /// Gets or sets the persistent id of the guild, if it's known.
    /// </summary>
    public Guid? PersistentId { get; set; }

    /// <summary>
    /// Gets the ids of the registered characters, the master first.
    /// </summary>
    public List<Guid> Members { get; } = [];
}
