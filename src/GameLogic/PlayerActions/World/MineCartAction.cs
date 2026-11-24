// <copyright file="MineCartAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.World;

using System.Runtime.CompilerServices;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// The mine cart ride of Kubera Mine (maps 123-127, one terrain): boarding puts the player on the rail, an island of
/// walkable tiles no one reaches on foot, the client walks the rail in the cart, and leaving sets the player down past
/// the wall at the rail's end. Behaviour per the Kanturu Babylon client (docs/babylon.md, <c>mine_cart</c>).
/// </summary>
public class MineCartAction
{
    /// <summary>
    /// The rail's first walkable tile, east end, beside the cart (Object159 at 190.13, 65.44).
    /// </summary>
    public static readonly Point RailStart = new(189, 65);

    /// <summary>
    /// Where a rider is set down, past the wall tile (168, 62) at the rail's west end.
    /// </summary>
    public static readonly Point RailExit = new(167, 62);

    /// <summary>
    /// The cart's tile; a board is taken within <see cref="BoardReach"/> tiles of it.
    /// </summary>
    private static readonly Point CartTile = new(190, 65);

    private const int BoardReach = 2;

    /// <summary>
    /// The cart's speed on the rail, Kanturu's 17 (a walk is 12).
    /// </summary>
    public const int RideSpeed = 17;

    /// <summary>
    /// The rail, east to west, as rows (y, first x, last x) of Terrain124.att.
    /// </summary>
    private static readonly (byte Y, byte From, byte To)[] Rail = [(65, 178, 189), (64, 177, 178), (63, 176, 177), (62, 169, 176)];

    /// <summary>
    /// The players who boarded (EE AA) and have not left yet.
    /// </summary>
    private static readonly ConditionalWeakTable<Player, object> Riders = new();

    /// <summary>
    /// Gets whether a tile is on the rail.
    /// </summary>
    /// <param name="tile">The tile.</param>
    /// <returns>True on the rail.</returns>
    public static bool IsOnRail(Point tile) => Rail.Any(r => tile.Y == r.Y && tile.X >= r.From && tile.X <= r.To);

    /// <summary>
    /// Gets whether a walk of <paramref name="player"/> may start at <paramref name="clientTile"/> however far the
    /// server lags: a rider who boarded, standing on the rail and reporting a rail tile. A ride runs longer than one
    /// walk packet, and at the cart's speed the server is about two tiles behind when the next packet comes.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="clientTile">The walk start the client reports.</param>
    /// <returns>True for a boarded rider's rail tile.</returns>
    public static bool MayStartOnRail(Player player, Point clientTile) =>
        Riders.TryGetValue(player, out _) && IsInMine(player) && IsOnRail(player.Position) && IsOnRail(clientTile);

    /// <summary>
    /// Boards the cart: a player beside it on a Kubera Mine map is put on the rail at once.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>Whether the player was put on the rail.</returns>
    public async ValueTask<bool> BoardAsync(Player player)
    {
        if (!IsInMine(player) || Math.Max(Math.Abs(player.Position.X - CartTile.X), Math.Abs(player.Position.Y - CartTile.Y)) > BoardReach)
        {
            return false;
        }

        await player.MoveAsync(RailStart).ConfigureAwait(false);
        Riders.AddOrUpdate(player, Riders);
        return true;
    }

    /// <summary>
    /// Leaves the cart: a rider anywhere on the rail (its end, or where a ride was cut off) is set down past it.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>Whether the player was set down.</returns>
    public async ValueTask<bool> LeaveAsync(Player player)
    {
        if (!IsInMine(player) || !IsOnRail(player.Position))
        {
            return false;
        }

        Riders.Remove(player);
        await player.MoveAsync(RailExit).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets whether a player is on a Kubera Mine map.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>True on maps 123-127.</returns>
    public static bool IsInMine(Player player) =>
        player.CurrentMap?.Definition.Number is >= 123 and <= 127;
}
