// <copyright file="StarSmithShape.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A recipe's constellation (section 10): the stars of the sky it links, in drawing order. A drawn path is a line from
/// star to star, so a shape is a chain of edges; an unordered shape may be drawn from either end.
/// </summary>
public class StarSmithShape
{
    /// <summary>Gets or sets the stars it links, by their index in the sky, in drawing order.</summary>
    public List<int> Stars { get; set; } = [];

    /// <summary>Gets or sets a value indicating whether it starts at its first star; false lets it start at either end.</summary>
    public bool Ordered { get; set; } = true;

    /// <summary>Gets or sets how a path matches it.</summary>
    public StarSmithShapeMatch Match { get; set; }

    /// <summary>Gets or sets how much bigger each link past an extendable shape makes the effect, in percent.</summary>
    public int ExtensionPercent { get; set; } = 10;

    /// <summary>Gets or sets the most links past the shape that still make the effect bigger.</summary>
    public int MaximumExtension { get; set; } = 3;

    /// <summary>
    /// Whether a path is still a prefix of the shape (the shape itself included): the line glows.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <returns>True while the path follows the shape.</returns>
    public bool IsPrefix(IReadOnlyList<int> path)
        => path.Count <= this.Stars.Count && this.Begins(path, path.Count);

    /// <summary>
    /// How far a path goes past the shape it completes. An exact shape is completed only by itself.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <returns>The links past the shape (0 for the shape itself), or -1 when the path does not complete it.</returns>
    public int Extension(IReadOnlyList<int> path)
    {
        if (this.Stars.Count == 0 || path.Count < this.Stars.Count || !this.Begins(path, this.Stars.Count))
        {
            return -1;
        }

        var extra = path.Count - this.Stars.Count;
        return extra == 0 || this.Match == StarSmithShapeMatch.Extendable ? extra : -1;
    }

    /// <summary>
    /// Whether a path follows the shape: a prefix of it, or a path that completes it.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <returns>True when the path is on this shape.</returns>
    public bool Follows(IReadOnlyList<int> path) => this.IsPrefix(path) || this.Extension(path) >= 0;

    /// <summary>
    /// Whether the path's first stars run along the shape, from its first star, or from its last when unordered.
    /// </summary>
    private bool Begins(IReadOnlyList<int> path, int length)
        => this.Runs(path, length, reversed: false) || (!this.Ordered && this.Runs(path, length, reversed: true));

    private bool Runs(IReadOnlyList<int> path, int length, bool reversed)
    {
        var last = this.Stars.Count - 1;
        for (var i = 0; i < length; i++)
        {
            if (path[i] != this.Stars[reversed ? last - i : i])
            {
                return false;
            }
        }

        return true;
    }
}
