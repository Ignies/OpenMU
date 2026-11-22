// <copyright file="StarSmithRarity.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The rarity tiers (section 3, "Rarity").
/// </summary>
public enum StarSmithRarity
{
    /// <summary>Common, white: no effect, one boost.</summary>
    Common,

    /// <summary>Uncommon, green: no effect, two boosts.</summary>
    Uncommon,

    /// <summary>Rare, blue: one effect, two boosts.</summary>
    Rare,

    /// <summary>Epic, purple: one effect, three boosts.</summary>
    Epic,

    /// <summary>Legendary, orange: two effects, three boosts.</summary>
    Legendary,

    /// <summary>Mythic, red: two effects, three boosts and an aura.</summary>
    Mythic,
}
