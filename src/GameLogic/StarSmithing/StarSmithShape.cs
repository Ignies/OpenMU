// <copyright file="StarSmithShape.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// How a drawn path matches a shape.
/// </summary>
public enum StarSmithShapeMatch
{
    /// <summary>The path must be the shape, no more.</summary>
    Exact,

    /// <summary>The path may go on past the shape; each further link makes the effect bigger.</summary>
    Extendable,
}

/// <summary>
/// A recipe's constellation (section 10): the stars of the sky it links, in drawing order. A drawn path is a line from
/// star to star, so a shape is a chain of edges; an unordered shape may be drawn from either end.
/// </summary>
public class StarSmithShape
{
    /// <summary>Gets or sets the stars it links, by their index in the sky, in drawing order.</summary>
    public List<int> Stars { get; set; } = [];

    /// <summary>Gets or sets whether it must be drawn from its first star (true) or may start at either end (false).</summary>
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
    public bool IsPrefix(IReadOnlyList<int> path) => false;

    /// <summary>
    /// How far a path goes past the shape it completes.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <returns>The links past the shape (0 for the shape itself), or -1 when the path does not complete it.</returns>
    public int Extension(IReadOnlyList<int> path) => -1;

    /// <summary>
    /// Whether a path follows the shape: a prefix of it, or the whole of it and further.
    /// </summary>
    /// <param name="path">The struck stars, in order.</param>
    /// <returns>True when the path is on this shape.</returns>
    public bool Follows(IReadOnlyList<int> path) => false;
}
