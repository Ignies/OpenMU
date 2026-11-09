// <copyright file="GuideQuestPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.GuideQuests;
using MUnique.OpenMU.GameLogic.Rewards;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Quest;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView.Quest;
using MUnique.OpenMU.Network.Packets.Babylon;

/// <summary>
/// Tests the guide quest packets of the Babylon protocol: C1 EE C0 and C1 EE C1.
/// </summary>
[TestFixture]
public class GuideQuestPacketsTest
{
    /// <summary>
    /// The state carries the step, its objective and progress, its spot, its rewards and its name.
    /// </summary>
    [Test]
    public async ValueTask StateIsSentAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var step = new GuideQuestStep
        {
            Number = 14,
            Name = "Hunt Lorencia Spider",
            Chapter = 1,
            MinimumLevel = 2,
            Objective = GuideQuestObjective.HuntMonster,
            Target = 3,
            Count = 3,
            Map = 0,
            X = 193,
            Y = 126,
            Gate = 17,
            Experience = 300,
            Money = 2000,
            Items = [new RewardItem { Group = 14, Number = 373, Count = 1 }, new RewardItem { Group = 13, Number = 146, Count = 2 }],
        };

        await new GuideQuestBabylonPlugIn(player).ShowGuideQuestAsync(new GuideQuestView(GuideQuestStatus.Active, step, 2, 164, 1), GuideQuestResult.Accepted).ConfigureAwait(false);

        var packet = new GuideQuestState(output.ToArray());
        Assert.That((packet.Header.Code, packet.Header.SubCode), Is.EqualTo(((byte)0xEE, (byte)0xC0)));
        Assert.That(output.Length, Is.EqualTo(GuideQuestStateRef.GetRequiredSize(2)));
        Assert.That((packet.Status, packet.Result), Is.EqualTo(((byte)2, (byte)1)));
        Assert.That((packet.StepNumber, packet.StepIndex, packet.StepCount, packet.Chapter), Is.EqualTo(((ushort)14, (ushort)2, (ushort)164, (byte)1)));
        Assert.That((packet.Objective, packet.MinimumLevel, packet.Target, packet.TargetNumber), Is.EqualTo(((byte)2, (ushort)2, 3u, (ushort)0)));
        Assert.That((packet.RequiredCount, packet.Progress), Is.EqualTo(((ushort)3, (ushort)1)));
        Assert.That((packet.MapNumber, packet.X, packet.Y, packet.Gate), Is.EqualTo(((ushort)0, (byte)193, (byte)126, (ushort)17)));
        Assert.That((packet.Experience, packet.Money), Is.EqualTo((300u, 2000u)));
        Assert.That(packet.Name, Is.EqualTo("Hunt Lorencia Spider"));
        Assert.That(packet.RewardItemCount, Is.EqualTo(2));
        Assert.That((packet[1].Group, packet[1].Number, packet[1].Count), Is.EqualTo(((byte)13, (ushort)146, (byte)2)));
    }

    /// <summary>
    /// Without a step (a finished chain) the state carries only its status.
    /// </summary>
    [Test]
    public async ValueTask StateWithoutStepAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new GuideQuestBabylonPlugIn(player).ShowGuideQuestAsync(new GuideQuestView(GuideQuestStatus.Finished, null, 0, 164, 0), GuideQuestResult.Completed).ConfigureAwait(false);

        var packet = new GuideQuestState(output.ToArray());
        Assert.That((packet.Status, packet.Result, packet.StepNumber, packet.StepCount, packet.RewardItemCount), Is.EqualTo(((byte)5, (byte)2, (ushort)0, (ushort)164, (byte)0)));
    }

    /// <summary>
    /// Only the Babylon client gets the guide quest view.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsTheView()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        player.GameContext.PlugInManager.RegisterPlugIn<IViewPlugIn, GuideQuestBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IGuideQuestViewPlugIn>(), Is.InstanceOf<GuideQuestBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IGuideQuestViewPlugIn>(), Is.Null);
    }

    /// <summary>
    /// The feature has bit 18 (muquest), and the fork carries it.
    /// </summary>
    [Test]
    public void FeatureIsCarried()
    {
        Assert.That(ForkFeatures.BitOf("muquest"), Is.EqualTo(1u << 18));
        Assert.That(ForkInfo.Has("muquest"), Is.True);
    }
}
