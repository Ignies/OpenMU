// <copyright file="FieldBossState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

using System.Collections.Concurrent;
using System.Threading;
using MUnique.OpenMU.GameLogic.NPC;

/// <summary>
/// The running state of one field boss on one game server.
/// </summary>
public sealed class FieldBossState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldBossState"/> class.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <param name="nextChangeAt">The time of the first spawn.</param>
    internal FieldBossState(FieldBossDefinition definition, DateTime nextChangeAt)
    {
        this.Definition = definition;
        this.NextChangeAt = nextChangeAt;
    }

    /// <summary>
    /// Gets the definition.
    /// </summary>
    public FieldBossDefinition Definition { get; }

    /// <summary>
    /// Gets the status.
    /// </summary>
    public FieldBossStatus Status { get; internal set; }

    /// <summary>
    /// Gets the time of the next change: the spawn while waiting, the end of the battle while engaged, the move out
    /// after a kill. <see cref="DateTime.MaxValue"/> when nothing is due.
    /// </summary>
    public DateTime NextChangeAt { get; internal set; }

    /// <summary>
    /// Gets the time of the next spawn after a kill.
    /// </summary>
    public DateTime RespawnAt { get; internal set; }

    /// <summary>
    /// Gets the index of the current stage.
    /// </summary>
    public int StageIndex { get; internal set; }

    /// <summary>
    /// Gets the living monster of the current stage.
    /// </summary>
    public Monster? StageMonster { get; internal set; }

    /// <summary>
    /// Gets the name of the last killer.
    /// </summary>
    public string? KillerName { get; internal set; }

    /// <summary>
    /// Gets the health of the stage monster in percent.
    /// </summary>
    public byte HealthPercent
    {
        get
        {
            if (this.StageMonster is not { IsAlive: true } monster)
            {
                return 0;
            }

            var maximum = monster.Attributes[Attributes.Stats.MaximumHealth];
            return maximum <= 0 ? (byte)0 : (byte)Math.Clamp(Math.Ceiling(monster.Health * 100 / maximum), 0, 100);
        }
    }

    /// <summary>
    /// Gets every monster which this boss spawned and which may still live.
    /// </summary>
    internal ConcurrentDictionary<Monster, bool> Monsters { get; } = new();

    /// <summary>
    /// Gets the health phases of the current stage which already started.
    /// </summary>
    internal HashSet<FieldBossHealthPhase> StartedPhases { get; } = [];

    /// <summary>
    /// Gets or sets the last health bucket (tens of percent) which was shown.
    /// </summary>
    internal int ShownHealthBucket { get; set; } = 10;

    /// <summary>
    /// Gets the lock which orders the changes of this state.
    /// </summary>
    internal SemaphoreSlim Lock { get; } = new(1, 1);
}
