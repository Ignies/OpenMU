// <copyright file="FieldBossSavedState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// What the state file keeps of a field boss.
/// </summary>
/// <param name="Status">The status.</param>
/// <param name="NextChangeAt">The time of the next change.</param>
/// <param name="RespawnAt">The time of the next spawn after a kill.</param>
public sealed record FieldBossSavedState(FieldBossStatus Status, DateTime NextChangeAt, DateTime RespawnAt);
