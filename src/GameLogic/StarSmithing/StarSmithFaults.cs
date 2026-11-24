// <copyright file="StarSmithFaults.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A fault line: a short segment in the item's box; a node on it cracks the attempt.
/// </summary>
/// <param name="A">One end.</param>
/// <param name="B">The other end.</param>
public readonly record struct StarSmithFault(StarSmithPoint A, StarSmithPoint B);

/// <summary>
/// The fault lines of an attempt (Readme.md "PR 7a contract", "Fault lines"), dealt from its seed.
/// </summary>
public static class StarSmithFaults
{
    /// <summary>
    /// Deals the faults: per fault x1, y1 inside the margin, an angle, a length, the other end clamped to the box; then
    /// the one a Jewel of Guardian reveals. The order of the draws is part of the contract.
    /// </summary>
    /// <param name="seed">The attempt's seed.</param>
    /// <param name="rules">The rules.</param>
    /// <param name="guarded">Whether a Jewel of Guardian is among the catalysts.</param>
    /// <returns>The faults and the revealed one's index (-1 none).</returns>
    public static (IReadOnlyList<StarSmithFault> Faults, int Revealed) Deal(long seed, StarSmithForgeRules rules, bool guarded)
    {
        var dice = new StarSmithDice(seed);
        var margin = Math.Clamp(rules.FaultMargin, 0, 0.49);
        var span = 1 - (2 * margin);
        var shortest = Math.Max(0, Math.Min(rules.FaultMinLength, rules.FaultMaxLength));
        var longest = Math.Max(shortest, rules.FaultMaxLength);
        var count = Math.Clamp(rules.FaultCount, 0, StarSmithForgeRules.MostFaults);
        var faults = new List<StarSmithFault>(count);
        for (var i = 0; i < count; i++)
        {
            var x1 = margin + (dice.NextDouble() * span);
            var y1 = margin + (dice.NextDouble() * span);
            var angle = dice.NextDouble() * Math.PI;
            var length = shortest + (dice.NextDouble() * (longest - shortest));
            var x2 = Math.Clamp(x1 + (length * Math.Cos(angle)), 0, 1);
            var y2 = Math.Clamp(y1 + (length * Math.Sin(angle)), 0, 1);
            faults.Add(new StarSmithFault(new(StarSmithPoint.ToBox(x1), StarSmithPoint.ToBox(y1)), new(StarSmithPoint.ToBox(x2), StarSmithPoint.ToBox(y2))));
        }

        var revealed = guarded && count > 0 ? dice.Next(count) : -1;
        return (faults, revealed);
    }

    /// <summary>
    /// Whether a point lies within a radius of a fault segment.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="fault">The fault.</param>
    /// <param name="radius">The radius, in the unit square.</param>
    /// <returns>True when it hits.</returns>
    public static bool Hits(StarSmithPoint point, StarSmithFault fault, double radius)
    {
        var (px, py) = point.Unit;
        var (ax, ay) = fault.A.Unit;
        var (bx, by) = fault.B.Unit;
        var dx = bx - ax;
        var dy = by - ay;
        var lengthSquared = (dx * dx) + (dy * dy);
        var t = lengthSquared <= 0 ? 0 : Math.Clamp((((px - ax) * dx) + ((py - ay) * dy)) / lengthSquared, 0, 1);
        var cx = ax + (t * dx) - px;
        var cy = ay + (t * dy) - py;
        return Math.Sqrt((cx * cx) + (cy * cy)) <= radius;
    }
}
