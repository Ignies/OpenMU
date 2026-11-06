// <copyright file="IElementInfoViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Character;

/// <summary>
/// Interface of a view whose implementation shows the elemental stats of the own character.
/// </summary>
public interface IElementInfoViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the elemental stats of the own character.
    /// </summary>
    ValueTask ShowElementInfoAsync();
}
