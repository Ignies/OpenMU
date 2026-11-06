// <copyright file="WorldSystemsPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView.World;
using MiningResultPacket = MUnique.OpenMU.Network.Packets.Babylon.MiningResult;

/// <summary>
/// Tests the packets of the world systems of the Babylon protocol: mining, soul converter, ranking, hunting record,
/// season pass and helper plus. A Season 6 client gets none of them.
/// </summary>
[TestFixture]
public class WorldSystemsPacketsTest
{
    /// <summary>
    /// Only the Babylon client gets the mining view; a Season 6 client has none and is told by a message instead.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheMiningView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, MiningBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>(), Is.InstanceOf<MiningBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IMiningViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The mining result carries the spot, the result, the stage and the jewels.
    /// </summary>
    [Test]
    public async ValueTask MiningResultHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var spot = new NonPlayerCharacter(null!, new MonsterDefinition { Number = 607 }, null!) { Id = 0x1234 };

        await new MiningBabylonPlugIn(player).ShowMiningResultAsync(spot, GameLogic.Mining.MiningResult.Collected, 5, 125, 1000).ConfigureAwait(false);

        var data = output.ToArray();
        Assert.That(data, Has.Length.EqualTo(MiningResultPacket.Length));
        var packet = new MiningResultPacket(data);
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xA3)));
        Assert.That((packet.NpcId, packet.Result, packet.Stage), Is.EqualTo(((ushort)0x1234, (byte)3, (byte)5)));
        Assert.That((packet.ItemCount, packet.BonusCount), Is.EqualTo(((ushort)125, (ushort)1000)));
    }
}
