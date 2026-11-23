// <copyright file="StarSmithStar.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A star of the sky. Its element, value and void are the server's until it is struck (or, for one void, revealed by
/// a Jewel of Guardian).
/// </summary>
/// <param name="X">Its position across the item's box, 0 to 1.</param>
/// <param name="Y">Its position down the item's box, 0 to 1.</param>
/// <param name="Element">Its element.</param>
/// <param name="Value">Its value, 1 to 5: its weight in the element mix.</param>
/// <param name="IsVoid">Whether it is a void, which cracks the attempt.</param>
/// <param name="Revealed">Whether the client is shown it is a void before the first strike.</param>
public readonly record struct StarSmithStar(double X, double Y, StarSmithElement Element, int Value, bool IsVoid, bool Revealed = false);
