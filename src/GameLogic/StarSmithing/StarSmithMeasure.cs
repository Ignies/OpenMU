// <copyright file="StarSmithMeasure.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A figure measured against one shape (<see cref="StarSmithAccuracy.Measure"/>).
/// </summary>
/// <param name="Matches">Whether the figure is the shape (with or without its extension).</param>
/// <param name="Prefix">Whether the figure is still on the way to the shape (3 placements or more, all fitting).</param>
/// <param name="Extended">Whether the match counts the extension.</param>
/// <param name="Accuracy">Each placement's accuracy, 0 to 1000 (1000 for placements 0 and 1).</param>
/// <param name="Score">The score of a match, 0 to 1000.</param>
/// <param name="Tier">The tier of a match.</param>
public sealed record StarSmithMeasure(bool Matches, bool Prefix, bool Extended, IReadOnlyList<int> Accuracy, int Score, StarSmithSignTier Tier);
