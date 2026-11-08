// <copyright file="ICastleSiegeBattleStatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.CastleSiege;

/// <summary>
/// A view which tells a player on the castle map whether the battle runs and how long it has left.
/// </summary>
public interface ICastleSiegeBattleStatePlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows that the battle started or ended.
    /// </summary>
    /// <param name="isStarted">Whether the battle runs.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask ShowBattleStateAsync(bool isStarted);

    /// <summary>
    /// Shows the remaining battle time.
    /// </summary>
    /// <param name="hours">The remaining hours.</param>
    /// <param name="minutes">The remaining minutes of the hour.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask ShowRemainingTimeAsync(byte hours, byte minutes);
}
