// <copyright file="IStarSmithingViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.StarSmithing;

using MUnique.OpenMU.GameLogic.StarSmithing;

/// <summary>
/// The view of Star Smithing (EE 90-9F, GameLogic/StarSmithing/Readme.md).
/// </summary>
public interface IStarSmithingViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Sends the smith's state (EE 91).
    /// </summary>
    /// <param name="state">The state.</param>
    ValueTask SendStateAsync(StarSmithState state);

    /// <summary>
    /// Shows a smith hammering at the bench (EE 92), to the smith and every observer.
    /// </summary>
    /// <param name="smith">The smith.</param>
    /// <param name="action">The action.</param>
    /// <param name="strikes">How many strikes to play.</param>
    ValueTask SendAnimationAsync(Player smith, StarSmithAction action, byte strikes);

    /// <summary>
    /// Answers a smithing action (EE 97).
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="result">The result.</param>
    /// <param name="refusal">The text moderation's refusal, for <see cref="StarSmithResultCode.TextRefused"/>.</param>
    /// <param name="zenCost">What the action costs in Zen.</param>
    ValueTask SendResultAsync(StarSmithAction action, StarSmithResultCode result, byte refusal, uint zenCost);
}
