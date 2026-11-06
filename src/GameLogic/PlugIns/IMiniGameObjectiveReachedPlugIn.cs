// <copyright file="IMiniGameObjectiveReachedPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// A plugin interface which is called when a player reached an objective of a mini game, e.g. a rank or a relic.
/// </summary>
[Guid("A3E95C27-4B81-4D60-9F2E-7C1D8B6A5F04")]
[PlugInPoint("Mini game objective reached", "Plugins which will be executed when a player reached an objective of a mini game, e.g. a rank at its end or a relic.")]
public interface IMiniGameObjectiveReachedPlugIn
{
    /// <summary>
    /// Is called when a player reached an objective of a mini game.
    /// </summary>
    /// <param name="miniGame">The mini game.</param>
    /// <param name="player">The player.</param>
    /// <param name="objective">The objective.</param>
    /// <param name="value">The value of the objective, e.g. the rank; 0 when it has none.</param>
    /// <returns>The task.</returns>
    ValueTask ObjectiveReachedAsync(MiniGameContext miniGame, Player player, MiniGameObjective objective, int value);
}
