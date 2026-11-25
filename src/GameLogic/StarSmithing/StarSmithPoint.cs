// <copyright file="StarSmithPoint.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A point in the item's picture box, in box units: 0 to 65535 from left to right and from top to bottom.
/// </summary>
/// <param name="X">From the left.</param>
/// <param name="Y">From the top.</param>
public readonly record struct StarSmithPoint(ushort X, ushort Y)
{
    /// <summary>The box units of the whole box.</summary>
    public const double Box = 65535.0;

    /// <summary>Gets the point in the unit square.</summary>
    public (double X, double Y) Unit => (this.X / Box, this.Y / Box);

    /// <summary>
    /// A unit-square value in box units, rounded.
    /// </summary>
    /// <param name="value">The value, 0 to 1.</param>
    /// <returns>The box units.</returns>
    public static ushort ToBox(double value) => (ushort)Math.Round(Math.Clamp(value, 0, 1) * Box, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The distance to another point, in the unit square.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The distance.</returns>
    public double DistanceTo(StarSmithPoint other)
    {
        var dx = (this.X - other.X) / Box;
        var dy = (this.Y - other.Y) / Box;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
