// <copyright file="StarSmithHint.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A glimmering line between two stars, an edge of a recipe's shape.
/// </summary>
/// <param name="From">One star.</param>
/// <param name="To">The other star.</param>
/// <param name="Clarity">How clearly it shows, 0 to 1.</param>
public readonly record struct StarSmithHint(int From, int To, double Clarity);
