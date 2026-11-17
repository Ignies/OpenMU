// <copyright file="TextKind.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.TextModeration;

/// <summary>
/// What a player text is.
/// </summary>
public enum TextKind
{
    /// <summary>An item's name: one line of 3 to 24 letters, digits, spaces, ' and -.</summary>
    Name,

    /// <summary>An item's lore: up to 140 characters on up to 3 lines.</summary>
    Lore,
}
