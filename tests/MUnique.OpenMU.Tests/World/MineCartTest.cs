// <copyright file="MineCartTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.World;

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

    private static async ValueTask<Player> CreateAsync(short map, Point position)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.CurrentMap!.Definition.Number = map;
        player.Position = position;
        return player;
    }
}
