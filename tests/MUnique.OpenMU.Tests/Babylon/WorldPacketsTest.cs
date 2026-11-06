// <copyright file="WorldPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.EliteZones;
using MUnique.OpenMU.GameLogic.FieldBosses;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MUnique.OpenMU.Network.Packets.Babylon;
using FieldBossStatePacket = MUnique.OpenMU.Network.Packets.Babylon.FieldBossState;

/// <summary>
/// Tests the world packets of the Babylon protocol: the elite zones and the field boss state.
/// </summary>
[TestFixture]
public class WorldPacketsTest
{
    /// <summary>
    /// Only the Babylon client gets the views; a Season 6 client has none.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheWorldViews()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, EliteZonesBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, FieldBossStateBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IEliteZonesViewPlugIn>(), Is.InstanceOf<EliteZonesBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IFieldBossStateViewPlugIn>(), Is.InstanceOf<FieldBossStateBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IEliteZonesViewPlugIn>(), Is.Null);
        Assert.That(player.ViewPlugIns.GetPlugIn<IFieldBossStateViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The zones packet carries every zone of the map with its area and entry flag.
    /// </summary>
    [Test]
    public async ValueTask EliteZonesHaveTheirFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var zone = new EliteZoneDefinition { MapNumber = 142, Element = EliteZoneElement.Darkness, EntranceX = 88, EntranceY = 231, X1 = 77, Y1 = 221, X2 = 103, Y2 = 239 };

        await new EliteZonesBabylonPlugIn(player).ShowEliteZonesAsync(142, [(zone, true)]).ConfigureAwait(false);

        var packet = new EliteZones(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xA0)));
        Assert.That(packet.MapNumber, Is.EqualTo(142));
        Assert.That(packet.ZoneCount, Is.EqualTo(1));
        var shown = packet[0];
        Assert.That((shown.Element, shown.EntranceX, shown.EntranceY), Is.EqualTo(((byte)5, (byte)88, (byte)231)));
        Assert.That((shown.X1, shown.Y1, shown.X2, shown.Y2), Is.EqualTo(((byte)77, (byte)221, (byte)103, (byte)239)));
        Assert.That(shown.CanEnter, Is.EqualTo(1));
    }

    /// <summary>
    /// The boss state packet carries the map, the boss, its status, health and timer.
    /// </summary>
    [Test]
    public async ValueTask FieldBossStateHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new FieldBossStateBabylonPlugIn(player).ShowFieldBossStateAsync(122, 794, FieldBossStatus.Engaged, 42, 300).ConfigureAwait(false);

        var data = output.ToArray();
        Assert.That(data, Has.Length.EqualTo(FieldBossStatePacket.Length));
        var packet = new FieldBossStatePacket(data);
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xA1)));
        Assert.That((packet.MapNumber, packet.BossNumber), Is.EqualTo(((ushort)122, (ushort)794)));
        Assert.That((packet.Status, packet.HealthPercent, packet.SecondsLeft), Is.EqualTo(((byte)2, (byte)42, 300u)));
    }
}
