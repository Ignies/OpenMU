// <copyright file="GuideQuestObjective.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

/// <summary>
/// What a guide quest step asks for; the values are the objective kinds of S21Emu <c>QuestGuide.txt</c>.
/// </summary>
public enum GuideQuestObjective
{
    /// <summary>
    /// Not defined.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Carry a number of items (<see cref="GuideQuestStep.Target"/> group, <see cref="GuideQuestStep.TargetNumber"/> number); the completion takes them.
    /// </summary>
    CollectItem = 1,

    /// <summary>
    /// Kill a number of monsters of the number <see cref="GuideQuestStep.Target"/>.
    /// </summary>
    HuntMonster = 2,

    /// <summary>
    /// Reach the level <see cref="GuideQuestStep.Target"/>.
    /// </summary>
    ReachLevel = 3,

    /// <summary>
    /// Finish the Season 6 class quest of the number <see cref="GuideQuestStep.Target"/> (1 second class, 2 hero status, 6 third class).
    /// </summary>
    CompleteQuest = 4,

    /// <summary>
    /// Go to the spot of the step on its map.
    /// </summary>
    MoveTo = 5,
}
