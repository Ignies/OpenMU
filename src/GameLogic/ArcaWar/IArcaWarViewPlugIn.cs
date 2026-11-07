// <copyright file="IArcaWarViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ArcaWar;

using MUnique.OpenMU.GameLogic.Views;

/// <summary>
/// Interface for view plugins which show the Arca War. A client without it is told in words.
/// </summary>
public interface IArcaWarViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the result of a request.
    /// </summary>
    /// <param name="kind">The kind of the request.</param>
    /// <param name="result">The result.</param>
    /// <returns>A task.</returns>
    ValueTask ShowRequestResultAsync(ArcaWarRequestKind kind, ArcaWarRequestResult result);

    /// <summary>
    /// Shows the stage of the war.
    /// </summary>
    /// <param name="stage">The stage.</param>
    /// <param name="secondsLeft">The seconds until the next stage.</param>
    /// <param name="guilds">The registered guilds.</param>
    /// <param name="registered">If set to <c>true</c>, the player is registered.</param>
    /// <param name="points">The points of the player in this war.</param>
    /// <returns>A task.</returns>
    ValueTask ShowStateAsync(ArcaWarStage stage, uint secondsLeft, byte guilds, bool registered, uint points);

    /// <summary>
    /// Shows the obelisks.
    /// </summary>
    /// <param name="obelisks">The obelisks.</param>
    /// <returns>A task.</returns>
    ValueTask ShowObelisksAsync(IReadOnlyList<ArcaObeliskView> obelisks);
}
