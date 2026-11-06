// <copyright file="IEnhanceTreeViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.Character;

using MUnique.OpenMU.GameLogic.EnhanceTree;

/// <summary>
/// Interface of a view whose implementation shows the skill enhancement tree of the own character.
/// </summary>
public interface IEnhanceTreeViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the learned tree skills and the free points.
    /// </summary>
    ValueTask ShowEnhanceTreeAsync();

    /// <summary>
    /// Shows the result of adding a point.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="skillNumber">The skill number of the node.</param>
    /// <param name="level">The level of the node after the request.</param>
    ValueTask ShowEnhanceTreePointResultAsync(EnhanceTreePointResult result, short skillNumber, int level);
}
