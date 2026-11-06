// <copyright file="IFieldBossStateViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.FieldBosses;

/// <summary>
/// Interface of a view whose implementation shows the state of a field boss of the current map.
/// </summary>
public interface IFieldBossStateViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the state of a field boss.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="bossNumber">The monster number of the boss.</param>
    /// <param name="status">The status.</param>
    /// <param name="healthPercent">The health of the current stage monster in percent, 0 when none lives.</param>
    /// <param name="secondsLeft">The seconds until the next change (spawn, end of the battle, move out); 0 when unknown.</param>
    ValueTask ShowFieldBossStateAsync(short mapNumber, short bossNumber, FieldBossStatus status, byte healthPercent, uint secondsLeft);
}
