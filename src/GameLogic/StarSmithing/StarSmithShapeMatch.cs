// <copyright file="StarSmithShapeMatch.cs" company="MUnique">
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
