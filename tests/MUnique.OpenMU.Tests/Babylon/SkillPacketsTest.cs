// <copyright file="SkillPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView.Character;
using MUnique.OpenMU.Network.Packets.Babylon;

/// <summary>
/// Tests the skill packets of the Babylon protocol (EE 40-48).
/// </summary>
[TestFixture]
public class SkillPacketsTest
{
    /// <summary>
    /// The Babylon client gets the wide skill list, master list and the tree; a Season 6 client keeps its plugins and has no tree.
    /// </summary>
    [Test]
    public void BabylonClientGetsTheSkillViewPlugIns()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, SkillListViewPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, SkillListViewBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, UpdateMasterSkillsPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, UpdateMasterSkillsBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, EnhanceTreeBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<ISkillListViewPlugIn>(), Is.InstanceOf<SkillListViewBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IUpdateMasterSkillsPlugIn>(), Is.InstanceOf<UpdateMasterSkillsBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IEnhanceTreeViewPlugIn>(), Is.InstanceOf<EnhanceTreeBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<ISkillListViewPlugIn>()?.GetType(), Is.EqualTo(typeof(SkillListViewPlugIn)));
        Assert.That(player.ViewPlugIns.GetPlugIn<IUpdateMasterSkillsPlugIn>()?.GetType(), Is.EqualTo(typeof(UpdateMasterSkillsPlugIn)));
        Assert.That(player.ViewPlugIns.GetPlugIn<IEnhanceTreeViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The result of an added tree point carries the result, the node, its level and the free points.
    /// </summary>
    [Test]
    public async ValueTask TreePointResultHasItsFieldsAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new EnhanceTreeBabylonPlugIn(player).ShowEnhanceTreePointResultAsync(EnhanceTreePointResult.MaximumReached, 1003, 15).ConfigureAwait(false);

        var data = output.ToArray();
        Assert.That(data, Has.Length.EqualTo(EnhanceTreeAddPointResult.Length));
        var packet = new EnhanceTreeAddPointResult(data);
        Assert.That(packet.Header.Code, Is.EqualTo(0xEE));
        Assert.That(packet.Header.SubCode, Is.EqualTo(0x42));
        Assert.That(packet.Result, Is.EqualTo((byte)EnhanceTreePointResult.MaximumReached));
        Assert.That(packet.SkillNumber, Is.EqualTo(1003));
        Assert.That(packet.Level, Is.EqualTo(15));
        Assert.That(packet.FreePoints, Is.Zero);
    }
}
