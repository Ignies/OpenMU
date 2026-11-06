// <copyright file="SeasonPassMissionState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// The progress of a mission.
/// </summary>
/// <param name="Index">The index; a special mission has 128 + its index.</param>
/// <param name="Completions">The completions of the day (a special mission: of the season).</param>
/// <param name="Progress">The progress towards the next completion.</param>
public sealed record SeasonPassMissionState(int Index, int Completions, long Progress);
