// <copyright file="ArcaObelisk.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The state of an obelisk during a war.
/// </summary>
public sealed class ArcaObelisk
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArcaObelisk"/> class.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <param name="element">The element of this war.</param>
    public ArcaObelisk(ArcaObeliskDefinition definition, ArcaElement element)
    {
        this.Definition = definition;
        this.Element = element;
    }

    /// <summary>
    /// Gets the definition.
    /// </summary>
    public ArcaObeliskDefinition Definition { get; }

    /// <summary>
    /// Gets the element of this war.
    /// </summary>
    public ArcaElement Element { get; }

    /// <summary>
    /// Gets or sets the guild which holds the obelisk, 0 for none.
    /// </summary>
    public uint OwnerGuildId { get; set; }

    /// <summary>
    /// Gets or sets the guild for which the shield is down, 0 for none.
    /// </summary>
    public uint OpenForGuildId { get; set; }

    /// <summary>
    /// Gets or sets the guild which stands on all auras, 0 for none.
    /// </summary>
    public uint HoldingGuildId { get; set; }

    /// <summary>
    /// Gets or sets since when <see cref="HoldingGuildId"/> stands on all auras.
    /// </summary>
    public DateTime HoldingSinceUtc { get; set; }

    /// <summary>
    /// Gets or sets the time at which a destroyed obelisk comes back.
    /// </summary>
    public DateTime? RespawnAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the monster of the obelisk, while it stands.
    /// </summary>
    public ArcaObeliskNpc? Npc { get; set; }

    /// <summary>
    /// Gets the auras by the guilds standing on them: bit i for aura i.
    /// </summary>
    public Dictionary<uint, byte> AuraMasks { get; } = [];

    /// <summary>
    /// Gets the health in percent, 0 while it's destroyed.
    /// </summary>
    public byte HealthPercent => this.Npc is { IsAlive: true } npc ? (byte)Math.Clamp(npc.Health * 100L / npc.MaximumHealth, 0, 100) : (byte)0;

    /// <summary>
    /// Determines whether the shield is down for a guild, which may then hurt the obelisk.
    /// </summary>
    /// <param name="guildId">The guild.</param>
    /// <returns><c>true</c>, if the guild may hurt it.</returns>
    public bool IsOpenFor(uint guildId) => guildId != 0 && this.OpenForGuildId == guildId && this.OwnerGuildId != guildId;
}
