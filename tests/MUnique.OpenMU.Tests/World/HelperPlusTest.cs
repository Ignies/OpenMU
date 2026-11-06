// <copyright file="HelperPlusTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.World;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.HelperPlus;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// Tests the MU Helper Plus of Season 21 (<see cref="HelperPlusPlugIn"/>).
/// </summary>
[TestFixture]
public class HelperPlusTest
{
    /// <summary>
    /// The plus mode needs its total level, and in a party the leader; the rules say whether the map has zones and the mode is on.
    /// </summary>
    [Test]
    public async ValueTask PlusModeNeedsItsLevelAndTheLeaderAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync(399).ConfigureAwait(false);
        await plugIn.SetPlusModeAsync(player, true).ConfigureAwait(false);
        Assert.That(plugIn.GetFlags(player), Is.EqualTo(HelperPlusFlags.MapHasZones));

        player.Attributes![Stats.MasterLevel] = 1;
        await plugIn.SetPlusModeAsync(player, true).ConfigureAwait(false);
        Assert.That(plugIn.GetFlags(player), Is.EqualTo(HelperPlusFlags.MapHasZones | HelperPlusFlags.MayUsePlus | HelperPlusFlags.PlusOn));

        var member = await CreatePlayerAsync(400, player.GameContext).ConfigureAwait(false);
        var party = new Party(new PartyManager(5, new NullLogger<Party>()), 5, new NullLogger<Party>());
        await party.AddAsync(player).ConfigureAwait(false);
        await party.AddAsync(member).ConfigureAwait(false);
        Assert.That(plugIn.GetFlags(member).HasFlag(HelperPlusFlags.MayUsePlus), Is.False, "Only the party leader.");

        await plugIn.SetPlusModeAsync(player, false).ConfigureAwait(false);
        Assert.That(plugIn.IsPlusOn(player), Is.False);
    }

    /// <summary>
    /// In the plus mode the helper costs the configured percent of the normal cost.
    /// </summary>
    [Test]
    public async ValueTask PlusModeCostsItsPercentAsync()
    {
        var plugIn = CreatePlugIn();
        plugIn.Configuration!.PlusCostPercent = 150;
        var player = await CreatePlayerAsync(400).ConfigureAwait(false);
        player.GameContext.FeaturePlugIns.AddPlugIn(plugIn, true);
        Assert.That(HelperPlusPlugIn.ApplyCost(player, 8000), Is.EqualTo(8000));

        await plugIn.SetPlusModeAsync(player, true).ConfigureAwait(false);

        Assert.That(HelperPlusPlugIn.ApplyCost(player, 8000), Is.EqualTo(12000));
        var helper = new GameLogic.MuHelper.MuHelperConfiguration { CostPerStage = [20], StageInterval = TimeSpan.FromHours(1) };
        Assert.That(GameLogic.MuHelper.MuHelperZenCostCalculator.Calculate(player, helper, DateTime.UtcNow), Is.EqualTo(12000), "The helper pays it.");
    }

    /// <summary>
    /// A move into a zone needs the plus mode and the level and zen of the ground's warp; it pays the zen and places the
    /// character at the hunting point of the zone.
    /// </summary>
    [Test]
    public async ValueTask MoveIntoAZoneAsync()
    {
        var plugIn = CreatePlugIn();
        var player = await CreatePlayerAsync(400).ConfigureAwait(false);
        var warp = new Persistence.BasicModel.WarpInfo { Index = 49, Costs = 50_000, LevelRequirement = 300, Gate = new Persistence.BasicModel.ExitGate { Map = player.CurrentMap!.Definition } };
        Moq.Mock.Get(player.GameContext.Configuration).Setup(c => c.WarpList).Returns(new List<WarpInfo> { warp });

        Assert.That(await plugIn.MoveAsync(player, 7).ConfigureAwait(false), Is.EqualTo(HelperPlusMoveResult.PlusModeOff));
        await plugIn.SetPlusModeAsync(player, true).ConfigureAwait(false);
        Assert.That(await plugIn.MoveAsync(player, 99).ConfigureAwait(false), Is.EqualTo(HelperPlusMoveResult.UnknownZone));
        Assert.That(await plugIn.MoveAsync(player, 7).ConfigureAwait(false), Is.EqualTo(HelperPlusMoveResult.NotEnoughMoney));

        warp.LevelRequirement = 500;
        player.Money = 60_000;
        Assert.That(await plugIn.MoveAsync(player, 7).ConfigureAwait(false), Is.EqualTo(HelperPlusMoveResult.LevelTooLow));

        warp.LevelRequirement = 300;
        Assert.That(await plugIn.MoveAsync(player, 7).ConfigureAwait(false), Is.EqualTo(HelperPlusMoveResult.Moved));
        Assert.That((player.Position, player.Money), Is.EqualTo((new Point(190, 98), 10_000)));
    }

    private static async ValueTask<Player> CreatePlayerAsync(int level, IGameContext? context = null)
    {
        var player = context is null ? await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false) : await PlayerTestHelper.CreatePlayerAsync(context).ConfigureAwait(false);
        player.CurrentMap = await player.GameContext.GetMapAsync(0).ConfigureAwait(false);
        player.Attributes![Stats.Level] = level;
        return player;
    }

    private static HelperPlusPlugIn CreatePlugIn()
        => new()
        {
            Configuration = new HelperPlusConfiguration
            {
                Grounds = [new HelperPlusGround { Index = 0, WarpIndex = 49, Name = "Debenter" }],
                Zones = [new HelperPlusZone { Index = 7, Ground = 0, MapNumber = 0, Element = 1, X = 190, Y = 98 }],
            },
        };
}
