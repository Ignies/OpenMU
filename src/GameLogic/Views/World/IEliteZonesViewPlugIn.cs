// <copyright file="IEliteZonesViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.EliteZones;

/// <summary>
/// Interface of a view whose implementation shows the elite zones of the current map.
/// </summary>
public interface IEliteZonesViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the elite zones of the current map.
    /// </summary>
    /// <param name="mapNumber">The map number.</param>
    /// <param name="zones">The zones and whether the player may enter each.</param>
    ValueTask ShowEliteZonesAsync(short mapNumber, IReadOnlyList<(EliteZoneDefinition Zone, bool CanEnter)> zones);
}
