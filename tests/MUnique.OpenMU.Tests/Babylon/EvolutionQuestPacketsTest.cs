// <copyright file="EvolutionQuestPacketsTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests.Babylon;

using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.GameLogic.Views.Quest;
using MUnique.OpenMU.GameServer;
using MUnique.OpenMU.GameServer.RemoteView.Character;
using MUnique.OpenMU.GameServer.RemoteView.Quest;
using MUnique.OpenMU.Network.Packets.Babylon;

/// <summary>
/// Tests the evolution quest packets of the Babylon protocol: C1 EE C4 and C1 EE 13.
/// </summary>
[TestFixture]
public class EvolutionQuestPacketsTest
{
    /// <summary>
    /// The state goes out with the next evolution, the flags, the level key and the required level.
    /// </summary>
    [Test]
    public async ValueTask StateIsSentAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);
        var state = new GameLogic.EvolutionQuests.EvolutionQuestState(ClassEvolution.Fourth, 800, ProgressionLevelKey.Season21Total, true, true);

        await new EvolutionQuestStateBabylonPlugIn(player).ShowEvolutionQuestStateAsync(state).ConfigureAwait(false);

        var packet = new EvolutionQuestState(output.ToArray());
        Assert.That(output.Length, Is.EqualTo(EvolutionQuestState.Length));
        Assert.That(packet.NextEvolution, Is.EqualTo(3));
        Assert.That(packet.RuleMet, Is.True);
        Assert.That(packet.ExperienceHeld, Is.True);
        Assert.That(packet.LevelKey, Is.EqualTo((byte)ProgressionLevelKey.Season21Total));
        Assert.That(packet.RequiredLevel, Is.EqualTo(800));
    }

    /// <summary>
    /// A class without an evolution quest sends 0xFF as the next evolution.
    /// </summary>
    [Test]
    public async ValueTask NoEvolutionIsSentAsFfAsync()
    {
        var (player, output) = CastleSiegeRemoteViewTestHelper.CreatePlayer(BabylonClient.Version);

        await new EvolutionQuestStateBabylonPlugIn(player).ShowEvolutionQuestStateAsync(default).ConfigureAwait(false);

        Assert.That(new EvolutionQuestState(output.ToArray()).NextEvolution, Is.EqualTo(0xFF));
    }

    /// <summary>
    /// The feature has bit 24, and the fork carries it.
    /// </summary>
    [Test]
    public void FeatureBitIsAppended()
    {
        Assert.That(ForkFeatures.BitOf("evolution_quests"), Is.EqualTo(1u << 24));
        Assert.That(ForkFeatures.Available & (1u << 24), Is.Not.Zero);
    }

    /// <summary>
    /// Only the Babylon client gets the plugins; a Season 6 client has none of them.
    /// </summary>
    [Test]
    public void OnlyTheBabylonClientGetsThePlugIns()
    {
        var (player, _) = CastleSiegeRemoteViewTestHelper.CreatePlayer();
        var manager = player.GameContext.PlugInManager;
        manager.RegisterPlugIn<IViewPlugIn, EvolutionQuestStateBabylonPlugIn>();
        manager.RegisterPlugIn<IViewPlugIn, ClassEvolvedBabylonPlugIn>();

        player.ClientVersion = BabylonClient.Version;
        Assert.That(player.ViewPlugIns.GetPlugIn<IEvolutionQuestStatePlugIn>(), Is.InstanceOf<EvolutionQuestStateBabylonPlugIn>());
        Assert.That(player.ViewPlugIns.GetPlugIn<IClassEvolvedPlugIn>(), Is.InstanceOf<ClassEvolvedBabylonPlugIn>());

        player.ClientVersion = new(6, 3, Network.PlugIns.ClientLanguage.English);
        Assert.That(player.ViewPlugIns.GetPlugIn<IEvolutionQuestStatePlugIn>(), Is.Null);
        Assert.That(player.ViewPlugIns.GetPlugIn<IClassEvolvedPlugIn>(), Is.Null);
    }
}
