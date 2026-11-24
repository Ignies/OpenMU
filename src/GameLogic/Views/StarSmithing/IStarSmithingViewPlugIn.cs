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
    /// Answers a Forge start with the attempt, or why there is none (EE 9D).
    /// </summary>
    /// <param name="message">What the client may know of the attempt, built field by field.</param>
    ValueTask SendForgeAttemptAsync(StarSmithForgeAttemptMessage message);

    /// <summary>
    /// Answers a Forge node or stash (EE 9E): the live match, the guide, accuracy and timing per node, a first find.
    /// </summary>
    /// <param name="answer">The answer.</param>
    ValueTask SendForgeNodeAsync(StarSmithFigureAnswer answer);

    /// <summary>
    /// Tells how a Forge attempt ended, or that a seal or abandon was refused (EE 9F).
    /// </summary>
    /// <param name="outcome">The outcome.</param>
    /// <param name="result">The result, or null when refused.</param>
    /// <param name="experience">The smith experience it gave.</param>
    /// <param name="firstSeal">Whether a first seal doubled its experience.</param>
    ValueTask SendForgeResultAsync(StarSmithForgeOutcome outcome, StarSmithForgeResult? result, int experience, bool firstSeal);

    /// <summary>
    /// Sends the beat ping (EE F3); the client answers at once with the token (EE F4), and the server measures the
    /// round trip on its own clock.
    /// </summary>
    /// <param name="token">The token the answer must carry.</param>
    ValueTask SendPingAsync(uint token);

    /// <summary>
    /// Sends the smith's Guidebook (EE 90): the recipe count, the found recipes in full and a sealed row with its hint
    /// for every other one.
    /// </summary>
    /// <param name="total">How many recipes the world has.</param>
    /// <param name="found">The rows, found ones first.</param>
    ValueTask SendGuidebookAsync(int total, IReadOnlyList<StarSmithGuidebookEntry> found);
}
