// <copyright file="ArcaSpawnGroup.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The places where the members of the guilds of a group enter the war (S21Emu <c>PlayerSpawnSettings</c>).
/// </summary>
public class ArcaSpawnGroup
{
    /// <summary>
    /// Gets or sets the map.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the areas, of which each entry picks one.
    /// </summary>
    public List<ArcaArea> Areas { get; set; } = [];
}
