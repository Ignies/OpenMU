// <copyright file="LabyrinthMissionProgress.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The progress of the mission of a room of the Labyrinth of Dimensions, from entering the room.
/// </summary>
/// <remarks>
/// The timed goals (<see cref="LabyrinthGoalKind.KillInTime"/>, <see cref="LabyrinthGoalKind.NoHealthPotion"/>,
/// <see cref="LabyrinthGoalKind.NoManaPotion"/>) have minutes as their target, as the comment of S21Emu
/// <c>MissionSettings</c> says; they are met when the monsters of the room (the count of the mission) are killed in that
/// time. A forbidden potion or the end of the time starts the mission again. Once done, a mission stays done.
/// Not thread safe; <see cref="LabyrinthRun"/> serializes the calls.
/// </remarks>
public sealed class LabyrinthMissionProgress
{
    private readonly IReadOnlyList<LabyrinthGoal> _goals;

    private readonly int _monsterCount;

    private DateTime _startedAt;

    private long _damage;

    private long _helperSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="LabyrinthMissionProgress"/> class.
    /// </summary>
    /// <param name="mission">The mission.</param>
    /// <param name="monsterCount">The number of monsters of the room, which the timed goals have to kill.</param>
    /// <param name="startedAt">The time the character entered the room.</param>
    public LabyrinthMissionProgress(LabyrinthMission mission, int monsterCount, DateTime startedAt)
    {
        this._goals = mission.Goals.Where(g => g.Kind != LabyrinthGoalKind.None).ToList();
        this._monsterCount = Math.Max(monsterCount, 1);
        this._startedAt = startedAt;
        this.IsComplete = this._goals.Count == 0;
    }

    /// <summary>
    /// Gets the kills since the mission (re)started.
    /// </summary>
    public int Kills { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the mission is done.
    /// </summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Gets the goals and their progress.
    /// </summary>
    public IReadOnlyList<LabyrinthGoalProgress> Goals => this._goals.Select(g => new LabyrinthGoalProgress(g.Kind, g.Target, this.GetProgress(g))).ToList();

    /// <summary>
    /// Counts a kill of a monster of the room.
    /// </summary>
    public void AddKill()
    {
        if (!this.IsComplete)
        {
            this.Kills++;
            this.Evaluate();
        }
    }

    /// <summary>
    /// Counts damage on a monster of the room.
    /// </summary>
    /// <param name="damage">The damage.</param>
    public void AddDamage(long damage)
    {
        if (!this.IsComplete && damage > 0)
        {
            this._damage += damage;
            this.Evaluate();
        }
    }

    /// <summary>
    /// Counts a second of the MU Helper in the room.
    /// </summary>
    public void AddHelperSecond()
    {
        if (!this.IsComplete)
        {
            this._helperSeconds++;
            this.Evaluate();
        }
    }

    /// <summary>
    /// Counts a used potion; a kind the mission forbids starts the mission again.
    /// </summary>
    /// <param name="health"><c>true</c> for a health potion, <c>false</c> for a mana potion.</param>
    /// <param name="now">The current time.</param>
    /// <returns><c>true</c>, if the mission started again.</returns>
    public bool UsedPotion(bool health, DateTime now)
    {
        var forbidden = health ? LabyrinthGoalKind.NoHealthPotion : LabyrinthGoalKind.NoManaPotion;
        if (this.IsComplete || this._goals.All(g => g.Kind != forbidden))
        {
            return false;
        }

        this.Restart(now);
        return true;
    }

    /// <summary>
    /// Starts the mission again when the time of a timed goal ran out.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <returns><c>true</c>, if the mission started again.</returns>
    public bool Tick(DateTime now)
    {
        if (this.IsComplete || !this._goals.Where(IsTimed).Any(g => now - this._startedAt >= TimeSpan.FromMinutes(g.Target)))
        {
            return false;
        }

        this.Restart(now);
        return true;
    }

    private static bool IsTimed(LabyrinthGoal goal)
        => goal.Kind is LabyrinthGoalKind.KillInTime or LabyrinthGoalKind.NoHealthPotion or LabyrinthGoalKind.NoManaPotion;

    private long GetProgress(LabyrinthGoal goal) => goal.Kind switch
    {
        LabyrinthGoalKind.Damage => this._damage,
        LabyrinthGoalKind.HelperTime => this._helperSeconds,
        LabyrinthGoalKind.Experience => goal.Target,
        _ => this.Kills,
    };

    private bool IsMet(LabyrinthGoal goal) => IsTimed(goal)
        ? this.Kills >= this._monsterCount
        : this.GetProgress(goal) >= goal.Target;

    private void Evaluate()
    {
        if (this._goals.All(this.IsMet))
        {
            this.IsComplete = true;
        }
    }

    private void Restart(DateTime now)
    {
        this.Kills = 0;
        this._damage = 0;
        this._helperSeconds = 0;
        this._startedAt = now;
    }
}
