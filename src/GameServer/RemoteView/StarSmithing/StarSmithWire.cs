// <copyright file="StarSmithWire.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.StarSmithing;

using System.Buffers.Binary;
using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The byte layouts Star Smithing's packets share (Readme.md "PR 7a contract", "Wire"): points, hulls and layers.
/// </summary>
public static class StarSmithWire
{
    /// <summary>The bytes of one layer: number (2), tier, look, point count, 8 points of 4 bytes.</summary>
    public const int LayerBytes = 37;

    /// <summary>The bytes of a hull's 8 points.</summary>
    public const int HullBytes = 4 * StarSmithHull.MaximumPoints;

    /// <summary>
    /// Writes a point: x then y, u16 little endian each.
    /// </summary>
    /// <param name="target">Where.</param>
    /// <param name="point">The point.</param>
    public static void WritePoint(Span<byte> target, StarSmithPoint point)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(target, point.X);
        BinaryPrimitives.WriteUInt16LittleEndian(target[2..], point.Y);
    }

    /// <summary>
    /// Writes up to 8 points.
    /// </summary>
    /// <param name="target">Where, 32 bytes.</param>
    /// <param name="points">The points.</param>
    /// <returns>How many were written.</returns>
    public static int WritePoints(Span<byte> target, IReadOnlyList<StarSmithPoint> points)
    {
        var count = Math.Min(points.Count, StarSmithHull.MaximumPoints);
        for (var i = 0; i < count; i++)
        {
            WritePoint(target[(i * 4)..], points[i]);
        }

        return count;
    }

    /// <summary>
    /// Writes up to 4 layers of 37 bytes.
    /// </summary>
    /// <param name="target">Where, 148 bytes.</param>
    /// <param name="layers">The layers.</param>
    /// <returns>How many were written.</returns>
    public static int WriteLayers(Span<byte> target, IReadOnlyList<StarSmithLayer> layers)
    {
        var count = Math.Min(layers.Count, StarSmithSlots.Maximum);
        for (var i = 0; i < count; i++)
        {
            var at = target.Slice(i * LayerBytes, LayerBytes);
            BinaryPrimitives.WriteUInt16LittleEndian(at, (ushort)layers[i].Number);
            at[2] = (byte)layers[i].Tier;
            at[3] = layers[i].Look;
            at[4] = (byte)WritePoints(at.Slice(5, HullBytes), layers[i].Hull);
        }

        return count;
    }
}
