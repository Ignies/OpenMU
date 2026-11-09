// <copyright file="GameMasterItemExtras.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Administration;

/// <summary>
/// The options of a game master's item beyond the <see cref="Interfaces.ItemSpecification"/>: the ones of the later
/// seasons, see <see cref="GameMasterItemOptions"/>.
/// </summary>
/// <param name="Sockets">The number of sockets; above the maximum of the item its maximum.</param>
/// <param name="Seeds">The seeds of the sockets in order, <c>index[:level]</c> separated by commas.</param>
/// <param name="Errtels">The errtels of a pentagram, <c>socket:ranks[:level]</c> separated by commas.</param>
/// <param name="Options">More options of the item, <c>kind:number[:level]</c> separated by commas.</param>
/// <param name="MuunOptionDays">The days of the additional option of a Muun; 0 for none.</param>
public sealed record GameMasterItemExtras(byte Sockets = byte.MaxValue, string? Seeds = null, string? Errtels = null, string? Options = null, byte MuunOptionDays = 0);
