// <copyright file="MineCartTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.World;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.PlayerActions.World;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// Tests the Kubera Mine cart ride (<see cref="MineCartAction"/>).
/// </summary>
[TestFixture]
public class MineCartTest
{
    /// <summary>
    /// Beside the cart, a board puts the player on the rail's first tile.
    /// </summary>
    [Test]
    public async ValueTask BoardMovesBesideTheCartOntoTheRailAsync()
    {
        var player = await CreateAsync(124, new Point(191, 66)).ConfigureAwait(false);

        var result = await new MineCartAction().BoardAsync(player).ConfigureAwait(false);

        Assert.That(result, Is.True);
        Assert.That(player.Position, Is.EqualTo(MineCartAction.RailStart));
    }

    /// <summary>
    /// Away from the cart, or on another map, the board is refused.
    /// </summary>
    [Test]
    public async ValueTask BoardIsRefusedAwayFromTheCartAsync()
    {
        var far = await CreateAsync(124, new Point(194, 65)).ConfigureAwait(false);
        var elsewhere = await CreateAsync(2, new Point(191, 65)).ConfigureAwait(false);

        Assert.That(await new MineCartAction().BoardAsync(far).ConfigureAwait(false), Is.False);
        Assert.That(far.Position, Is.EqualTo(new Point(194, 65)));
        Assert.That(await new MineCartAction().BoardAsync(elsewhere).ConfigureAwait(false), Is.False);
    }

    /// <summary>
    /// From any rail tile (the ride's end, or where a ride was cut off), a leave sets the player down past the rail's end.
    /// </summary>
    [Test]
    public async ValueTask LeaveFromTheRailSetsDownPastItsEndAsync()
    {
        foreach (var tile in new[] { new Point(169, 62), new Point(185, 65), new Point(177, 63) })
        {
            var player = await CreateAsync(127, tile).ConfigureAwait(false);

            Assert.That(await new MineCartAction().LeaveAsync(player).ConfigureAwait(false), Is.True);
            Assert.That(player.Position, Is.EqualTo(MineCartAction.RailExit));
        }
    }

    /// <summary>
    /// Off the rail a leave does nothing.
    /// </summary>
    [Test]
    public async ValueTask LeaveOffTheRailIsRefusedAsync()
    {
        var player = await CreateAsync(123, new Point(166, 62)).ConfigureAwait(false);

        Assert.That(await new MineCartAction().LeaveAsync(player).ConfigureAwait(false), Is.False);
        Assert.That(player.Position, Is.EqualTo(new Point(166, 62)));
    }

    /// <summary>
    /// A rider on the rail rolls at Kanturu's 17 (a step of 100 / 17 * 40 ms), off it the walk stays at 12.
    /// </summary>
    [Test]
    public async ValueTask RiderRollsAtTheCartSpeedAsync()
    {
        var rider = await CreateAsync(124, new Point(180, 65)).ConfigureAwait(false);
        var walker = await CreateAsync(124, new Point(191, 66)).ConfigureAwait(false);

        Assert.That(rider.StepDelay.TotalMilliseconds, Is.EqualTo(100.0 / MineCartAction.RideSpeed * 40.0).Within(0.01));
        Assert.That(MineCartAction.RideSpeed, Is.EqualTo(17));
        Assert.That(walker.StepDelay.TotalMilliseconds, Is.EqualTo(100.0 / 12 * 40.0).Within(0.01));
    }

    /// <summary>
    /// The rail is longer than one walk packet (15 steps), and at 17 the server runs about two tiles behind the
    /// client: a boarded rider's next walk, starting ahead on the rail while the first still runs, is taken.
    /// </summary>
    [Test]
    public async ValueTask BoardedRidersNextWalkFromAheadOnTheRailIsTakenAsync()
    {
        var rider = await BoardedAtAsync(MineCartAction.RailStart).ConfigureAwait(false);

        Assert.That(await RollThenWalkFromAsync(rider, new Point(186, 65)).ConfigureAwait(false), Is.True);
    }

    /// <summary>
    /// The rail start is taken only from a boarded rider and only for rail tiles: a body on the rail that never
    /// boarded, a rider starting off the rail, and a rider who left keep the normal offset check.
    /// </summary>
    [Test]
    public async ValueTask OthersOnOrOffTheRailKeepTheOffsetCheckAsync()
    {
        var unboarded = await CreateAsync(124, MineCartAction.RailStart).ConfigureAwait(false);
        var offRail = await BoardedAtAsync(MineCartAction.RailStart).ConfigureAwait(false);
        var left = await BoardedAtAsync(new Point(169, 62)).ConfigureAwait(false);
        await new MineCartAction().LeaveAsync(left).ConfigureAwait(false);
        left.Position = MineCartAction.RailStart;

        Assert.That(await RollThenWalkFromAsync(unboarded, new Point(186, 65)).ConfigureAwait(false), Is.False);
        Assert.That(await RollThenWalkFromAsync(offRail, new Point(182, 67)).ConfigureAwait(false), Is.False);
        Assert.That(await RollThenWalkFromAsync(left, new Point(186, 65)).ConfigureAwait(false), Is.False);
    }

    /// <summary>
    /// The ride is new: its request handler is off until a world switches it on (Admin > Systems).
    /// </summary>
    [Test]
    public void RequestHandlerIsOffByDefault()
    {
        var handler = typeof(MUnique.OpenMU.GameServer.GameServer).Assembly.GetType("MUnique.OpenMU.GameServer.MessageHandler.Babylon.MineCartRequestHandlerPlugIn");

        Assert.That(handler, Is.Not.Null);
        Assert.That(handler!.IsAssignableTo(typeof(IDisabledByDefault)), Is.True);
    }

    /// <summary>
    /// A player who boarded the cart (EE AA), then rolled to <paramref name="position"/> as the server sees it.
    /// </summary>
    private static async ValueTask<Player> BoardedAtAsync(Point position)
    {
        var player = await CreateAsync(124, new Point(191, 66)).ConfigureAwait(false);
        await new MineCartAction().BoardAsync(player).ConfigureAwait(false);
        player.Position = position;
        return player;
    }

    /// <summary>
    /// Starts the first ride packet (west to 178,65), then at once asks for the next walk west from
    /// <paramref name="from"/>, as a client running ahead does; true when the server took that start.
    /// </summary>
    private static async ValueTask<bool> RollThenWalkFromAsync(Player player, Point from)
    {
        var steps = new WalkingStep[player.Position.X - 178];
        for (var i = 0; i < steps.Length; i++)
        {
            steps[i] = new(new Point((byte)(player.Position.X - i), player.Position.Y), new Point((byte)(player.Position.X - i - 1), player.Position.Y), Direction.West);
        }

        await player.WalkToAsync(new Point(178, player.Position.Y), steps).ConfigureAwait(false);
        var to = new Point((byte)(from.X - 1), from.Y);
        WalkingStep[] next = [new(from, to, Direction.West)];
        await player.WalkToAsync(to, next).ConfigureAwait(false);
        await player.StopWalkingAsync().ConfigureAwait(false);
        return player.Position == from || player.Position == to;
    }

    private static async ValueTask<Player> CreateAsync(short map, Point position)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.CurrentMap!.Definition.Number = map;
        player.Position = position;
        return player;
    }
}
