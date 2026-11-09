// <copyright file="GuideQuestView.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.GuideQuests;

/// <summary>
/// What the player is shown of its guide quests.
/// </summary>
/// <param name="Status">The status.</param>
/// <param name="Step">The current or next step; <c>null</c> when there is none.</param>
/// <param name="Index">The place of the step in the chain of the class, from 1.</param>
/// <param name="Count">The number of steps in the chain of the class.</param>
/// <param name="Progress">How far the objective is: kills, items, the level, or 1 when a move or class quest is done.</param>
public readonly record struct GuideQuestView(GuideQuestStatus Status, GuideQuestStep? Step, int Index, int Count, int Progress);
