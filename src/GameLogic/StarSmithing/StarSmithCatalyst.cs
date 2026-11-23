// <copyright file="StarSmithCatalyst.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// A catalyst a recipe takes: an item that adds to its chance (a Jewel of Luck, say).
/// </summary>
public class StarSmithCatalyst
{
    /// <summary>Gets or sets the item group.</summary>
    public byte Group { get; set; }

    /// <summary>Gets or sets the item number.</summary>
    public short Number { get; set; }

    /// <summary>Gets or sets what it adds to the chance, in hundredths of a percent.</summary>
    public int Chance { get; set; }
}
