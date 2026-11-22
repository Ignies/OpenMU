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

    /// <summary>
    /// Sends the dyes a recolour may spend (EE 98).
    /// </summary>
    /// <param name="dyes">The dyes.</param>
    ValueTask SendDyesAsync(IReadOnlyList<StarSmithDye> dyes);

    /// <summary>
    /// Answers a Forge start with the dealt sky, or why there is none (EE 9D).
    /// </summary>
    /// <param name="sky">What the client may know of the sky, built field by field.</param>
    ValueTask SendForgeSkyAsync(StarSmithForgeSkyMessage sky);

    /// <summary>
    /// Answers a Forge strike (EE 9E). A refused strike shows nothing of the star.
    /// </summary>
    /// <param name="star">The star named.</param>
    /// <param name="strike">The answer, with the struck star.</param>
    /// <param name="strikesLeft">How many stars may still be struck.</param>
    /// <param name="guidance">The known recipes the path is still a prefix of.</param>
    ValueTask SendForgeStrikeAsync(int star, StarSmithStrike strike, int strikesLeft, IReadOnlyList<short> guidance);

    /// <summary>
    /// Tells how a Forge attempt ended, or that a seal or abandon was refused (EE 9F).
    /// </summary>
    /// <param name="outcome">The outcome.</param>
    /// <param name="result">The result, or null when refused.</param>
    ValueTask SendForgeResultAsync(StarSmithForgeOutcome outcome, StarSmithForgeResult? result);

    /// <summary>
    /// Sends the smith's Guidebook (EE 90): the recipe count and the found recipes only.
    /// </summary>
    /// <param name="total">How many recipes the world has.</param>
    /// <param name="found">The found recipes.</param>
    ValueTask SendGuidebookAsync(int total, IReadOnlyList<StarSmithGuidebookEntry> found);
}
