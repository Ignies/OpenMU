// <copyright file="StarSmithHintKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What a sealed Guidebook row shows of a recipe not found yet (the wire byte of EE 90).
/// </summary>
public enum StarSmithHintKind : byte
{
    /// <summary>Nothing.</summary>
    None,

    /// <summary>A riddle text.</summary>
    Riddle,

    /// <summary>One of its ingredients: "a recipe uses ...".</summary>
    Ingredient,
}
