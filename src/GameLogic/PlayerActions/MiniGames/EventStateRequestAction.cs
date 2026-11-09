// <copyright file="EventStateRequestAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;

using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.PlugIns.PeriodicTasks;

/// <summary>
/// Shows a player the state of a Season 21 event of a game level, for the entry window of the Babylon client.
/// </summary>
public class EventStateRequestAction
{
    /// <summary>
    /// Shows the state of the event: open with the seconds until its entrance closes, playing, or closed with the
    /// seconds until its next start (0 when unknown). A game level the event doesn't have gets no answer.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="type">The event.</param>
    /// <param name="gameLevel">The game level.</param>
    public async ValueTask ShowStateAsync(Player player, MiniGameType type, byte gameLevel)
    {
        if (player.GameContext.Configuration.MiniGameDefinitions.FirstOrDefault(d => d.Type == type && d.GameLevel == gameLevel) is not { } definition)
        {
            return;
        }

        var (state, secondsLeft) = await GetStateAsync(player.GameContext, definition).ConfigureAwait(false);
        await player.InvokeViewPlugInAsync<ISeason21EventViewPlugIn>(p => p.ShowStateAsync(type, gameLevel, state, secondsLeft)).ConfigureAwait(false);
    }

    private static async ValueTask<(MiniGameState State, uint SecondsLeft)> GetStateAsync(IGameContext gameContext, MiniGameDefinition definition)
    {
        if (gameContext.MiniGames.GetRunningMiniGames(definition.Type).FirstOrDefault(g => g.Definition == definition && g.State is MiniGameState.Open or MiniGameState.Playing) is { } game)
        {
            return game.State == MiniGameState.Open ? (MiniGameState.Open, Seconds(game.EnterEndsAtUtc - DateTime.UtcNow)) : (MiniGameState.Playing, 0);
        }

        var starter = gameContext.PlugInManager.GetStrategy<MiniGameType, IPeriodicMiniGameStartPlugIn>(definition.Type);
        var next = starter is null ? null : await starter.GetDurationUntilNextStartAsync(gameContext, definition).ConfigureAwait(false);
        return (MiniGameState.Closed, next is { } wait ? Seconds(wait) : 0);
    }

    private static uint Seconds(TimeSpan time) => time > TimeSpan.Zero ? (uint)Math.Ceiling(time.TotalSeconds) : 0;
}
