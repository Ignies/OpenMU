// <copyright file="ArcaHolding.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// An obelisk a guild held at the end of a war: its members get the buff of the element until it expires.
/// </summary>
public class ArcaHolding
{
    /// <summary>
    /// Gets or sets the id of the guild at the time of the war.
    /// </summary>
    public uint GuildId { get; set; }

    /// <summary>
    /// Gets or sets the persistent id of the guild, if it was known.
    /// </summary>
    public Guid? PersistentGuildId { get; set; }

    /// <summary>
    /// Gets or sets the element.
    /// </summary>
    public ArcaElement Element { get; set; }

    /// <summary>
    /// Gets or sets when the buff ends.
    /// </summary>
    public DateTime ExpiresUtc { get; set; }
}
