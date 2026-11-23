// <copyright file="StarSmithOnFail.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What a failed attempt does (section 3, step 5).
/// </summary>
public enum StarSmithOnFail
{
    /// <summary>The ingredients are lost, the item stays as it was.</summary>
    IngredientsLost,

    /// <summary>The item's customisation is cleared; the item stays.</summary>
    ItemDowngraded,

    /// <summary>The item is destroyed (only on a risky recipe without protection).</summary>
    ItemDestroyed,
}
