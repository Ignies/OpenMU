// <copyright file="IMiningViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.Mining;
using MUnique.OpenMU.GameLogic.NPC;

/// <summary>
/// Interface of a view whose implementation shows the mining window of a mining spot.
/// </summary>
public interface IMiningViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the result of a mining request.
    /// </summary>
    /// <param name="spot">The mining spot.</param>
    /// <param name="result">The result.</param>
    /// <param name="stage">The stage reached, 0 before the first.</param>
    /// <param name="itemCount">The jewels which are pending, or which were collected.</param>
    /// <param name="bonusCount">The jewels of the jackpot, 0 when none.</param>
    ValueTask ShowMiningResultAsync(NonPlayerCharacter spot, MiningResult result, byte stage, int itemCount, int bonusCount);
}
