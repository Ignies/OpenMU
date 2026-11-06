// <copyright file="ISeason21EventViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.MiniGames;

using MUnique.OpenMU.GameLogic.MiniGames.Season21;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.GameLogic.Views;

/// <summary>
/// Interface for view plugins which show the events and party instances of Season 21: their state, the score while
/// they run and the result. The Season 6 client has no view of them; it gets the mini game messages in words.
/// </summary>
public interface ISeason21EventViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the state of an event.
    /// </summary>
    /// <param name="type">The type of the event.</param>
    /// <param name="gameLevel">The game level.</param>
    /// <param name="state">The state.</param>
    /// <param name="secondsLeft">The seconds until the next state, 0 when unknown.</param>
    ValueTask ShowStateAsync(MiniGameType type, byte gameLevel, MiniGameState state, uint secondsLeft);

    /// <summary>
    /// Shows the score of the own character in a running event.
    /// </summary>
    /// <param name="type">The type of the event.</param>
    /// <param name="round">The round, floor or stage, 0 when the event has none.</param>
    /// <param name="score">The score.</param>
    /// <param name="rank">The rank, 0 when unranked.</param>
    /// <param name="playerCount">The number of players.</param>
    /// <param name="secondsLeft">The seconds left.</param>
    ValueTask ShowScoreAsync(MiniGameType type, byte round, int score, int rank, int playerCount, uint secondsLeft);

    /// <summary>
    /// Shows the result of an event for the own character.
    /// </summary>
    /// <param name="type">The type of the event.</param>
    /// <param name="success">If set to <c>true</c>, the character succeeded.</param>
    /// <param name="score">The score.</param>
    /// <param name="rank">The rank.</param>
    /// <param name="experience">The experience of the reward.</param>
    ValueTask ShowResultAsync(MiniGameType type, bool success, int score, int rank, long experience);

    /// <summary>
    /// Shows why the entry failed.
    /// </summary>
    /// <param name="type">The type of the event.</param>
    /// <param name="result">The result.</param>
    ValueTask ShowEnterResultAsync(MiniGameType type, EnterResult result);

    /// <summary>
    /// Shows the standing of the own character in the league of the Labyrinth of Dimensions.
    /// </summary>
    /// <param name="standing">The standing.</param>
    /// <param name="day">The kind of the day in the league schedule.</param>
    ValueTask ShowLabyrinthLeagueAsync(LabyrinthStanding standing, LabyrinthDayKind day);
}
