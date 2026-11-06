// <copyright file="LabyrinthMoveAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;

using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;

/// <summary>
/// Action to move through a portal of a room of the Labyrinth of Dimensions.
/// </summary>
public class LabyrinthMoveAction
{
    /// <summary>
    /// Moves the player through the portal of the side of its room and shows the result.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="direction">The direction.</param>
    /// <returns>A task.</returns>
    public async ValueTask MoveAsync(Player player, LabyrinthDirection direction)
    {
        var result = Enum.IsDefined(direction) && player.CurrentMiniGame is Season21RoundsContext { Rooms: { } rooms }
            ? await rooms.MoveAsync(player, direction).ConfigureAwait(false)
            : LabyrinthMoveResult.NotInRooms;
        await player.InvokeViewPlugInAsync<ILabyrinthRoomsViewPlugIn>(p => p.ShowMoveResultAsync(direction, result)).ConfigureAwait(false);
    }
}
