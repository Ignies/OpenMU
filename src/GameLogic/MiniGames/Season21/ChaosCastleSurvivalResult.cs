// <copyright file="ChaosCastleSurvivalResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// The result of a character in a game of the Chaos Castle Survival.
/// </summary>
/// <param name="CharacterId">The id of the character.</param>
/// <param name="MonsterKills">The killed monsters.</param>
/// <param name="PlayerKills">The killed players.</param>
/// <param name="IsWinner">If set to <c>true</c>, the character won the game.</param>
public readonly record struct ChaosCastleSurvivalResult(Guid CharacterId, int MonsterKills, int PlayerKills, bool IsWinner);
