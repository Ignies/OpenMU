// <copyright file="StarSmithFigureAnswer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What an answer of EE 9E answers.
/// </summary>
public enum StarSmithFigureKind : byte
{
    /// <summary>A node.</summary>
    Node,

    /// <summary>A stash.</summary>
    Stash,
}

/// <summary>
/// The answer to a node or a stash (EE 9E byte 5).
/// </summary>
public enum StarSmithFigureResult : byte
{
    /// <summary>The node is placed, or the figure banked.</summary>
    Done,

    /// <summary>The node hit a fault: the attempt cracked.</summary>
    Cracked,

    /// <summary>No attempt is running.</summary>
    NoAttempt,

    /// <summary>No strike is left.</summary>
    NoStrikesLeft,

    /// <summary>The node is too near a node of the attempt.</summary>
    TooNear,

    /// <summary>The figure is closed and has its extension.</summary>
    FigureFull,

    /// <summary>The figure matches no constellation.</summary>
    NoMatch,

    /// <summary>The figure's hull overlaps a used hull.</summary>
    Overlap,

    /// <summary>No slot is free.</summary>
    NoSlot,
}

/// <summary>
/// The flags of EE 9E byte 8.
/// </summary>
[Flags]
public enum StarSmithFigureFlags : byte
{
    /// <summary>Nothing.</summary>
    None = 0,

    /// <summary>The node closed the figure.</summary>
    Closed = 1,

    /// <summary>The node hit a fault.</summary>
    Fault = 2,

    /// <summary>The figure's hull overlaps a used hull.</summary>
    Overlaps = 4,

    /// <summary>The match is a first find.</summary>
    Found = 8,

    /// <summary>The match counts its extension.</summary>
    Extended = 16,

    /// <summary>The match is a reseal of the item's layer of the same constellation.</summary>
    Reseal = 32,
}

/// <summary>
/// The answer to a node or a stash (EE 9E).
/// </summary>
/// <param name="Kind">Node or stash.</param>
/// <param name="Result">The answer.</param>
/// <param name="Timing">The node's timing, or null.</param>
/// <param name="StrikesLeft">The strikes left.</param>
/// <param name="Flags">The flags.</param>
/// <param name="FigurePlacements">The figure's placements (0 after a stash).</param>
/// <param name="Banked">The banked figures.</param>
/// <param name="Match">The constellation the figure matches (on a stash, the banked one), or null.</param>
/// <param name="Measure">The match's measure, or null.</param>
/// <param name="Turn">The node's measured turn, in degrees.</param>
/// <param name="Ratio">The node's measured length ratio.</param>
/// <param name="Guide">The known constellations the figure is a prefix of, by number, at most 8.</param>
/// <param name="Placements">Per placement of the figure: its timing and its accuracy against the match, else the first guide, else null.</param>
/// <param name="Hull">The figure's hull (on a stash the banked one's).</param>
public sealed record StarSmithFigureAnswer(
    StarSmithFigureKind Kind,
    StarSmithFigureResult Result,
    StarSmithTiming? Timing,
    int StrikesLeft,
    StarSmithFigureFlags Flags,
    int FigurePlacements,
    int Banked,
    StarSmithRecipe? Match,
    StarSmithMeasure? Measure,
    double Turn,
    double Ratio,
    IReadOnlyList<short> Guide,
    IReadOnlyList<(StarSmithTiming Timing, int? Accuracy)> Placements,
    IReadOnlyList<StarSmithPoint> Hull)
{
    /// <summary>
    /// A refusal: nothing but the counts.
    /// </summary>
    /// <param name="kind">Node or stash.</param>
    /// <param name="result">The refusal.</param>
    /// <param name="strikesLeft">The strikes left.</param>
    /// <param name="figure">The figure's placements.</param>
    /// <param name="banked">The banked figures.</param>
    /// <returns>The answer.</returns>
    public static StarSmithFigureAnswer Refused(StarSmithFigureKind kind, StarSmithFigureResult result, int strikesLeft, int figure, int banked)
        => new(kind, result, null, strikesLeft, StarSmithFigureFlags.None, figure, banked, null, null, 0, 0, [], [], []);
}
