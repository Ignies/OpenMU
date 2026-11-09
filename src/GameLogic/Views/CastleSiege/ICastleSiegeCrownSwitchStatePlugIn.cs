// <copyright file="ICastleSiegeCrownSwitchStatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.CastleSiege;

/// <summary>
/// A view which answers a player who talks to or held a Castle Siege Crown switch.
/// </summary>
public interface ICastleSiegeCrownSwitchStatePlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the player's access to a Crown switch.
    /// </summary>
    /// <param name="switchId">The network object identifier of the switch.</param>
    /// <param name="occupant">The other player who pushes the switch, if <paramref name="access"/> is <see cref="CastleSiegeCrownSwitchAccess.PushedByOther"/>.</param>
    /// <param name="access">The access.</param>
    /// <returns>A task that represents the asynchronous show operation.</returns>
    ValueTask ShowCrownSwitchStateAsync(ushort switchId, Player? occupant, CastleSiegeCrownSwitchAccess access);
}
