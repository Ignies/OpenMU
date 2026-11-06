// <copyright file="ErrtelSocketResult.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Items;

/// <summary>
/// The result of an errtel socket request.
/// </summary>
public enum ErrtelSocketResult
{
    /// <summary>
    /// The errtel was put in or taken out.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Taking the errtel out failed; the errtel is destroyed.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// The request was refused: a wrong item, element or socket, no space in the inventory, or the feature is off.
    /// </summary>
    Refused = 2,
}
