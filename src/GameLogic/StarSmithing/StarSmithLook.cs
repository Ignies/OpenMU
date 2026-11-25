// <copyright file="StarSmithLook.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// What a sealed constellation's layer looks like (Readme.md "PR 7a contract", "Looks"): the catalogue's numbers
/// (Hidden-Babylon docs/star_constellations.md); none for the boost recipes.
/// </summary>
public enum StarSmithLook : byte
{
    /// <summary>No colour of its own.</summary>
    None,

    /// <summary>Flame hue.</summary>
    Fire,

    /// <summary>Venom green.</summary>
    Poison,

    /// <summary>Ice blue.</summary>
    Frost,

    /// <summary>Electric violet.</summary>
    Storm,

    /// <summary>Smoke black.</summary>
    Shadow,

    /// <summary>Radiant white-gold.</summary>
    Light,

    /// <summary>Deep crimson.</summary>
    Blood,

    /// <summary>Stone and amber.</summary>
    Earth,

    /// <summary>Sea teal.</summary>
    Water,

    /// <summary>Pale jade.</summary>
    Wind,

    /// <summary>Deep violet runes.</summary>
    Arcane,

    /// <summary>Leaf green.</summary>
    Nature,

    /// <summary>Polished gold.</summary>
    Gold,

    /// <summary>Bright silver.</summary>
    Silver,

    /// <summary>Black glass with red veins.</summary>
    Obsidian,

    /// <summary>Carved jade.</summary>
    Jade,

    /// <summary>Red-tempered steel.</summary>
    CrimsonSteel,

    /// <summary>Night blue with moving stars.</summary>
    Starlight,

    /// <summary>A rainbow that turns with the view.</summary>
    Prism,

    /// <summary>Chroma chrome.</summary>
    Chroma,

    /// <summary>Pearl with a rose sheen.</summary>
    Pearl,

    /// <summary>Aged bronze with green edges.</summary>
    Bronze,
}
