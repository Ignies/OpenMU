// <copyright file="StarSmithSignTier.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The tier of a sealed constellation (design section 13's ladder), from its score (Readme.md "PR 7a contract"). The
/// byte is the item rarity of the same number.
/// </summary>
public enum StarSmithSignTier : byte
{
    /// <summary>A light tint: a score under 400.</summary>
    Faint,

    /// <summary>The full colour: 400 to 599.</summary>
    Hue,

    /// <summary>A metallic sheen and an edge glow: 600 to 799.</summary>
    Lustre,

    /// <summary>The effect starts: 800 to 899.</summary>
    Venom,

    /// <summary>The full effect: 900 and more.</summary>
    Virulent,

    /// <summary>Virulent and the extension placed perfect and clean: the chroma chrome.</summary>
    Chroma,
}
