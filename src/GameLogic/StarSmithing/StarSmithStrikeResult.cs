// <copyright file="StarSmithStrikeResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The answer to a strike.
/// </summary>
public enum StarSmithStrikeResult
{
    /// <summary>The star is struck and linked.</summary>
    Linked,

    /// <summary>The star was a void: the attempt cracked.</summary>
    Cracked,

    /// <summary>Refused: the attempt is sealed or cracked.</summary>
    NotDrafting,

    /// <summary>Refused: no such star.</summary>
    NoSuchStar,

    /// <summary>Refused: the ingredients allow no more strikes.</summary>
    NoStrikesLeft,

    /// <summary>Refused: the star is already struck.</summary>
    StarUsed,

    /// <summary>Refused: the star may not be linked to the last one.</summary>
    NotAdjacent,
}
