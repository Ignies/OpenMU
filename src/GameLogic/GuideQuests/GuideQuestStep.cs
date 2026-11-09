// <copyright file="GuideQuestStep.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Rewards;

/// <summary>
/// One step of the guide quests (a row of S21Emu <c>QuestGuide.txt</c>).
/// </summary>
public class GuideQuestStep
{
    /// <summary>
    /// Gets or sets the number of the step; the steps of a class follow each other in the order of the configuration.
    /// </summary>
    public short Number { get; set; }

    /// <summary>
    /// Gets or sets the name, as the client shows it.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chapter of the guide the step belongs to (1-4).
    /// </summary>
    public byte Chapter { get; set; }

    /// <summary>
    /// Gets or sets the level from which the step can be accepted.
    /// </summary>
    public int MinimumLevel { get; set; }

    /// <summary>
    /// Gets or sets the base classes which get the step; empty for every class.
    /// </summary>
    public List<BaseClassCode> Classes { get; set; } = [];

    /// <summary>
    /// Gets or sets what the step asks for.
    /// </summary>
    public GuideQuestObjective Objective { get; set; }

    /// <summary>
    /// Gets or sets the monster number, the level, the class quest number, or the item group of the objective.
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// Gets or sets the item number of a collect objective.
    /// </summary>
    public short TargetNumber { get; set; }

    /// <summary>
    /// Gets or sets the kills or items the objective asks for.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the map of the step's spot: where to go, or where the monsters are.
    /// </summary>
    public short Map { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate of the spot.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate of the spot.
    /// </summary>
    public byte Y { get; set; }

    /// <summary>
    /// Gets or sets the warp gate of the data which leads near the spot.
    /// </summary>
    public short Gate { get; set; }

    /// <summary>
    /// Gets or sets the experience reward.
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Gets or sets the money reward.
    /// </summary>
    public int Money { get; set; }

    /// <summary>
    /// Gets or sets the item rewards.
    /// </summary>
    public List<RewardItem> Items { get; set; } = [];

    /// <summary>
    /// Determines whether a character of the base class gets the step.
    /// </summary>
    /// <param name="baseClass">The base class; <c>null</c> when it has none.</param>
    /// <returns><c>true</c>, if the class gets it.</returns>
    public bool IsFor(BaseClassCode? baseClass) => this.Classes.Count == 0 || (baseClass is { } code && this.Classes.Contains(code));

    /// <summary>
    /// Gets what the progress has to reach: the kills or items, the level, or 1 for a move or a class quest.
    /// </summary>
    /// <returns>The required progress.</returns>
    public int GetRequiredProgress() => this.Objective switch
    {
        GuideQuestObjective.ReachLevel => this.Target,
        GuideQuestObjective.HuntMonster or GuideQuestObjective.CollectItem => Math.Max(1, this.Count),
        _ => 1,
    };
}
