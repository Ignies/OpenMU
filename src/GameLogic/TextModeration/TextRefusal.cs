// <copyright file="TextRefusal.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.TextModeration;

/// <summary>
/// Why a player text is refused.
/// </summary>
public enum TextRefusal
{
    /// <summary>Not refused.</summary>
    None,

    /// <summary>Nothing but spaces.</summary>
    Empty,

    /// <summary>A name shorter than 3 characters.</summary>
    TooShort,

    /// <summary>Longer than its kind allows.</summary>
    TooLong,

    /// <summary>A lore of more than 3 lines.</summary>
    TooManyLines,

    /// <summary>Two spaces in a row in a name.</summary>
    DoubleSpace,

    /// <summary>A character its kind does not allow (control and invisible characters, punctuation in a name).</summary>
    BadCharacter,

    /// <summary>A link or a web address.</summary>
    Link,

    /// <summary>A colour code.</summary>
    ColourCode,

    /// <summary>An entry of the word list.</summary>
    Word,
}
