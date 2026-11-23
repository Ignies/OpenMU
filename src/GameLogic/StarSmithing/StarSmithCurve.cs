// <copyright file="StarSmithCurve.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A curve of the form base * x^exponent.
/// </summary>
public class StarSmithCurve
{
    /// <summary>Gets or sets the base (the level curve's base, the scaling's factor).</summary>
    public double Base { get; set; }

    /// <summary>Gets or sets the exponent.</summary>
    public double Exponent { get; set; }
}
