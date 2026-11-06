// <copyright file="EliteZoneDefinition.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.EliteZones;

using MUnique.OpenMU.Pathfinding;

/// <summary>
/// One elite zone: an area of a map whose elites spawn inside it.
/// </summary>
public class EliteZoneDefinition
{
    /// <summary>
    /// Gets or sets the number of the map.
    /// </summary>
    public short MapNumber { get; set; }

    /// <summary>
    /// Gets or sets the element of the zone.
    /// </summary>
    public EliteZoneElement Element { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate of the entrance, as Webzen publishes it.
    /// </summary>
    public byte EntranceX { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate of the entrance, as Webzen publishes it.
    /// </summary>
    public byte EntranceY { get; set; }

    /// <summary>
    /// Gets or sets the lowest x coordinate of the area.
    /// </summary>
    public byte X1 { get; set; }

    /// <summary>
    /// Gets or sets the lowest y coordinate of the area.
    /// </summary>
    public byte Y1 { get; set; }

    /// <summary>
    /// Gets or sets the highest x coordinate of the area.
    /// </summary>
    public byte X2 { get; set; }

    /// <summary>
    /// Gets or sets the highest y coordinate of the area.
    /// </summary>
    public byte Y2 { get; set; }

    /// <summary>
    /// Determines whether the area contains a point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns><c>true</c>, if the point is inside the area.</returns>
    public bool Contains(Point point) => point.X >= this.X1 && point.X <= this.X2 && point.Y >= this.Y1 && point.Y <= this.Y2;

    /// <inheritdoc />
    public override string ToString() => $"{this.MapNumber} {this.Element} ({this.EntranceX}, {this.EntranceY})";
}
