// <copyright file="EliteZonesTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.EliteZones;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.EliteZones;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// Tests the entry rule of the <see cref="EliteZonesFeaturePlugIn"/>.
/// </summary>
[TestFixture]
public class EliteZonesTest
{
    private static readonly EliteZoneDefinition Zone = new()
    {
        MapNumber = 0,
        Element = EliteZoneElement.Fire,
        EntranceX = 15,
        EntranceY = 15,
        X1 = 10,
        Y1 = 10,
        X2 = 20,
        Y2 = 20,
    };

    /// <summary>
    /// A walk into a zone is refused when the character is below the map level plus the zone's offset, and allowed above it.
    /// </summary>
    [Test]
    public async Task WalkIntoTheZoneFollowsTheEntryRuleAsync()
    {
        var plugIn = CreatePlugIn(levelAboveMap: 50);
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes![Stats.Level] = 30;

        Assert.That(await IsAllowedAsync(plugIn, player, new Point(9, 15), new Point(10, 15)).ConfigureAwait(false), Is.False);

        player.Attributes[Stats.Level] = 40;
        player.Attributes[Stats.MasterLevel] = 10;
        Assert.That(await IsAllowedAsync(plugIn, player, new Point(9, 15), new Point(10, 15)).ConfigureAwait(false), Is.True, "Level plus master level count.");
    }

    /// <summary>
    /// Walks outside a zone and walks which start inside it are never refused, so nobody gets stuck inside.
    /// </summary>
    [Test]
    public async Task WalksOutsideOrFromInsideAreAllowedAsync()
    {
        var plugIn = CreatePlugIn(levelAboveMap: 50);
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes![Stats.Level] = 1;

        Assert.That(await IsAllowedAsync(plugIn, player, new Point(5, 5), new Point(6, 6)).ConfigureAwait(false), Is.True);
        Assert.That(await IsAllowedAsync(plugIn, player, new Point(15, 15), new Point(16, 16)).ConfigureAwait(false), Is.True);
        Assert.That(await IsAllowedAsync(plugIn, player, new Point(10, 15), new Point(9, 15)).ConfigureAwait(false), Is.True);
    }

    /// <summary>
    /// Without an offset, a zone asks what its map asks, so everybody on the map may enter.
    /// </summary>
    [Test]
    public async Task ZoneAsksTheMapLevelByDefaultAsync()
    {
        var plugIn = CreatePlugIn(levelAboveMap: 0);
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        player.Attributes![Stats.Level] = 1;

        Assert.That(await IsAllowedAsync(plugIn, player, new Point(9, 15), new Point(10, 15)).ConfigureAwait(false), Is.True);
        Assert.That(plugIn.GetZoneAt(0, new Point(15, 15)), Is.SameAs(Zone));
        Assert.That(plugIn.GetZoneAt(1, new Point(15, 15)), Is.Null, "Zones belong to their map.");
    }

    private static EliteZonesFeaturePlugIn CreatePlugIn(int levelAboveMap)
        => new() { Configuration = new EliteZoneConfiguration { LevelAboveMap = levelAboveMap, Zones = [Zone] } };

    private static async ValueTask<bool> IsAllowedAsync(EliteZonesFeaturePlugIn plugIn, Player player, Point from, Point to)
    {
        var args = new WalkRequestValidationArgs();
        var steps = new WalkingStep[] { new(from, to, Direction.East) };
        await plugIn.ValidateWalkRequestAsync(player, steps, args).ConfigureAwait(false);
        return args.IsWalkAllowed;
    }
}
