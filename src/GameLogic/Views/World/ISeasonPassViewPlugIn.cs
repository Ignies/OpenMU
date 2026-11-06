// <copyright file="ISeasonPassViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.SeasonPass;

/// <summary>
/// Interface of a view whose implementation shows the season pass.
/// </summary>
public interface ISeasonPassViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the state of the season pass.
    /// </summary>
    /// <param name="state">The state.</param>
    ValueTask ShowSeasonPassStateAsync(SeasonPassView state);

    /// <summary>
    /// Shows the result of a claimed reward.
    /// </summary>
    /// <param name="pass">The pass.</param>
    /// <param name="level">The level.</param>
    /// <param name="result">The result.</param>
    ValueTask ShowSeasonPassClaimResultAsync(SeasonPassKind pass, byte level, SeasonPassClaimResult result);
}
