// <copyright file="ArcaElement.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

/// <summary>
/// The element of an obelisk, which decides the buff of the guild that holds it.
/// </summary>
public enum ArcaElement
{
    /// <summary>
    /// No element: the war picks one at random.
    /// </summary>
    None,

    /// <summary>
    /// Fire.
    /// </summary>
    Fire,

    /// <summary>
    /// Water.
    /// </summary>
    Water,

    /// <summary>
    /// Earth.
    /// </summary>
    Earth,

    /// <summary>
    /// Wind.
    /// </summary>
    Wind,

    /// <summary>
    /// Darkness.
    /// </summary>
    Darkness,
}
