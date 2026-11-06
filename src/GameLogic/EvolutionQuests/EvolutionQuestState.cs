// <copyright file="EvolutionQuestState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EvolutionQuests;

using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;

/// <summary>
/// The state of a character towards its next evolution with a quest.
/// </summary>
/// <param name="NextEvolution">The evolution of the next class, or <c>null</c> when the class has none or no quest leads there.</param>
/// <param name="RequiredLevel">The level which the evolution asks for, in the unit of <paramref name="LevelKey"/>.</param>
/// <param name="LevelKey">The level which the requirement is compared with.</param>
/// <param name="RuleMet">A value indicating whether the character meets the level rule.</param>
/// <param name="ExperienceHeld">A value indicating whether the character gains no experience until it evolves.</param>
public readonly record struct EvolutionQuestState(ClassEvolution? NextEvolution, int RequiredLevel, ProgressionLevelKey LevelKey, bool RuleMet, bool ExperienceHeld);
