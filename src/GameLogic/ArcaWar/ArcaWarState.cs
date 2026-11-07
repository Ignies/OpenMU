// <copyright file="ArcaWarState.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The state of the Arca War which outlasts a restart: the obelisks the guilds held at the end of the last wars.
/// </summary>
public class ArcaWarState
{
    /// <summary>
    /// Gets or sets the holdings.
    /// </summary>
    public List<ArcaHolding> Holdings { get; set; } = [];
}
