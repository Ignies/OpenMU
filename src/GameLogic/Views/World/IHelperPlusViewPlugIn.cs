// <copyright file="IHelperPlusViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.HelperPlus;

/// <summary>
/// Interface of a view whose implementation shows the rules and moves of the helper plus.
/// </summary>
public interface IHelperPlusViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the rules of the helper plus.
    /// </summary>
    /// <param name="flags">The flags for the character and its map.</param>
    /// <param name="configuration">The configuration with the level, ranges and cost.</param>
    ValueTask ShowHelperPlusRulesAsync(HelperPlusFlags flags, HelperPlusConfiguration configuration);

    /// <summary>
    /// Shows the result of a move into a hunting zone.
    /// </summary>
    /// <param name="zoneIndex">The zone index.</param>
    /// <param name="result">The result.</param>
    ValueTask ShowHelperPlusMoveResultAsync(short zoneIndex, HelperPlusMoveResult result);
}
