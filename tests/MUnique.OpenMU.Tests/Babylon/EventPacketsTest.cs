// <copyright file="EventPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.PlayerActions.MiniGames;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView.Events;
using MUnique.OpenMU.GameServer.RemoteView.MiniGames;
using MUnique.OpenMU.Network.Packets.Babylon;

/// <summary>
/// Tests the event packets of the Babylon protocol (range B0-BF) and that a Season 6 client gets none of them.
/// </summary>
[TestFixture]
public class EventPacketsTest
{
    /// <summary>
    /// Only the Babylon client gets the event view.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheEventView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, Season21EventBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>(), Is.InstanceOf<Season21EventBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<ISeason21EventViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// A Season 6 client gets no packet for a failed entry into a Season 21 event, and nothing throws.
    /// </summary>
    [Test]
    public async ValueTask Season6ClientGetsNoEnterResultOfASeason21EventAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(new(6, 3, Network.PlugIns.ClientLanguage.English));

        await new ShowMiniGameEnterResultViewPlugIn(player).ShowResultAsync(MiniGameType.PartyInstance, EnterResult.Full).ConfigureAwait(false);

        Assert.That(output.ToArray(), Is.Empty);
    }

    /// <summary>
    /// The state packet carries the type, the level, the state and the timer.
    /// </summary>
    [Test]
    public async ValueTask StateHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new Season21EventBabylonPlugIn(player).ShowStateAsync(MiniGameType.PartyInstance, 2, MiniGameState.Playing, 600).ConfigureAwait(false);

        var packet = new EventState(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xB0)));
        Assert.That((packet.EventType, packet.GameLevel, packet.State, packet.SecondsLeft), Is.EqualTo(((ushort)MiniGameType.PartyInstance, (byte)2, (byte)2, 600u)));
    }

    /// <summary>
    /// The score and result packets carry their numbers, clamped to the field sizes.
    /// </summary>
    [Test]
    public async ValueTask ScoreAndResultHaveTheirFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var view = new Season21EventBabylonPlugIn(player);

        await view.ShowScoreAsync(MiniGameType.PartyInstance, 3, 1500, 2, 7, 90).ConfigureAwait(false);
        var score = new EventScore(output.ToArray());
        Assert.That((score.Round, score.Score, score.Rank, score.PlayerCount, score.SecondsLeft), Is.EqualTo(((byte)3, 1500u, (ushort)2, (ushort)7, 90u)));

        output.SetLength(0);
        await view.ShowResultAsync(MiniGameType.PartyInstance, true, 1500, 1, 10_000_000_000).ConfigureAwait(false);
        var result = new EventResult(output.ToArray());
        Assert.That((result.Header.SubCode, result.Success, result.Score, result.Rank, result.Experience), Is.EqualTo(((byte)0xB3, true, 1500u, (ushort)1, uint.MaxValue)));

        output.SetLength(0);
        await view.ShowEnterResultAsync(MiniGameType.PartyInstance, EnterResult.Full).ConfigureAwait(false);
        var enter = new EventEnterResult(output.ToArray());
        Assert.That((enter.EventType, enter.Result), Is.EqualTo(((ushort)MiniGameType.PartyInstance, (byte)EnterResult.Full)));
    }
}
