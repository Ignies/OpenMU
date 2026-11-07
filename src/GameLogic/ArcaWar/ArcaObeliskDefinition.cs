// <copyright file="ArcaObeliskDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// An obelisk of the Arca War (S21Emu <c>ObeliskInformation</c> and <c>AuraPositionSettings</c>).
/// </summary>
public class ArcaObeliskDefinition
{
    /// <summary>
    /// Gets or sets the index (1-based in the data).
    /// </summary>
    public byte Index { get; set; }

    /// <summary>
    /// Gets or sets the map.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of the monster of the obelisk (598, Fire Tower).
    /// </summary>
    public short MonsterNumber { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate.
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate.
    /// </summary>
    public byte Y { get; set; }

    /// <summary>
    /// Gets or sets the health at the start of the war.
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Gets or sets the health of the obelisk once a guild holds it.
    /// </summary>
    public int OccupiedHealth { get; set; }

    /// <summary>
    /// Gets or sets the seconds until a destroyed obelisk comes back.
    /// </summary>
    public int RegenSeconds { get; set; }

    /// <summary>
    /// Gets or sets the element; <see cref="ArcaElement.None"/> picks one at random for every war.
    /// </summary>
    public ArcaElement Element { get; set; }

    /// <summary>
    /// Gets or sets the three auras (barriers) around the obelisk.
    /// </summary>
    public List<ArcaPosition> Auras { get; set; } = [];
}
