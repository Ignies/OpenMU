// <copyright file="StarSmithBoostRange.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A boost an outcome gives: a stat and the range it is rolled in, in the stat's unit (percent or flat).
/// </summary>
public class StarSmithBoostRange
{
    /// <summary>Gets or sets the stat.</summary>
    public StarSmithBoostStat Stat { get; set; }

    /// <summary>Gets or sets the least value.</summary>
    public double Minimum { get; set; }

    /// <summary>Gets or sets the most value.</summary>
    public double Maximum { get; set; }
}
