// <copyright file="ArcaObeliskNpc.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The monster of an obelisk: it doesn't move or attack, and only the guild for which its shield is down can hurt it.
/// </summary>
public sealed class ArcaObeliskNpc : AttackableNpcBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArcaObeliskNpc"/> class.
    /// </summary>
    /// <param name="spawnInfo">The spawn information.</param>
    /// <param name="stats">The definition of the monster.</param>
    /// <param name="map">The map.</param>
    /// <param name="obelisk">The obelisk of the war.</param>
    /// <param name="dropGenerator">The drop generator.</param>
    /// <param name="plugInManager">The plugin manager.</param>
    public ArcaObeliskNpc(MonsterSpawnArea spawnInfo, MonsterDefinition stats, GameMap map, ArcaObelisk obelisk, IDropGenerator dropGenerator, PlugInManager plugInManager)
        : base(spawnInfo, stats, map, null, dropGenerator, plugInManager)
    {
        this.Obelisk = obelisk;
    }

    /// <summary>
    /// Gets the obelisk of the war.
    /// </summary>
    public ArcaObelisk Obelisk { get; }

    /// <summary>
    /// Gets the maximum health of this spawn.
    /// </summary>
    public int MaximumHealth => this.SpawnArea.MaximumHealthOverride ?? Math.Max(1, (int)this.Attributes[Stats.MaximumHealth]);

    /// <inheritdoc/>
    public override ValueTask ReflectDamageAsync(IAttacker reflector, uint damage) => ValueTask.CompletedTask;

    /// <inheritdoc />
    public override ValueTask ApplyPoisonDamageAsync(IAttacker initialAttacker, uint damage) => ValueTask.CompletedTask;

    /// <inheritdoc />
    public override ValueTask ApplyBleedingDamageAsync(IAttacker initialAttacker, uint damage) => ValueTask.CompletedTask;

    /// <inheritdoc />
    protected override bool CanBeAttackedBy(IAttacker attacker)
        => (attacker as Player ?? (attacker as IPlayerSurrogate)?.Owner) is { GuildStatus: { } status }
           && this.Obelisk.IsOpenFor(status.GuildId);
}
