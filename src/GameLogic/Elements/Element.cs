// <copyright file="Element.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// The elements of Season 21 (the value of <see cref="Attributes.Stats.Element"/>).
/// </summary>
public enum Element
{
    /// <summary>
    /// No element: a character without pentagram or a monster without element.
    /// </summary>
    None = 0,

    /// <summary>
    /// The fire element.
    /// </summary>
    Fire = 1,

    /// <summary>
    /// The water element.
    /// </summary>
    Water = 2,

    /// <summary>
    /// The earth element.
    /// </summary>
    Earth = 3,

    /// <summary>
    /// The wind element.
    /// </summary>
    Wind = 4,

    /// <summary>
    /// The darkness element.
    /// </summary>
    Darkness = 5,

    /// <summary>
    /// A monster of this element takes a random one of the five when it is added to its map (S21Emu <c>ElementalAttribute="6"</c>).
    /// </summary>
    Random = 6,
}
