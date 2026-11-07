// <copyright file="ArcaMonsterGroup.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// A group of monsters around an obelisk (S21Emu <c>MonsterGroupInformation</c>, <c>MonsterGroupSpot</c> and
/// <c>MonsterGroupMember</c>).
/// </summary>
public class ArcaMonsterGroup
{
    /// <summary>
    /// Gets or sets the map.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of monsters the group has after each regeneration.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the minutes between the regenerations of the group.
    /// </summary>
    public int RegenMinutes { get; set; }

    /// <summary>
    /// Gets or sets the numbers of the monsters, of which each spawn picks one.
    /// </summary>
    public List<short> Monsters { get; set; } = [];

    /// <summary>
    /// Gets or sets the spots, of which each spawn picks one.
    /// </summary>
    public List<ArcaArea> Spots { get; set; } = [];
}
