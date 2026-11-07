// <copyright file="ArcaArea.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// A rectangle of tiles on a map; the corners may come in any order.
/// </summary>
public class ArcaArea
{
    /// <summary>
    /// Gets or sets the first x coordinate.
    /// </summary>
    public byte StartX { get; set; }

    /// <summary>
    /// Gets or sets the first y coordinate.
    /// </summary>
    public byte StartY { get; set; }

    /// <summary>
    /// Gets or sets the second x coordinate.
    /// </summary>
    public byte EndX { get; set; }

    /// <summary>
    /// Gets or sets the second y coordinate.
    /// </summary>
    public byte EndY { get; set; }

    /// <summary>
    /// Gets the rectangle with the lower coordinates first.
    /// </summary>
    /// <returns>The rectangle.</returns>
    public (byte X1, byte Y1, byte X2, byte Y2) Normalized()
        => (Math.Min(this.StartX, this.EndX), Math.Min(this.StartY, this.EndY), Math.Max(this.StartX, this.EndX), Math.Max(this.StartY, this.EndY));
}
