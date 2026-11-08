// <copyright file="IIllusionTempleLeagueViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames.Season21;

using MUnique.OpenMU.GameLogic.Views;

/// <summary>
/// The view of the Illusion Temple League and the stones of the renewed temple. Only the Babylon client has it.
/// </summary>
public interface IIllusionTempleLeagueViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the league of the guild of the player.
    /// </summary>
    /// <param name="standing">The standing.</param>
    /// <returns>A task.</returns>
    ValueTask ShowLeagueStateAsync(IllusionTempleLeagueStanding standing);

    /// <summary>
    /// Shows the stones of the renewed temple and where the Cursed Hollow is.
    /// </summary>
    /// <param name="ownTeam">The team of the player.</param>
    /// <param name="hollow">Where the hollow is.</param>
    /// <param name="stones">The stones.</param>
    /// <returns>A task.</returns>
    ValueTask ShowStonesAsync(IllusionTempleTeam ownTeam, IllusionTempleHollowState hollow, IReadOnlyList<IllusionTempleStoneView> stones);
}
