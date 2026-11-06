// <copyright file="ErrtelSocketMode.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Items;

/// <summary>
/// The mode of an errtel socket request.
/// </summary>
public enum ErrtelSocketMode
{
    /// <summary>
    /// Puts an errtel into a socket of a pentagram.
    /// </summary>
    Insert = 0,

    /// <summary>
    /// Takes the errtel of a socket out of the pentagram.
    /// </summary>
    Remove = 1,
}
