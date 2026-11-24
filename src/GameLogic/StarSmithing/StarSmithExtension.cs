// <copyright file="StarSmithExtension.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A shape's optional extension: one more placement past it, needed for Chroma.
/// </summary>
public class StarSmithExtension
{
    /// <summary>Gets or sets its turn from the last segment, in degrees, left positive.</summary>
    public double Turn { get; set; }

    /// <summary>Gets or sets its length over the figure's first segment.</summary>
    public double Length { get; set; }
}
