// <copyright file="IPentagramViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Inventory;

using MUnique.OpenMU.GameLogic.PlayerActions.Items;

/// <summary>
/// Interface of a view whose implementation shows the errtels of a pentagram and the result of putting an errtel
/// into a socket or taking it out.
/// </summary>
public interface IPentagramViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the result of an errtel socket request.
    /// </summary>
    /// <param name="mode">The mode of the request.</param>
    /// <param name="result">The result.</param>
    /// <param name="pentagramSlot">The inventory slot of the pentagram.</param>
    /// <param name="socket">The socket.</param>
    ValueTask ShowErrtelSocketResultAsync(ErrtelSocketMode mode, ErrtelSocketResult result, byte pentagramSlot, byte socket);

    /// <summary>
    /// Shows the errtels in the sockets of a pentagram.
    /// </summary>
    /// <param name="pentagram">The pentagram.</param>
    ValueTask ShowPentagramErrtelsAsync(Item pentagram);
}
