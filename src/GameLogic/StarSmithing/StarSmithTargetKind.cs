// <copyright file="StarSmithTargetKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The kinds of item a recipe works on.
/// </summary>
[Flags]
public enum StarSmithTargetKind
{
    /// <summary>No kind.</summary>
    None = 0,

    /// <summary>A weapon, staff or book.</summary>
    Weapon = 1,

    /// <summary>A helm, armour, pants, gloves, boots or shield.</summary>
    Armor = 2,

    /// <summary>Wings and capes.</summary>
    Wings = 4,

    /// <summary>A pendant.</summary>
    Pendant = 8,

    /// <summary>A ring.</summary>
    Ring = 16,
}
