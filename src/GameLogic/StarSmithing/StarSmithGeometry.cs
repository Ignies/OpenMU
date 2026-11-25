// <copyright file="StarSmithGeometry.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Text.Json.Serialization;

/// <summary>
/// A constellation's shape (design section 14, Readme.md "PR 7a contract"): its node count, the turn at each inner
/// corner (signed degrees, left positive), each segment's length over the first, whether it closes, and an optional
/// extension. Any rotation and size, never mirrored, drawn from its first node forwards.
/// </summary>
public class StarSmithGeometry
{
    /// <summary>The least turn difference that keeps two shapes apart: twice the miss.</summary>
    public const double ApartDegrees = 30;

    /// <summary>The least length factor that keeps two shapes apart.</summary>
    public const double ApartFactor = 1.5;

    /// <summary>Gets or sets the corners of the shape.</summary>
    public int Nodes { get; set; }

    /// <summary>Gets or sets a value indicating whether the last corner joins the first (one more placement, back on the first node).</summary>
    public bool Closed { get; set; }

    /// <summary>Gets or sets the turn at each inner corner, in degrees, left positive: n - 2 open, n - 1 closed.</summary>
    public List<double> Turns { get; set; } = [];

    /// <summary>Gets or sets each segment's length over the first: n - 1 open, n closed; the first is 1.</summary>
    public List<double> Lengths { get; set; } = [];

    /// <summary>Gets or sets the extension Chroma needs, or null.</summary>
    public StarSmithExtension? Extension { get; set; }

    /// <summary>Gets the base placements: the nodes, and one more to close a closed shape.</summary>
    [JsonIgnore]
    public int Placements => this.Closed ? this.Nodes + 1 : this.Nodes;

    /// <summary>Gets a value indicating whether the counts fit the node count and every length is positive.</summary>
    [JsonIgnore]
    public bool IsWellFormed
        => this.Nodes >= 3
           && this.Turns.Count == (this.Closed ? this.Nodes - 1 : this.Nodes - 2)
           && this.Lengths.Count == (this.Closed ? this.Nodes : this.Nodes - 1)
           && this.Lengths.All(l => l > 0)
           && this.Extension is not { Length: <= 0 };

    /// <summary>
    /// A shape with the default extension: one more node turning 90 degrees against its last turn, at half the first
    /// length.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <param name="closed">Whether it closes.</param>
    /// <param name="turns">The turns.</param>
    /// <param name="lengths">The lengths.</param>
    /// <returns>The shape.</returns>
    public static StarSmithGeometry Of(int nodes, bool closed, double[] turns, double[] lengths) => new()
    {
        Nodes = nodes,
        Closed = closed,
        Turns = [.. turns],
        Lengths = [.. lengths],
        Extension = new StarSmithExtension { Turn = turns.Length > 0 && turns[^1] >= 0 ? -90 : 90, Length = 0.5 },
    };

    /// <summary>
    /// Whether no figure can match both shapes: their placement counts differ, or at some measured placement their
    /// turns differ by 30 degrees or more or their lengths by a factor of 1.5 or more.
    /// </summary>
    /// <param name="a">One shape.</param>
    /// <param name="aExtended">Whether it is taken with its extension.</param>
    /// <param name="b">The other shape.</param>
    /// <param name="bExtended">Whether it is taken with its extension.</param>
    /// <returns>True when apart.</returns>
    public static bool Apart(StarSmithGeometry a, bool aExtended, StarSmithGeometry b, bool bExtended)
    {
        var left = a.Targets(aExtended);
        var right = b.Targets(bExtended);
        if (left is null || right is null || left.Count != right.Count)
        {
            return true;
        }

        for (var i = 0; i < left.Count; i++)
        {
            var high = Math.Max(left[i].Length, right[i].Length);
            var low = Math.Min(left[i].Length, right[i].Length);
            if (Math.Abs(StarSmithAccuracy.Wrap(left[i].Turn - right[i].Turn)) >= ApartDegrees || high >= ApartFactor * low)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The target turn of a measured placement (2 to the placements, the last being the extension).
    /// </summary>
    /// <param name="placement">The placement.</param>
    /// <returns>The turn, in degrees.</returns>
    public double TurnAt(int placement) => placement == this.Placements ? this.Extension!.Turn : this.Turns[placement - 2];

    /// <summary>
    /// The target length ratio of a placement's segment (1 to the placements, the last being the extension).
    /// </summary>
    /// <param name="placement">The placement.</param>
    /// <returns>The length over the first.</returns>
    public double LengthAt(int placement) => placement == this.Placements ? this.Extension!.Length : this.Lengths[placement - 1];

    /// <summary>
    /// The targets of the measured placements (from placement 2), or null when the shape is not well formed or has no
    /// extension to take.
    /// </summary>
    private List<(double Turn, double Length)>? Targets(bool extended)
    {
        if (!this.IsWellFormed || (extended && this.Extension is null))
        {
            return null;
        }

        var count = this.Placements + (extended ? 1 : 0);
        var targets = new List<(double Turn, double Length)>();
        for (var k = 2; k < count; k++)
        {
            targets.Add((this.TurnAt(k), this.LengthAt(k)));
        }

        return targets;
    }
}
