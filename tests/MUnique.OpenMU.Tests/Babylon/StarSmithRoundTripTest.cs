// <copyright file="StarSmithRoundTripTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using Moq;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;

/// <summary>
/// The beat ping (Readme.md "PR 6 contract", "Beat ping"): the server measures the round trip on its own clock, from
/// its ping to the client's answer with the same token; the client never states a time.
/// </summary>
[TestFixture]
public class StarSmithRoundTripTest
{
    private long _now;

    /// <summary>
    /// No answer yet is no allowance; an answer with the token is the time between, on the server's clock.
    /// </summary>
    [Test]
    public void MeasuresOnTheServersClock()
    {
        var trip = new StarSmithRoundTrip();
        Assert.That(trip.RoundTripMs, Is.Zero);
        var token = trip.Send(1_000);
        Assert.That(token, Is.Not.Null);
        trip.Answer(token!.Value + 1, 1_010);
        Assert.That(trip.RoundTripMs, Is.Zero, "another token is ignored");
        trip.Answer(token.Value, 1_080);
        Assert.That(trip.RoundTripMs, Is.EqualTo(80));
        trip.Answer(token.Value, 1_090);
        Assert.That(trip.RoundTripMs, Is.EqualTo(80), "a token counts once");
    }

    /// <summary>
    /// One ping is out at a time and at most one per 2 seconds; an answer after 5 seconds is ignored.
    /// </summary>
    [Test]
    public void OneAtATime()
    {
        var trip = new StarSmithRoundTrip();
        var token = trip.Send(0)!.Value;
        Assert.That(trip.Send(1_000), Is.Null, "one is out");
        Assert.That(trip.Send(4_000), Is.Null, "still out");
        trip.Answer(token, 5_001);
        Assert.That(trip.RoundTripMs, Is.Zero, "too late");
        var next = trip.Send(5_001);
        Assert.That(next, Is.Not.Null, "the lost one expired");
        trip.Answer(next!.Value, 5_051);
        Assert.That(trip.Send(6_000), Is.Null, "at most one per 2 seconds");
        Assert.That(trip.Send(7_001), Is.Not.Null);
    }

    /// <summary>
    /// The round trip is the lowest of the last three samples, so a slow answer cannot widen the window alone.
    /// </summary>
    [Test]
    public void TheLowestOfTheLastThree()
    {
        var trip = new StarSmithRoundTrip();
        long now = 0;
        foreach (var sample in new[] { 120, 60, 90, 200, 150 })
        {
            var token = trip.Send(now)!.Value;
            trip.Answer(token, now + sample);
            now += 10_000;
        }

        Assert.That(trip.RoundTripMs, Is.EqualTo(90));
    }

    /// <summary>
    /// The smith's state goes out with a ping; the answer's round trip is the one a forge start uses.
    /// </summary>
    [Test]
    public async ValueTask TheForgeStartUsesTheMeasuredRoundTripAsync()
    {
        this._now = 10_000;
        var player = await PlayerTestHelper.CreatePlayerAsync().ConfigureAwait(false);
        await AddAsync(player, 0, 2, 1000, 255).ConfigureAwait(false);
        await AddAsync(player, 12, 0, 1).ConfigureAwait(false);
        await AddAsync(player, 13, 14, 13).ConfigureAwait(false);
        var recipe = new StarSmithRecipe { Number = 7, Targets = StarSmithTargetKind.Weapon, Ingredients = [new StarSmithIngredient { Group = 14, Number = 13 }], BaseChance = 10_000, Look = StarSmithLook.Fire, Geometry = StarSmithGeometry.Of(3, true, [120, 120], [1, 1, 1]) };
        var plugIn = new StarSmithingFeaturePlugIn { Configuration = new StarSmithingConfiguration { Recipes = [recipe] }, Clock = () => this._now, NewSeed = () => 4711 };
        var view = Mock.Get(player.ViewPlugIns.GetPlugIn<IStarSmithingViewPlugIn>()!);
        uint? sent = null;
        view.Setup(v => v.SendPingAsync(It.IsAny<uint>())).Callback<uint>(t => sent = t).Returns(ValueTask.CompletedTask);

        await plugIn.SendStateAsync(player).ConfigureAwait(false);
        Assert.That(sent, Is.Not.Null, "a ping goes with the state");
        this._now += 70;
        plugIn.Pong(player, sent!.Value);

        await plugIn.StartForgeAsync(player, 12, [13], []).ConfigureAwait(false);
        Assert.That(player.StarSmithForge!.RoundTripMs, Is.EqualTo(70));
    }

    private static async ValueTask AddAsync(Player player, byte slot, byte group, short number, byte durability = 1)
    {
        var item = player.PersistenceContext.CreateNew<Item>();
        item.Definition = new MUnique.OpenMU.Persistence.BasicModel.ItemDefinition { Group = group, Number = number, Width = 1, Height = 1, Durability = durability };
        item.Durability = durability;
        await player.Inventory!.AddItemAsync(slot, item).ConfigureAwait(false);
    }
}
