// <copyright file="StarSmithIngredient.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// One ingredient of a recipe: an item and how many of it the recipe takes.
/// </summary>
public class StarSmithIngredient
{
    /// <summary>Gets or sets the item group.</summary>
    public byte Group { get; set; }

    /// <summary>Gets or sets the item number.</summary>
    public short Number { get; set; }

    /// <summary>Gets or sets the fewest the recipe takes.</summary>
    public byte Minimum { get; set; } = 1;

    /// <summary>Gets or sets the most the recipe takes.</summary>
    public byte Maximum { get; set; } = 1;
}
