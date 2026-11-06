// <copyright file="EventPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.MiniGames;
using MUnique.OpenMU.GameLogic.MiniGames.Season21;
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
    /// <summary>
    /// The league packet of the labyrinth carries the tier, rank, group, day kind, points and floors.
    /// </summary>
    [Test]
    public async ValueTask LabyrinthLeagueHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var view = new Season21EventBabylonPlugIn(player);

        await view.ShowLabyrinthLeagueAsync(new LabyrinthStanding(LabyrinthLeagueTier.Gold, 2, 5, 12_345, 7), LabyrinthDayKind.Play).ConfigureAwait(false);

        var league = new LabyrinthLeague(output.ToArray());
        Assert.That(
            (league.Header.SubCode, league.Tier, league.Rank, league.GroupSize, league.Day, league.Points, league.FloorsToday),
            Is.EqualTo(((byte)0xB5, (byte)2, (byte)2, (byte)5, (byte)2, 12_345u, (ushort)7)));
    }

    /// <summary>
    /// Only the Babylon client gets the view of the labyrinth rooms, so only it plays them.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheLabyrinthRooms()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, LabyrinthRoomsBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<ILabyrinthRoomsViewPlugIn>(), Is.InstanceOf<LabyrinthRoomsBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<ILabyrinthRoomsViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The board of the labyrinth carries the room, the area of the other map, the portals and the opened and cleared rooms
    /// as bits.
    /// </summary>
    [Test]
    public async ValueTask LabyrinthBoardHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var view = new LabyrinthRoomsBabylonPlugIn(player);
        var discovered = new bool[100];
        discovered[0] = discovered[9] = discovered[99] = true;
        var cleared = new bool[100];
        cleared[9] = true;
        var area = new LabyrinthArea { Number = 25, MapNumber = 113, StartX = 181, StartY = 132, EndX = 198, EndY = 143 };

        await view.ShowBoardAsync(new LabyrinthBoard(7, 9, area, LabyrinthAreaType.Mission, [LabyrinthPortal.Closed, LabyrinthPortal.Mission, LabyrinthPortal.Open, LabyrinthPortal.Closed], discovered, cleared)).ConfigureAwait(false);

        var board = new LabyrinthRoomBoard(output.ToArray());
        Assert.That(
            (board.Header.SubCode, board.PathNumber, board.RoomIndex, board.MapNumber, board.StartX, board.StartY, board.EndX, board.EndY, board.AreaType, board.AreaNumber),
            Is.EqualTo(((byte)0xB6, (byte)7, (byte)9, (ushort)113, (byte)181, (byte)132, (byte)198, (byte)143, (byte)3, (byte)25)));
        Assert.That((board.PortalTop, board.PortalBottom, board.PortalLeft, board.PortalRight), Is.EqualTo(((byte)0, (byte)2, (byte)1, (byte)0)));
        Assert.That(board.Discovered.ToArray()[0..2], Is.EqualTo(new byte[] { 0x01, 0x02 }));
        Assert.That(board.Discovered[12], Is.EqualTo((byte)0x08));
        Assert.That(board.Cleared.ToArray()[0..2], Is.EqualTo(new byte[] { 0x00, 0x02 }));
    }

    /// <summary>
    /// The mission of a room carries its goals and their progress; the move result its direction and result.
    /// </summary>
    [Test]
    public async ValueTask LabyrinthMissionAndMoveHaveTheirFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var view = new LabyrinthRoomsBabylonPlugIn(player);

        await view.ShowMissionAsync(new LabyrinthMissionView(LabyrinthAreaType.MainMission, 0, [new LabyrinthGoalProgress(LabyrinthGoalKind.Kill, 15, 3), new LabyrinthGoalProgress(LabyrinthGoalKind.Damage, 1_500_000, 20_000)], false)).ConfigureAwait(false);
        var mission = new LabyrinthRoomMissionRef(output.ToArray());
        Assert.That((mission.Header.SubCode, mission.AreaType, mission.MissionIndex, mission.Completed, mission.GoalCount), Is.EqualTo(((byte)0xB7, (byte)4, (byte)0, false, (byte)2)));
        Assert.That((mission[1].Kind, mission[1].Target, mission[1].Progress), Is.EqualTo(((byte)3, 1_500_000u, 20_000u)));

        output.SetLength(0);
        await view.ShowMoveResultAsync(LabyrinthDirection.Right, LabyrinthMoveResult.MissionNotDone).ConfigureAwait(false);
        var result = new LabyrinthRoomMoveResult(output.ToArray());
        Assert.That((result.Header.SubCode, result.Direction, result.Result), Is.EqualTo(((byte)0xB9, (byte)3, (byte)2)));
    }
}
